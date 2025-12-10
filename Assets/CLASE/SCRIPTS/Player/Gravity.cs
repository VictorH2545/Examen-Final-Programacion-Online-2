using UnityEngine;
using Fusion;
using Fusion.Addons.KCC;

public class Gravity : NetworkBehaviour
{
    [SerializeField] private float gravity = 15f;
    private KCC kcc;

    [Networked] private float verticalVelocity { get; set; }

    void Awake()
    {
        kcc = GetComponent<KCC>();
    }

    public override void FixedUpdateNetwork()
    {
        if (kcc == null) return;

       
        if (kcc.Data.IsGrounded)
        {
            verticalVelocity = 0f;
        }
        else
        {
           
            verticalVelocity += gravity * Runner.DeltaTime;
        }

       
        kcc.AddExternalDelta(Vector3.down * verticalVelocity * Runner.DeltaTime);
    }
}