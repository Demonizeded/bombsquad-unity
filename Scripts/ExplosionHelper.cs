using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class ExplosionHelper : NetworkBehaviour
{
    public static ExplosionHelper Instance;

    public GameObject explosionVfxPrefab;
    public AudioClip explosionClip;
    public float explosionRadius = 5f;
    public int damageAmount = 100;
    public float explosionForce = 50f;

    private void Awake()
    {
       
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
    
        if (IsServer && !GetComponent<NetworkObject>().IsSpawned)
        {
            GetComponent<NetworkObject>().Spawn();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TriggerExplosionServerRpc(Vector3 pos, ulong attackerid)
    {
        PlayExplosionClientRpc(pos);
        ApplyExplosionDamage(pos, explosionRadius, damageAmount, attackerid);
    }

    [ClientRpc]
    public void PlayExplosionClientRpc(Vector3 pos)
    {
        if (explosionVfxPrefab != null)
        {
            var fx = Instantiate(explosionVfxPrefab, pos, Quaternion.identity);
            var ps = fx.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(fx, ps != null ? ps.main.duration : 3f);
        }

        if (explosionClip != null)
            AudioSource.PlayClipAtPoint(explosionClip, pos);

        Debug.Log($" Explosion sound spawned on {pos}");
    }

    private void ApplyExplosionDamage(Vector3 pos, float radius, int dmg, ulong attackerId)
    {
        Collider[] hits = Physics.OverlapSphere(pos, radius);
        foreach (var hit in hits)
        {
            var player = hit.GetComponent<PlayerHealth>();
            if (player != null) { 
                
                
                player.TakeDamage(dmg, attackerId);
            }
           
        }

    }
}
