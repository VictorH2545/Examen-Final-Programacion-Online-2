using Fusion;
using System.Threading.Tasks;
using UnityEngine;


public class Bullet : NetworkBehaviour
{
    [SerializeField] private float speed = 100f;
    [SerializeField] private float lifetime = 1f;
    [SerializeField] public int damage;
    [SerializeField] private NetworkPrefabRef decalPrefab;
    [SerializeField] private NetworkPrefabRef particlesPrefab;
    private Rigidbody rb;

    [Networked] public PlayerRef OwnerBullet { get; set; } 
    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();
        if (Object.HasStateAuthority) rb.linearVelocity = speed * transform.forward;
        DespawnAfterTime();
    }

    private async void DespawnAfterTime()
    {
        await Task.Delay((int)(lifetime * 1000));
        if (Object != null && Object.HasStateAuthority) Runner.Despawn(Object);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SpawnParticles(Vector3 position)
    {
        if (particlesPrefab.IsValid)
        {
            var particlesObj = Runner.Spawn(particlesPrefab, position, Quaternion.identity); // Spawn the particle system at the contact point
            if (particlesObj != null)
            {
                var ps = particlesObj.GetComponent<ParticleSystem>();
                if (ps != null) ps.Play();
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_SpawnDecal(Vector3 position, Quaternion rotation)
    {
        if (decalPrefab.IsValid)
        {
            var decalObj = Runner.Spawn(decalPrefab, position, rotation); // Spawn the decal at the contact point
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    private void RPC_Damage(PlayerRef jugador, NetworkObject enemigo, int daño)
    {
        if (enemigo != null && enemigo.TryGetComponent<Health>(out Health health))
        {
            health.Rpc_TakeDamage(daño, jugador);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!ValidCollision()) return;

        ContactPoint hit = collision.GetContact(0);

        if (collision.gameObject.CompareTag("Target"))
        {
            if (collision.gameObject.TryGetComponent<Health>(out Health health))
            {
                RPC_Damage(OwnerBullet, health.Object, damage);

                RPC_SpawnParticles(hit.point);
                Runner.Despawn(Object);
            }

            else if (collision.gameObject.CompareTag("Wall"))
            {
                Vector3 spawnPos = hit.point + hit.normal * 0.01f;
                Quaternion decalRotation = Quaternion.LookRotation(-hit.normal);

                RPC_SpawnDecal(spawnPos, decalRotation);
                Runner.Despawn(Object);
            }
        }
    }

    private bool ValidCollision()
    {
        return Object != null && Object.HasStateAuthority;
    }


}