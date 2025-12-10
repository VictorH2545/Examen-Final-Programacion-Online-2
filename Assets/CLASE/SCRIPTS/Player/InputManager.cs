
using UnityEngine;

public class InputManager : MonoBehaviour
{

    private static InputManager _instance = null;
    
    public static InputManager Instance { get => _instance; private set => _instance = value;}
    
    
    private PlayerControls playerControls;

    private void Awake()
    {
        if(_instance != null && _instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
            Debug.Log("Instance created");
        }
        playerControls = new PlayerControls();
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }
    
    private void OnDisable()
    {
        playerControls.Disable();
    }
    
    public Vector2 GetMoveInput() // Vector2 because is a 2D input (x,y)
    {
        return playerControls.Player.Move.ReadValue<Vector2>();
    }
    
    public bool IsMoveInputPressed() // revisa si se esta moviendo
    {
        return playerControls.Player.Move.IsPressed();
    }
    
    public bool WasRunInputPressed() // revisa si se esta corriendo
    {
        return playerControls.Player.Run.IsPressed();
    }
    
    public bool IsMovingBackwards() // revisa si se esta moviendo hacia atras
    {
        return playerControls.Player.Move.ReadValue<Vector2>().y < 0;
    }
    
    public bool IsMovingOnXAxis() // revisa si se esta moviendo en el eje x
    {
        return playerControls.Player.Move.ReadValue<Vector2>().x != 0;
    }
    
    public Vector2 GetMouseDelta() // revisa el movimiento del mouse
    {
        return playerControls.Player.Look.ReadValue<Vector2>();
    }

    public bool ShootInputPressed() // revisa si se presionó el botón de disparo
    {
        return playerControls.Player.Fire.IsPressed();
    }

    public bool ReloadInputPressed() // revisa si se presionó el botón de recarga
    {
        return playerControls.Player.Reload.WasPressedThisFrame();
    }

    public bool InteractInputPressed() // revisa si se presionó el botón de interactuar
    {
        return playerControls.Player.Interact.WasPressedThisFrame();
    }

}
