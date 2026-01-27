using UnityEngine;
using Unity.Netcode;

public class CollisionGrenade : NetworkBehaviour
{
    [SerializeField] private float destroyDelay = 0.1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;

        if (ExplosionHelper.Instance != null)
            ExplosionHelper.Instance.TriggerExplosionServerRpc(transform.position, OwnerClientId);

        Destroy(gameObject, destroyDelay);
    }
}
