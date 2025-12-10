using Fusion;
using Fusion.Addons.KCC;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(GroundCheck), typeof(KCC))]
public class MovementController : NetworkBehaviour
{
    private KCC kcc;

    [SerializeField] private Animator _animator;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5.5f;
    [SerializeField] private float runSpeed = 7.7f;

    [Networked] private NetworkBool IsWalking { get; set; }
    [Networked] private NetworkBool IsRunning { get; set; }
    [Networked] private float WalkingZ { get; set; }
    [Networked] private float WalkingX { get; set; }

    private void Awake()
    {
        kcc = GetComponent<KCC>();
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInfoData input))
        {
            UpdateAnimations(input);
            Movement(input);
        }

        ApplyAnimations();
    }

    private void UpdateAnimations(NetworkInfoData input)
    {
        if (HasInputAuthority)
        {
            IsWalking = input.move.magnitude > 0.01f;
            IsRunning = input.buttons.IsSet(NetworkInfoData.BUTTON_RUN);
            WalkingZ = input.move.y;
            WalkingX = input.move.x;
        }
    }

    private void ApplyAnimations()
    {
        if (_animator != null)
        {
            _animator.SetBool("IsWalking", IsWalking);
            _animator.SetBool("IsRunning", IsRunning);
            _animator.SetFloat("WalkingZ", WalkingZ);
            _animator.SetFloat("WalkingX", WalkingX);
        }
    }

    #region Movimiento

    private void Movement(NetworkInfoData input)
    {
        Vector3 moveDirection = transform.forward * input.move.y + transform.right * input.move.x;
        Vector3 horizontalVelocity = moveDirection.normalized * Speed(input);
        kcc.SetKinematicVelocity(new Vector3(horizontalVelocity.x, 0, horizontalVelocity.z));
    }

    private float Speed(NetworkInfoData input)
    {
        return input.move.y < 0 || input.move.x != 0 ? walkSpeed : input.buttons.IsSet(NetworkInfoData.BUTTON_RUN) ? runSpeed : walkSpeed;
    }

    #endregion
}