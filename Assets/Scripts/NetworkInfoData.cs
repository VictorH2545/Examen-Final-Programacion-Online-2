using Fusion;
using UnityEngine;
// Este struct es la iformacion que se envia a los demas clientes
public struct NetworkInfoData : INetworkInput // Implementa la interfaz INetworkInput para enviar datos de entrada a otros clientes
{
    /// <summary>
    /// Una forma eficiente de enviar muchos botones usando bits
    /// Se pueden guardar hasta 32 botones en un solo entero usando bits
    /// Es como tener una lista de indices para cada boton
    /// Un bool requiere 1 bit
    /// </summary>
    public NetworkButtons buttons; 

    public Vector3 move; // Movimiento del jugador
    public Vector2 rotation; // Rotación del jugador

    public const byte BUTTON_FIRE = 0; // Botón de disparo
    public const byte BUTTON_RUN = 1; // Botón de correr
    public const byte BUTTON_RELOAD = 2; // Botón de recargar
    public const byte BUTTON_INTERACT = 3;
}
