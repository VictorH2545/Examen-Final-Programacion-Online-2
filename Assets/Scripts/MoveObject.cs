using UnityEngine;
using Fusion;

public class MoveObject : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float verticalDistance = 3f;

    [Networked] private bool IsInUpPosition { get; set; }

    private Vector3 _downPosition;
    private Vector3 _upPosition;

    public override void Spawned()
    {
        InitializePositions();
    }

    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority)
        {
            MoveToTargetPosition();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_TogglePosition()
    {
        IsInUpPosition = !IsInUpPosition;
    }

    private void InitializePositions()
    {
        _downPosition = transform.position;
        _upPosition = _downPosition + Vector3.up * verticalDistance;
    }

    private void MoveToTargetPosition()
    {
        Vector3 targetPosition = IsInUpPosition ? _upPosition : _downPosition;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Runner.DeltaTime
        );
    }
}