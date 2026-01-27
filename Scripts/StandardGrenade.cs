using UnityEngine;
using Unity.Netcode;

public class StandardGrenade : NetworkBehaviour
{
    public float delay = 3f;

    private void Start()
    {
        if (IsServer)
            Invoke(nameof(Explode), delay);
    }

    private void Explode()
    {
        if (ExplosionHelper.Instance != null)
            ExplosionHelper.Instance.TriggerExplosionServerRpc(transform.position, OwnerClientId);

        Destroy(gameObject, 0.1f);
    }
}
