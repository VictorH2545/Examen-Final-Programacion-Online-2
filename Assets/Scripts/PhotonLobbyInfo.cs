using TMPro;
using UnityEngine;

public class PhotonLobbyInfo : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI lobbyNameText;
    public TextMeshProUGUI playersCountText;
    private string sessionName;

    public void SetInfo(string lobbyName, string playerCount)
    {
        sessionName = lobbyName;
        lobbyNameText.text = lobbyName;
        playersCountText.text = playerCount;

        Debug.Log($"Lobby info set: {lobbyName} - {playerCount}");
    }

    public void JoinGame()
    {
        Debug.Log($"Join button clicked for session: {sessionName}");

        PhotonManager photonManager = FindFirstObjectByType<PhotonManager>();

        if (photonManager != null)
        {
            photonManager.JoinSession(sessionName);
        }
        else
        {
            Debug.LogError("PhotonManager not found in scene!");
        }
    }
}
