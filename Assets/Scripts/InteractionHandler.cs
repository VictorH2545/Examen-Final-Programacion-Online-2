using Fusion;
using UnityEngine;

public class InteractHandler : NetworkBehaviour
{
    [Header("Interact Config")]
    [SerializeField] private float interactRange = 5f;
    [SerializeField] private LayerMask interactLayer;

    private Collider[] interactDetections = new Collider[3];

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasInputAuthority) return;

        if (GetInput(out NetworkInfoData input))
        {
            if (input.buttons.IsSet(NetworkInfoData.BUTTON_INTERACT)) ValidInteractions();
        }
    }

    private void ValidInteractions()
    {
        int numDetections = Physics.OverlapSphereNonAlloc(transform.position, interactRange, interactDetections, interactLayer);
        Debug.Log("Número de interacciones detectadas: " + numDetections);

        for (int i = 0; i < numDetections; i++)
        {
            if (interactDetections[i] == null) continue; // Si no hay colisión, continuar

            MoveObject moveObject = interactDetections[i].GetComponent<MoveObject>(); 
            if (moveObject != null) moveObject.RPC_TogglePosition();
            return;

        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}