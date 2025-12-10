using Fusion;
using System;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] private Weapon currentWeapon;
    private Action shoot;

    public override void FixedUpdateNetwork()
    {
        if (!HasInputAuthority) return;
        {
            if (GetInput(out NetworkInfoData input))
            {
                if (input.buttons.IsSet((NetworkInfoData.BUTTON_FIRE)))
                {
                    SwitchWeapon();
                }

                if (input.buttons.IsSet((NetworkInfoData.BUTTON_RELOAD)))
                {
                    Debug.Log("Recargando arma");
                    currentWeapon.RpcReload();
                }
            }
        }
    }

    private void SwitchWeapon()
    {
        switch (currentWeapon.ShootType)
        {
            case Weapon.ShootingType.RigidBody:
                currentWeapon.RigiBodyShoot();
                break;

            case Weapon.ShootingType.Raycast:
                currentWeapon.RpcRaycastShoot();
                break;
        }
    }

}
