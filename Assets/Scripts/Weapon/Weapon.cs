using Fusion;
using UnityEngine;

public abstract class Weapon : NetworkBehaviour
{
    [SerializeField] protected ShootingType shootingType;

    public ShootingType ShootType
    {
        get => shootingType;
    }

    [Header("Raycast Settings")]
    [SerializeField] protected LayerMask hitLayers;
    [SerializeField] protected Transform cameraPos;

    [Header("Weapon Stats")]
    [SerializeField] protected int damage;
    [SerializeField] protected float fireRate;
    [SerializeField] protected float range;
    [SerializeField] protected int magSize;
    [SerializeField] protected int ammoInStock;
    [SerializeField] protected int maxAmmoCapacity;
    [SerializeField] protected float reloadTime;
    protected RaycastHit hitInfo;

    public abstract void RigiBodyShoot();

    public virtual void RpcRaycastShoot()
    {

    }

    public virtual void RpcReload()
    {

    }

    public enum ShootingType
    {
        RigidBody,
        Raycast
    }



}
