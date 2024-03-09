using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Networking.Core.Interfaces;
using Sirenix.OdinInspector;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.Networking.Core
{
    [HideMonoScript, InfoBox("Debug Mode Enabled! Make sure to disable it before building!", InfoMessageType.Warning, nameof(debugMode))]
    public class GameNetPortal : MonoBehaviour
    {
        public enum ConnectionStatus
        {
            Idle,
            Success,
            LobbyFull,
            UserDisconnect,
            RelayFailure,
            LobbyFailure
        }
        
        public static GameNetPortal Instance;
        
        public static readonly ulong HostNetworkId = 0;
        
        public LobbyAPIInterface GetLobbyAPIInterface => _lobbyAPIInterface;
        public RelayAPIInterface GetRelayAPIInterface => _relayAPIInterface;
        public AuthenticationAPIInterface GetAuthenticationAPIInterface => _authenticationAPIInterface;
        public NetworkManager GetNetworkManager => NetworkManager.Singleton;

        [ShowInInspector, ReadOnly] private ConnectionStatus _connectionStatus;
        [Header("Options")]
        [SerializeField] private bool spawnPlayerPrefab;
        [Header("UI")] 
        [SerializeField] private bool displayLobbyCode;
        [SerializeField, ShowIf(nameof(displayLobbyCode))] private TMP_Text lobbyCodeText;

        [Header("Scene Management")]
        [SerializeField] private bool enableSceneManagement = true;
        [SerializeField, ShowIf(nameof(enableSceneManagement))] private string offlineSceneName;
        [SerializeField, ShowIf(nameof(enableSceneManagement))] private string onlineSceneName;
        [Header("Debug")]
        [SerializeField] private bool debugMode = true;
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
            _connectionStatus = ConnectionStatus.Idle;
        }

        private void Subscribe()
        {
            GetNetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
        }
        
        private void OnDestroy()
        {
            if(GetNetworkManager)
                GetNetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
        }

        #endregion

        #region Disconnecting
        
        private async void OnClientDisconnect(ulong clientId)
        {
            if(clientId == HostNetworkId && GetNetworkManager.IsServer)
                await UserDisconnectClientRpc();
        }
        
        [ClientRpc]
        public async Task UserDisconnectClientRpc()
        {
            var nm = GetNetworkManager;
            if(!nm.IsListening || nm.ShutdownInProgress)
                return;
            HideLobbyCode();
            if(nm.IsHost)
                nm.ConnectionApprovalCallback -= HandleConnectionApproval;

            await LeaveOrDeleteLobby();
            _connectionStatus = ConnectionStatus.UserDisconnect;
            nm.Shutdown();
        }
        
        [ServerRpc]
        public async Task UserDisconnectServerRpc()
        {
            var nm = GetNetworkManager;
            if(!nm.IsListening || nm.ShutdownInProgress)
                return;
            HideLobbyCode();
            if(nm.IsHost)
                nm.ConnectionApprovalCallback -= HandleConnectionApproval;

            await LeaveOrDeleteLobby();
            _connectionStatus = ConnectionStatus.UserDisconnect;
            nm.Shutdown();
            if(!nm.IsHost)
                LoadOfflineScene();
            else
                SwitchSceneServerRpc(offlineSceneName, true);
        }

        private async Task LeaveOrDeleteLobby()
        {
            if(_lobbyAPIInterface.JoinedLobby == null || GetNetworkManager == null)
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
            response.CreatePlayerObject = spawnPlayerPrefab;
            response.Approved = true;
        }
        
        public async Task<string> StartHost(string lobbyName, int maxPlayers = 4, bool isPrivate = false)
        {
            if (maxPlayers <= 0 || string.IsNullOrWhiteSpace(lobbyName))
                return "";

            var relayStarted = await _relayAPIInterface.StartRelayServer(maxPlayers, debugMode);

            if (!relayStarted)
            {
                _connectionStatus = ConnectionStatus.RelayFailure;
                return "";
            }
        
            var lobby = await _lobbyAPIInterface.CreateLobby(AuthenticationService.Instance.PlayerId, lobbyName, maxPlayers, isPrivate, 
                new Dictionary<string, PlayerDataObject>(), new Dictionary<string, DataObject>()
                {
                    {
                        "joinCode",
                        new DataObject(DataObject.VisibilityOptions.Member, _relayAPIInterface.HostData.JoinCode)
                    }
                }, debugMode);

            if (_lobbyAPIInterface.LobbyConnectionStatus == LobbyConnectionStatus.Failed)
            {
                _connectionStatus = ConnectionStatus.LobbyFailure;
                return "";
            }
            
            GetNetworkManager.ConnectionApprovalCallback += HandleConnectionApproval;
            GetNetworkManager.StartHost();
            ShowLobbyCode();
            LoadOnlineScene();
            _connectionStatus = ConnectionStatus.Success;
            return lobby.LobbyCode;
        }

        public async Task StartClient(string lobbyCode)
        {
            if (lobbyCode.Length != 6)
                return;
            var lobby = await _lobbyAPIInterface.JoinLobbyByCode(AuthenticationService.Instance.PlayerId, lobbyCode, null);
            if(lobby.IsLocked)
                return;
            
            if (lobby.Players.Count > lobby.MaxPlayers)
            {
                _connectionStatus = ConnectionStatus.LobbyFull;
                if(debugMode)
                    Debug.Log("Can not join! Lobby Full!");
                return;
            }

            if (_lobbyAPIInterface.LobbyConnectionStatus == LobbyConnectionStatus.Failed)
            {
                _connectionStatus = ConnectionStatus.LobbyFailure;
                return;
            }

            var relayJoinCode = lobby.Data["joinCode"].Value;

            await _relayAPIInterface.JoinRelayServer(relayJoinCode);

            if (_relayAPIInterface.RelayConnectionStatus == RelayConnectionStatus.Failed)
            {
                _connectionStatus = ConnectionStatus.RelayFailure;
                return;
            }
        
            GetNetworkManager.StartClient();
            ShowLobbyCode();
            _connectionStatus = ConnectionStatus.Success;
        }
        
        #endregion
        
        #region UI

        public void ShowLobbyCode()
        {
            if(displayLobbyCode)
                lobbyCodeText.text = _lobbyAPIInterface.JoinedLobby.LobbyCode;
        }
    
        public void HideLobbyCode()
        {
            if(displayLobbyCode)
                lobbyCodeText.text = "";
        }

        #endregion
        
        #region Scene Managment
        
        private void LoadOnlineScene()
        {
            SwitchSceneServerRpc(onlineSceneName);
        }
    
        private void LoadOfflineScene()
        {
            SwitchSceneServerRpc(offlineSceneName, false);
        }

        [ServerRpc]
        private void SwitchSceneServerRpc(string sceneName, bool useNetworkSceneManager = true)
        {
            if(!enableSceneManagement)
                return;
            if(useNetworkSceneManager)
                GetNetworkManager.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            else
                SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        
        #endregion

        #region Lobby Heartbeat

        private async void Update()
        {
            if(GetNetworkManager.IsHost)
                UpdateLobbyHeartbeat();
            if (leaveOnEscape && Input.GetKeyDown(KeyCode.Escape))
                await UserDisconnectServerRpc();
        }
        
        private void UpdateLobbyHeartbeat()
        {
            _heartbeatTime += Time.deltaTime;
            if (_heartbeatTime > HeartbeatPeriod)
            {
                _heartbeatTime -= HeartbeatPeriod;
                _lobbyAPIInterface.SendHeartbeatPing(_lobbyAPIInterface.JoinedLobby.Id);
                if(debugMode)
                    Debug.Log($"Sent a ping to the lobby with the ID {_lobbyAPIInterface.JoinedLobby.Id}.");
            }
        }

        #endregion
    }
}