using UnityEngine;
using Unity.Netcode;

public class FragGrenade : NetworkBehaviour
{
    public GameObject explosionVfx; 
    public AudioClip explosionSfx;


    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;



        Explode();
    }

    private void Explode()
    {

        var helper = ExplosionHelper.Instance;
        if (helper != null)
            helper.TriggerExplosionServerRpc(transform.position, OwnerClientId);


        if (NetworkObject != null && NetworkObject.IsSpawned) NetworkObject.Despawn(true);
        else Destroy(gameObject);
    }
}
