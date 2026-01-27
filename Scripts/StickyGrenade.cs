using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class StickyGrenade : NetworkBehaviour
{

    [SerializeField] private float destroyDelay = 3f;

    private bool stuck = false;
    private Rigidbody rb;
    private NetworkTransform netTransform;
    private ulong ownerId; 

    private Transform followTarget;
    private Vector3 followOffset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        netTransform = GetComponent<NetworkTransform>();
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
        {
            netTransform.enabled = false;
        }

        Invoke(nameof(Explode), destroyDelay);

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
        {
            netTransform.enabled = false;
        }

  
        if (targetNetId != 0 &&
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetId, out var targetObj))
        {
            followTarget = targetObj.transform;
        }
    }

    private void Explode()
    {
        if (!IsServer) return;

      
        if (ExplosionHelper.Instance != null)
            ExplosionHelper.Instance.TriggerExplosionServerRpc(transform.position, OwnerClientId);

        Destroy(gameObject, 0.1f);
    }
}
