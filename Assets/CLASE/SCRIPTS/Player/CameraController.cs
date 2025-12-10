using Fusion;
using Fusion.Addons.KCC;
using System;
using UnityEngine;
using UnityEngine.Serialization;


public class CameraController : NetworkBehaviour
{
    [Header("Camera Settings")]
    [SerializeField]
    private Transform player;

    [SerializeField] private float mouseSensitivity = 1;

    [FormerlySerializedAs("smooth")]
    [SerializeField]
    private float smoothnes;

    [SerializeField] private float maxAngleY = 80;
    [SerializeField] private float minAngleY = -80;

    private Vector2 camVelociy;
    private Vector2 smoothVelocity;

    [Header("Blob Movement")]
    [SerializeField] private float walkingSpeed = 1f;

    [SerializeField, Range(0, 0.1f)] private float walkingAmplitude = 0.015f; // Que tanto se mueve hacia los lados al caminar
    [SerializeField, Range(0, 0.1f)] private float runningAmplitude = 0.015f; // Que tanto se mueve hacia los lados al correr
    [SerializeField, Range(0, 15)] private float walkingFrequency = 10.0f; // La frecuencia con la que se mueve al caminar
    [SerializeField, Range(10, 20)] private float runningFrequency = 18f; // La frecuencia con la que se mueve al correr
    [SerializeField] private float resetPosSpeed = 3.0f; // Cuando dejas de moverte que regrese al centro
    [SerializeField] private float toggleSpeed = 3.0f; // 

    private Vector3 startPos; // Posicion inicial de la cabeza , el centro
    [SerializeField] private bool moveHead;
    private Vector2 head;
    private InputManager inputManager;

    [Networked] private float CamY { get; set; } // Rotación vertical (pitch)
    [Networked] private float CamX { get; set; } // Rotación horizontal (yaw)
    [Networked] private Vector2 CamVelocity { get; set; } // Velocidad acumulada de la cámara

    private KCC kcc;


    private void Awake()
    {
        startPos = transform.localPosition;

        if (player == null)
        {
            player = FindFirstObjectByType<MovementController>().transform;
        }

        kcc = player.GetComponent<KCC>();
    }
    public override void Spawned()
    {
        Transform body = player.Find("Body");

        if (body == null)
        {
            Debug.LogWarning("Body transform not found on player");
            return;
        }

        if (HasInputAuthority)
        {
            ConfigureLocalPlayer(body);
        }
        else
        {
            ConfigureRemotePlayer(body);
        }
    }

    private void ConfigureLocalPlayer(Transform body)
    {
        int localPlayerLayer = LayerMask.NameToLayer("HostPlayer");
        SetLayerRecursively(body.gameObject, localPlayerLayer);

        Camera localCamera = GetComponent<Camera>();
        if (localCamera != null)
        {
            int hiddenLayerMask = ~(1 << localPlayerLayer);
            localCamera.cullingMask = hiddenLayerMask;
        }
    }

    private void ConfigureRemotePlayer(Transform body)
    {
        GetComponent<Camera>().enabled = false;
        GetComponent<AudioListener>().enabled = false;
        Debug.Log("Configured remote player camera and audio listener.");
        int remotePlayerLayer = LayerMask.NameToLayer("ClientPlayer");
        SetLayerRecursively(body.gameObject, remotePlayerLayer);
        Debug.Log("Set remote player body layer to ClientPlayer.");
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;

        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public override void Render()
    {
        transform.localRotation = Quaternion.AngleAxis(-CamY, Vector3.right);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInfoData input))
        {
            RotateCamera(input);
        }

        Quaternion lookRotation = Quaternion.AngleAxis(CamX, Vector3.up);
        kcc.SetLookRotation(lookRotation);
        player.rotation = lookRotation;
    }

    private void RotateCamera(NetworkInfoData input)
    {
        // Usa input.rotation en lugar de inputManager.GetMouseDelta()
        Vector2 rawFrameVelocity = Vector2.Scale(input.rotation, Vector2.one * mouseSensitivity);
        smoothVelocity = Vector2.Lerp(smoothVelocity, rawFrameVelocity, 1 / smoothnes);

        Vector2 currentVelocity = CamVelocity;
        currentVelocity += smoothVelocity;
        currentVelocity.y = Mathf.Clamp(currentVelocity.y, minAngleY, maxAngleY);
        CamVelocity = currentVelocity;

        CamY = CamVelocity.y;
        CamX = CamVelocity.x;

        transform.localRotation = Quaternion.AngleAxis(-CamVelocity.y, Vector3.right);
    }
    #region Blob Movement
    private void BlobMove()
    {
        if (!inputManager.IsMoveInputPressed()) // Si no presiono ningun input
        {
            return; // termina el metodo
        }

        if (inputManager.IsMoveInputPressed()) // Pregunto si me estoy moviendo
        {
            if (inputManager.IsMovingBackwards() || inputManager.IsMovingOnXAxis()) // Me estoy moviendo hacia atras o hacia los lados?
            {
                transform.localPosition += FootStepMotion();
            }
            else //  Entonces me muevo hacia adelante
            {
                if (inputManager.WasRunInputPressed()) // Estoy corriendo?
                {
                    transform.localPosition += RunningFootStepMotion();
                }
                else // Estoy caminando
                {
                    transform.localPosition += FootStepMotion();
                }
            }
        }

        if (inputManager.IsMoveInputPressed())
        {
            transform.localPosition += inputManager.IsMovingBackwards() || inputManager.IsMovingOnXAxis() ? FootStepMotion() : inputManager.WasRunInputPressed() ? RunningFootStepMotion() : FootStepMotion();
        }



    }

    private void ResetPosition()
    {
        if (transform.localPosition == startPos) return; // Si la camara ya esta en la pos inicial, no hace nada
        transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, resetPosSpeed * Time.deltaTime);
    }

    private Vector3 FootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * walkingFrequency) * walkingAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * walkingFrequency / 2) * walkingAmplitude * 2 * walkingSpeed;
        return pos;
    }


    private Vector3 RunningFootStepMotion()
    {
        Vector3 pos = Vector3.zero;
        pos.y = Mathf.Sin(Time.time * runningFrequency) * runningAmplitude * walkingSpeed;
        pos.x = Mathf.Cos(Time.time * runningFrequency / 2) * runningAmplitude * 2 * walkingSpeed;
        return pos;
    }

    #endregion Blob movement

}