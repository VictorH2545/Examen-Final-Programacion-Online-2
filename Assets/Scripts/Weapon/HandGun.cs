using Fusion;
using UnityEngine;

public class HandGun : Weapon
{
    [SerializeField] private NetworkObject bulletPrefab; // Prefab de la bala a spawnear
    [SerializeField] private Transform firePoint; // Posición de la cámara del jugador

    private float nextTimeToFire = 0f; // Tiempo para el próximo disparo permitido

    [Networked] public PlayerRef OwnerGun { get; set; }

    public override void Spawned()
    {
        OwnerGun = Object.InputAuthority;
    }

    public override void RigiBodyShoot()
    {
        if (!Object.HasInputAuthority) return;

        if (Time.time >= nextTimeToFire)
        {
            RpcRequestRigibodyShoot(firePoint.position, firePoint.rotation);
            nextTimeToFire = Time.time + (1f / fireRate);
        }

    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)] // RpcTargets.StateAuthority se usa para spawnear objetos en el servidor
    private void RpcRequestRigibodyShoot(Vector3 pos, Quaternion rot, RpcInfo info = default)
    {
        NetworkObject _bullet = Runner.Spawn(bulletPrefab, pos, rot);

        if (_bullet.TryGetComponent(out Bullet bullet))
        {
            bullet.damage = damage;
            bullet.OwnerBullet = OwnerGun;

        }

    }


    [Rpc(RpcSources.InputAuthority, RpcTargets.All)] // RpcTargets.All se usa para objetos que no necesitan spawnear en el servidor
    public override void RpcRaycastShoot() // Disparo con raycast
    {

        if (Physics.Raycast(cameraPos.position, cameraPos.forward, out hitInfo, range, hitLayers))
        {
            Debug.Log("Has golpeado a: " + hitInfo.collider.name);
        }

        if (hitInfo.collider.TryGetComponent<Health>(out Health health))
        {
            health.Rpc_TakeDamage(damage,OwnerGun);
        }

    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public override void RpcReload() // Recarga del arma
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(cameraPos.position, cameraPos.forward * range);
    }
}
