using System.Collections.Generic;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

[HideMonoScript, InfoBox("Debug Mode Enabled! Make sure to disable it before building!", InfoMessageType.Warning, nameof(debugMode))]
public class GameNetPortal : MonoBehaviour
{
    public static GameNetPortal Instance;

    public static readonly ulong HostNetworkId = 0;

    public LobbyAPIInterface GetLobbyAPIInterface => _lobbyAPIInterface;
    public RelayAPIInterface GetRelayAPIInterface => _relayAPIInterface;
    public AuthenticationAPIInterface GetAuthenticationAPIInterface => _authenticationAPIInterface;
    public NetworkManager GetNetworkManager => NetworkManager.Singleton;
    
    [Header("UI")] 
    [SerializeField] private bool displayLobbyCode;
    [SerializeField, ShowIf(nameof(displayLobbyCode))] private TMP_Text lobbyCodeText;

    [Header("Scene Management")]
    [SerializeField] private string offlineSceneName;
    [SerializeField] private string onlineSceneName;
    [SerializeField] private string singlePlayerSceneName;

    [Header("Debug")] [SerializeField] private bool debugMode = true;
    [SerializeField] private bool leaveOnEscape = true;

    private LobbyAPIInterface _lobbyAPIInterface;
    private RelayAPIInterface _relayAPIInterface;
    private AuthenticationAPIInterface _authenticationAPIInterface;

    private const float HeartbeatPeriod = 8;
    private float _heartbeatTime;

    #region Initialization

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

