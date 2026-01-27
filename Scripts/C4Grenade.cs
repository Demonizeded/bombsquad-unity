using System.Collections;
using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Components;

public class C4Grenade : NetworkBehaviour
{
  
    public ParticleSystem ps;
    [SerializeField] private float destroyDelay = 0.2f;

    public NetworkVariable<int> id = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private GrenadeOwner grenadeOwner;
    private KeyCode idKey = KeyCode.None;
    private bool stuck = false;

    private ulong ownerId;
    private Rigidbody rb;
    private NetworkTransform netTransform;

    private Transform followTarget;
    private Vector3 followOffset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        netTransform = GetComponent<NetworkTransform>();
    }

    private void Start()
    {
        grenadeOwner = GetComponent<GrenadeOwner>();
        id.OnValueChanged += OnIdChanged;
        idKey = IdToKey(id.Value);
    }

    public override void OnNetworkSpawn()
    {
        ownerId = OwnerClientId;
    }

    private void LateUpdate()
    {
 
        if (stuck && followTarget != null)
        {
            transform.position = followTarget.TransformPoint(followOffset);
        }

   
        if (grenadeOwner != null &&
            grenadeOwner.ownerId.Value == NetworkManager.Singleton.LocalClientId &&
            Input.GetKeyDown(idKey))
        {
            ExplodeServerRpc();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (stuck || !IsServer) return;

        var hitObj = collision.gameObject;

     
        var player = hitObj.GetComponent<PlayerHealth>();
        if (player != null && player.OwnerClientId == ownerId)
            return;

     
        stuck = true;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

    
        var contact = collision.contacts[0];
        transform.position = contact.point;
        transform.rotation = Quaternion.LookRotation(-contact.normal);


        Collider col = GetComponent<Collider>();
        col.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("IgnorePlayerPhysics");


        followTarget = hitObj.transform;
        followOffset = hitObj.transform.InverseTransformPoint(contact.point);

        if (netTransform != null)
            netTransform.enabled = false;

     
        ulong targetNetId = hitObj.GetComponent<NetworkObject>()?.NetworkObjectId ?? 0;
        NotifyClientsStickClientRpc(targetNetId, transform.position, transform.rotation, followOffset);
    }

    [ClientRpc]
    private void NotifyClientsStickClientRpc(ulong targetNetId, Vector3 pos, Quaternion rot, Vector3 offset)
    {
        if (IsServer) return;

        stuck = true;
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Collider col = GetComponent<Collider>();
        col.enabled = false;
        gameObject.layer = LayerMask.NameToLayer("IgnorePlayerPhysics");

        transform.position = pos;
        transform.rotation = rot;
        followOffset = offset;

        if (netTransform != null)
            netTransform.enabled = false;

        if (targetNetId != 0 &&
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetId, out var targetObj))
        {
            followTarget = targetObj.transform;
        }
    }

    private void OnDestroy()
    {
        id.OnValueChanged -= OnIdChanged;
    }

    private void OnIdChanged(int oldValue, int newValue)
    {
        idKey = IdToKey(newValue);
        Debug.Log($"[C4] id changed from {oldValue} -> {newValue}, key = {idKey}");
    }

    private KeyCode IdToKey(int id)
    {
        return (KeyCode)((int)KeyCode.Alpha0 + id);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExplodeServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong sender = rpcParams.Receive.SenderClientId;

   
        if (grenadeOwner == null || grenadeOwner.ownerId.Value != sender)
        {
            Debug.LogWarning($"[C4] Unauthorized detonation attempt by {sender}");
            return;
        }

        Debug.Log($"[C4] Authorized detonation by {sender}");

 
        if (ExplosionHelper.Instance != null)
            ExplosionHelper.Instance.TriggerExplosionServerRpc(transform.position, OwnerClientId);
        else
            Debug.LogWarning("[C4] ExplosionHelper.Instance missing!");

   
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn(true);
        else
            Destroy(gameObject, destroyDelay);
    }
}
