using UnityEngine;
using Unity.Netcode;
using Unity.VisualScripting;

public class GrenadeThrower : NetworkBehaviour
{

    public Transform throwPoint;
    public float throwForce = 12f;
    public GameObject fragPrefab;
    public GameObject collisionPrefab;
    public GameObject stickyPrefab;
    public GameObject c4Prefab;
    public bool alrchose = false;
    public int nadescount = 6;
    

    private PlayerHealth ph;
    public int localC4Id = 0;

    private void Start()
    {
        ph = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0) && alrchose == true && nadescount !=0)
        {
            int type = ph != null ? ph.grenadeType.Value : 0;
            
            if (type == 3) localC4Id++;

            nadescount--;
            
          
            ThrowServerRpc(type, throwPoint.position, throwPoint.rotation, throwPoint.forward, localC4Id);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void ThrowServerRpc(int type, Vector3 pos, Quaternion rot, Vector3 forward, int c4id, ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;

        GameObject prefab = GetPrefabForType(type);
        if (prefab == null) return;

        var nade = Instantiate(prefab, pos, rot);

        var rb = nade.GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = forward * throwForce;

        var nob = nade.GetComponent<NetworkObject>();
        if (nob == null)
        {
            return;
        }

       
        

        var grenadeOwner = nade.GetComponent<GrenadeOwner>();
        if (grenadeOwner != null)
        {
            grenadeOwner.ownerId.Value = senderClientId;
        }
        nob.SpawnWithOwnership(senderClientId);
   
        if (type == 3)
        {
            var c4 = nade.GetComponent<C4Grenade>();
            if (c4 != null) c4.id.Value = c4id;
        }

    }

    [ServerRpc(RequireOwnership = false)]

    public void NadeAblitiesServerRpc(int aid)
    {
        switch (aid)
        {
            case 1://Gunner
                nadescount = 20;

            break;
        }
    }



    private GameObject GetPrefabForType(int type)
    {
        switch (type)
        {
            case 0: return fragPrefab;
            case 1: return collisionPrefab;
            case 2: return stickyPrefab;
            case 3: return c4Prefab;
            default: return fragPrefab;
        }
    }
}
