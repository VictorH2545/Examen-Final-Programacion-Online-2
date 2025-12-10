using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner runner;
    [SerializeField] private NetworkPrefabRef prefabPlayer;

    [Header("UI Settings")]
    [SerializeField] private GameObject container;
    [SerializeField] private GameObject lobbyUIPrefab;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    public event Action OnLobbyListUpdate;
    private int sessionCount;
    private List<SessionInfo> availableSessions = new List<SessionInfo>();
    private Dictionary<PlayerRef, NetworkObject> playerList = new Dictionary<PlayerRef, NetworkObject>();

    private bool isStartingGame = false; // Prevenir múltiples llamadas

    private void OnEnable()
    {
        OnLobbyListUpdate += UpdateSessionList;

        if (hostButton != null) hostButton.onClick.AddListener(StartGameAsHost);
        if (joinButton != null) joinButton.onClick.AddListener(SearchSessions);
    }

    private void OnDisable()
    {
        OnLobbyListUpdate -= UpdateSessionList;

        if (hostButton != null) hostButton.onClick.RemoveListener(StartGameAsHost);
        if (joinButton != null) joinButton.onClick.RemoveListener(SearchSessions);
    }

    private NetworkRunner CreateRunner()
    {
        if (runner != null)
        {
            Destroy(runner.gameObject);
            runner = null;
        }

        GameObject runnerObj = new GameObject("NetworkRunner");
        runner = runnerObj.AddComponent<NetworkRunner>();
        runner.name = "NetworkRunner";
        DontDestroyOnLoad(runnerObj);

        return runner;
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {

    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        if (SceneManager.GetActiveScene().name == "Game")
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (!playerList.ContainsKey(player))
            {
                Debug.Log($"Player Joined: {player.PlayerId}");
                Vector3 spawnPosition = new Vector3(3f, 0f, -30f);
                NetworkObject networkObject = runner.Spawn(prefabPlayer, spawnPosition, Quaternion.identity, player);
                playerList.Add(player, networkObject);
            }
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (playerList.ContainsKey(player))
        {
            if (playerList[player] != null) runner.Despawn(playerList[player]);
            playerList.Remove(player);
            Debug.Log($"Player left: {player.PlayerId}");
        }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInfoData data = new NetworkInfoData()
        {
            move = InputManager.Instance.GetMoveInput(),
            rotation = InputManager.Instance.GetMouseDelta(),
        };

        if (InputManager.Instance.WasRunInputPressed())
        {
            data.buttons.Set(NetworkInfoData.BUTTON_RUN, true);
        }

        if (InputManager.Instance.ShootInputPressed())
        {
            data.buttons.Set(NetworkInfoData.BUTTON_FIRE, true);
        }

        if (InputManager.Instance.ReloadInputPressed())
        {
            data.buttons.Set(NetworkInfoData.BUTTON_RELOAD, true);
        }

        if (InputManager.Instance.InteractInputPressed())
        {
            data.buttons.Set(NetworkInfoData.BUTTON_INTERACT, true);
        }

        input.Set(data);
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

        sessionCount = sessionList.Count;
        availableSessions = sessionList;

        OnLobbyListUpdate?.Invoke();
    }

    private async void StartGame(GameMode game, string sessionName)
    {

        isStartingGame = true; //proceso de inicio

        if (runner != null && runner.IsRunning)
        {
            await runner.Shutdown();
            await System.Threading.Tasks.Task.Delay(500);
        }

        runner = CreateRunner();
        runner.AddCallbacks(this);
        runner.ProvideInput = true;

        var sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        try
        {

            var sceneInfo = new NetworkSceneInfo();
            sceneInfo.AddSceneRef(SceneRef.FromIndex(1), LoadSceneMode.Single);

            await runner.StartGame(new StartGameArgs
            {
                GameMode = game,
                SessionName = sessionName,
                Scene = sceneInfo,
                PlayerCount = 2,
                CustomLobbyName = "MiPartida",
                SceneManager = sceneManager
            });

        }
        catch (Exception e)
        {
            Debug.LogError($"Error starting game: {e.Message}");
        }


    }

    public void StartGameAsHost()
    {
        if (isStartingGame) return; // Evitar múltiples llamadas
        string sessionName = RandomSession();
        StartGame(GameMode.Host, sessionName);
    }

    public async void SearchSessions()
    {

        if (runner != null && runner.IsRunning)
        {
            await runner.Shutdown();
            await System.Threading.Tasks.Task.Delay(300);
        }

        runner = CreateRunner();
        runner.AddCallbacks(this);
        runner.ProvideInput = false;

        try
        {
            await runner.JoinSessionLobby(SessionLobby.Custom, "MiPartida");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error: {e.Message}");
        }
    }

    public async void JoinSession(string sessionName)
    {

        if (runner != null && runner.IsRunning)
        {
            await runner.Shutdown();
            await System.Threading.Tasks.Task.Delay(500);
        }

        runner = CreateRunner();
        runner.AddCallbacks(this);
        runner.ProvideInput = true;

        var sceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        try
        {
            await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = sessionName,
                SceneManager = sceneManager,
                CustomLobbyName = "MiPartida"
            });
        }
        catch (Exception e)
        {
            Debug.LogError($"Error:{e.Message}");
        }
    }

    public string RandomSession()
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        string sessionName = "";

        for (int i = 0; i < 5; i++)
        {
            int randomChar = UnityEngine.Random.Range(0, chars.Length);
            sessionName += chars[randomChar];
        }

        Debug.Log($"Generated session name: {sessionName}");
        return sessionName;
    }

    #region UI

    public void UpdateSessionList()
    {

        DeleteLobbyFromCanvas();

        for (int i = 0; i < sessionCount; i++)
        {
            GameObject lobbyInfo = Instantiate(lobbyUIPrefab, container.transform);

            lobbyInfo.SetActive(true);

            string playerInfo = $"{availableSessions[i].PlayerCount}/{availableSessions[i].MaxPlayers}";

            PhotonLobbyInfo lobbyInfoComponent = lobbyInfo.GetComponent<PhotonLobbyInfo>();

            if (lobbyInfoComponent != null)
            {
                lobbyInfoComponent.SetInfo(availableSessions[i].Name, playerInfo);
            }
        }
    }

    public void DeleteLobbyFromCanvas()
    {
        if (container == null) return;

        int childCount = container.transform.childCount;

        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(container.transform.GetChild(i).gameObject);
        }
    }

    #endregion UI
}