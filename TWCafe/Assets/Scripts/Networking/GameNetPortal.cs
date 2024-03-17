using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetPortal : MonoBehaviour
{
    public static GameNetPortal Instance;

    public static readonly ulong HostNetworkId = 0;

    public LobbyAPIInterface GetLobbyAPIInterface => _lobbyAPIInterface;
    public RelayAPIInterface GetRelayAPIInterface => _relayAPIInterface;
    public AuthenticationAPIInterface GetAuthenticationAPIInterface => _authenticationAPIInterface;

    [Header("UI")] 
    [SerializeField] private bool displayLobbyCode;
    [SerializeField, ShowIf(nameof(displayLobbyCode))] private TMP_Text lobbyCodeText;

    [Header("Scene Management")]
    [SerializeField] private string offlineSceneName;
    [SerializeField] private string onlineSceneName;
    [SerializeField] private string singlePlayerSceneName;

    [Header("Debug")]
    [SerializeField] private bool leaveOnEscape = true;

    private LobbyAPIInterface _lobbyAPIInterface;
    private RelayAPIInterface _relayAPIInterface;
    private AuthenticationAPIInterface _authenticationAPIInterface;

    private const float HeartbeatPeriod = 8;
    private float _heartbeatTime;
    private NetworkManager _nm;

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
        _nm = NetworkManager.Singleton;
        Subscribe();
    }

    private void Subscribe()
    {
        _nm.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnDestroy()
    {
        if (_nm)
            _nm.OnClientDisconnectCallback -= OnClientDisconnect;
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
        var nm = _nm;
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
        if (_lobbyAPIInterface.JoinedLobby == null || _nm == null)
            return;
        var lobby = _lobbyAPIInterface.JoinedLobby;
        var joinedLobbyID = lobby.Id;
        if (lobby.Players.Count <= 1 || NetworkManager.Singleton.IsHost)
            await _lobbyAPIInterface.DeleteLobby(joinedLobbyID);
        else
            await _lobbyAPIInterface.RemovePlayerFromLobby(AuthenticationService.Instance.PlayerId, joinedLobbyID);
    }

    #endregion

    #region Joinig/Creating

    private void HandleConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.CreatePlayerObject = false;
        response.Approved = true;
    }

    public async Task<string> CreateGame()
    {
        var relayStarted = await _relayAPIInterface.StartRelayServer(4);
        if (!relayStarted)
            return "";

        var lobby = await _lobbyAPIInterface.CreateLobby(AuthenticationService.Instance.PlayerId, GenerateRandomString(16), 4,
            false,
            new Dictionary<string, PlayerDataObject>(), new Dictionary<string, DataObject>()
            {
                {
                    "joinCode",
                    new DataObject(DataObject.VisibilityOptions.Member, _relayAPIInterface.HostData.JoinCode)
                }
            });

        _nm.ConnectionApprovalCallback += HandleConnectionApproval;
        _nm.StartHost();
        LoadingScreen.Instance.LoadFake();
        LoadOnlineScene();
        ShowLobbyCode();
        return lobby.LobbyCode;
    }

    public async Task StartClient(string lobbyCode)
    {
        var lobby = await _lobbyAPIInterface.JoinLobbyByCode(AuthenticationService.Instance.PlayerId, lobbyCode, null);
        if (lobby.IsLocked)
            return;

        if (lobby.Players.Count > lobby.MaxPlayers)
        {
            Debug.Log("Can not join! Lobby Full!");
            return;
        }

        var relayJoinCode = lobby.Data["joinCode"].Value;
        var relayStarted = await _relayAPIInterface.JoinRelayServer(relayJoinCode);
        if (!relayStarted)
            return;

        _nm.StartClient();
        LoadingScreen.Instance.LoadFake();
        ShowLobbyCode();
    }
    
    public void StartSingleplayerGame()
    {
        _nm.ConnectionApprovalCallback += HandleConnectionApproval;
        _nm.StartHost();
        if (GameManager.Instance.GetGameType() == GameType.Singleplayer)
        {
            var player = Instantiate(_nm.NetworkConfig.PlayerPrefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(HostNetworkId);
        }
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
        _nm.SceneManager.LoadScene(singlePlayerSceneName, LoadSceneMode.Single);
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
        _nm.SceneManager.LoadScene(onlineSceneName, LoadSceneMode.Single);
    }
    #endregion

    #region Lobby Heartbeat

    private async void Update()
    {
        if (_nm.IsHost && GameManager.Instance.GetGameType() == GameType.Multiplayer)
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
        }
    }

    #endregion

    #region Utilities

    public string GenerateRandomString(int length)
    {
        var glyphs= "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        var charAmount = Random.Range(length + 5, length - 5);
        var sb = new StringBuilder(charAmount);
        for(var i = 0; i < charAmount; i++)
            sb.Append(glyphs[Random.Range(0, glyphs.Length)]);
        return sb.ToString();
    }

    #endregion
}