        _lobbyAPIInterface = new LobbyAPIInterface();
        _relayAPIInterface = new RelayAPIInterface();
        _authenticationAPIInterface = new AuthenticationAPIInterface();
    }

    private async void Start()
    {
        await _authenticationAPIInterface.InitializeAndSignInAsync();
        Subscribe();
    }

    private void Subscribe()
    {
        GetNetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnDestroy()
    {
        if (GetNetworkManager)
            GetNetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    #endregion

    #region Disconnecting

    private void OnClientDisconnect(ulong clientId)
    {
        if (clientId == HostNetworkId)
        {
            HideLobbyCode();
            LoadOfflineScene();
        }
    }

    public async Task Disconnect()
    {
        var nm = GetNetworkManager;
        if (!nm.IsListening || nm.ShutdownInProgress)
            return;
        HideLobbyCode();
        if (nm.IsHost)
        {
            nm.ConnectionApprovalCallback -= HandleConnectionApproval;
        }

        if(GameManager.Instance.GetGameType() == GameType.Multiplayer)
            await LeaveOrDeleteLobby();
        nm.Shutdown();
        LoadOfflineScene();
        LoadingScreen.Instance.LoadFake();
    }

    private async Task LeaveOrDeleteLobby()
    {
        if (_lobbyAPIInterface.JoinedLobby == null || GetNetworkManager == null)
            return;
        var lobby = _lobbyAPIInterface.JoinedLobby;
        var joinedLobbyID = lobby.Id;
        if (lobby.Players.Count <= 1 || NetworkManager.Singleton.IsHost)
        {
            await _lobbyAPIInterface.DeleteLobby(joinedLobbyID);
            if (debugMode)
                Debug.Log($"Deleted the lobby with the ID {joinedLobbyID}.");
        }
        else
        {
            await _lobbyAPIInterface.RemovePlayerFromLobby(AuthenticationService.Instance.PlayerId, joinedLobbyID);
            if (debugMode)
                Debug.Log($"{AuthenticationService.Instance.PlayerId} has left the lobby.");
        }
    }

    #endregion

    #region Joinig/Creating

    private void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.CreatePlayerObject = GameManager.Instance.GetGameType() == GameType.Singleplayer;
        response.Approved = true;
    }

    public async Task<string> CreateGame(string lobbyName, int maxPlayers = 4)
    {
        if (maxPlayers <= 0 || string.IsNullOrWhiteSpace(lobbyName))
            return "";

        var relayStarted = await _relayAPIInterface.StartRelayServer(maxPlayers, debugMode);

        if (!relayStarted)
            return "";

        var lobby = await _lobbyAPIInterface.CreateLobby(AuthenticationService.Instance.PlayerId, lobbyName, maxPlayers,
            false,
            new Dictionary<string, PlayerDataObject>(), new Dictionary<string, DataObject>()
            {
                {
                    "joinCode",
                    new DataObject(DataObject.VisibilityOptions.Member, _relayAPIInterface.HostData.JoinCode)
                }
            }, debugMode);

        if (_lobbyAPIInterface.LobbyConnectionStatus == LobbyConnectionStatus.Failed)
            return "";
        
        GetNetworkManager.ConnectionApprovalCallback += HandleConnectionApproval;
        GetNetworkManager.StartHost();
        LoadingScreen.Instance.LoadFake();
        LoadOnlineScene();
        ShowLobbyCode();
        return lobby.LobbyCode;
    }

    public async Task StartClient(string lobbyCode)
    {
        if (lobbyCode.Length != 6)
            return;
        var lobby = await _lobbyAPIInterface.JoinLobbyByCode(AuthenticationService.Instance.PlayerId, lobbyCode, null);
        if (lobby.IsLocked)
            return;

        if (lobby.Players.Count > lobby.MaxPlayers)
        {
            if (debugMode)
                Debug.Log("Can not join! Lobby Full!");
            return;
        }

        if (_lobbyAPIInterface.LobbyConnectionStatus == LobbyConnectionStatus.Failed)
            return;

        var relayJoinCode = lobby.Data["joinCode"].Value;

        await _relayAPIInterface.JoinRelayServer(relayJoinCode);

        if (_relayAPIInterface.RelayConnectionStatus == RelayConnectionStatus.Failed)
            return;

        GetNetworkManager.StartClient();
        LoadingScreen.Instance.LoadFake();
        ShowLobbyCode();
    }
    
    public void StartSingleplayerGame()
    {
        GetNetworkManager.ConnectionApprovalCallback += HandleConnectionApproval;
        GetNetworkManager.StartHost();
        LoadingScreen.Instance.LoadFake();
        LoadSingleplayerScene();
    }

    #endregion

    #region UI

    public void ShowLobbyCode()
    {
        if (displayLobbyCode)
            lobbyCodeText.text = _lobbyAPIInterface.JoinedLobby.LobbyCode;
    }

    public void HideLobbyCode()
    {
        if (displayLobbyCode)
            lobbyCodeText.text = "";
    }

    #endregion

    #region Scene Managment

    private void LoadSingleplayerScene()
    {
        SceneManager.LoadSceneAsync(singlePlayerSceneName, LoadSceneMode.Single);
        LoadingScreen.Instance.LoadFake();
    }
    
    private void LoadOnlineScene()
    {
        LoadOnlineSceneServerRpc();
    }

    private void LoadOfflineScene()
    {
        SceneManager.LoadSceneAsync(offlineSceneName, LoadSceneMode.Single);
        LoadingScreen.Instance.LoadFake();
    }

    [ServerRpc]
    private void LoadOnlineSceneServerRpc()
    {
        GetNetworkManager.SceneManager.LoadScene(onlineSceneName, LoadSceneMode.Single);
    }
    #endregion

    #region Lobby Heartbeat

    private async void Update()
    {
        if (GetNetworkManager.IsHost && GameManager.Instance.GetGameType() == GameType.Multiplayer)
            UpdateLobbyHeartbeat();
        if (leaveOnEscape && Input.GetKeyDown(KeyCode.Escape))
        {
            await Disconnect();
        }
    }

    private void UpdateLobbyHeartbeat()
    {
        _heartbeatTime += Time.deltaTime;
        if (_heartbeatTime > HeartbeatPeriod)
        {
            _heartbeatTime -= HeartbeatPeriod;
            _lobbyAPIInterface.SendHeartbeatPing(_lobbyAPIInterface.JoinedLobby.Id);
            if (debugMode)
                Debug.Log($"Sent a ping to the lobby with the ID {_lobbyAPIInterface.JoinedLobby.Id}.");
        }
    }

    #endregion
}