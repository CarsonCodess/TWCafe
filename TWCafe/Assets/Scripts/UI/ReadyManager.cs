using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public struct PlayerReadyData : INetworkSerializable, IEquatable<PlayerReadyData>
{
    public FixedString32Bytes PlayerName;
    public bool IsReady;
    public ulong ClientId;
    
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref IsReady);
        serializer.SerializeValue(ref ClientId);
    }

    public bool Equals(PlayerReadyData other)
    {
        return PlayerName.Equals(other.PlayerName) && IsReady == other.IsReady && ClientId == other.ClientId;
    }
}

public class ReadyManager : NetworkBehaviour
{
    [SerializeField] private GameObject startGameButton;
    [SerializeField] private string gameplaySceneName = "Imated";
    [SerializeField] private GameObject readyPlayerUiParent;
    [SerializeField] private GameObject readyPlayerUiPrefab;
    
    private Dictionary<ulong, GameObject> _playerPrefabs = new Dictionary<ulong, GameObject>();
    private NetworkVariable<int> _readyPlayers = new NetworkVariable<int>();

    private void Start()
    {
        if(!IsHost)
            return;
        NetworkManager.OnClientConnectedCallback += OnClientConnect;
        NetworkManager.OnClientDisconnectCallback += OnClientDisconnect;
        SpawnPlayerUI(NetworkManager.LocalClientId);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        if(!NetworkManager || !IsHost)
            return;
        NetworkManager.OnClientConnectedCallback -= OnClientConnect;
        NetworkManager.OnClientDisconnectCallback -= OnClientDisconnect;
    }

    private void Update()
    {
        if(!IsHost)
            return;
        if(_readyPlayers.Value == _playerPrefabs.Count)
            startGameButton.SetActive(true);
        else
            startGameButton.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnReadyUpServerRpc(bool host)
    {
        if(host)
            _readyPlayers.Value++;
        else
            _readyPlayers.Value--;
    }

    [ServerRpc(RequireOwnership = false)]
    public void OnUnReadyUpServerRpc(bool host)
    {
        if(host)
            _readyPlayers.Value--;
        else
            _readyPlayers.Value++;
    }

    private void OnClientConnect(ulong clientId)
    {
        SpawnPlayerUI(clientId);
    }
    
    private void OnClientDisconnect(ulong clientId)
    {
        _playerPrefabs[clientId].GetComponent<NetworkObject>().Despawn();
        _playerPrefabs.Remove(clientId);
    }

    private void SpawnPlayerUI(ulong clientId)
    {
        var readyPlayer = Instantiate(readyPlayerUiPrefab, readyPlayerUiParent.transform);
        readyPlayer.GetComponent<NetworkObject>().Spawn(true);
        readyPlayer.transform.SetParent(readyPlayerUiParent.transform);
        var readyPlayerUI = readyPlayer.GetComponent<ReadyPlayerUI>();
        _playerPrefabs.Add(clientId, readyPlayer);
        readyPlayerUI.InitializeClientRpc(clientId);
    }

    public void ReadyUp()
    {
        var clientId = NetworkManager.LocalClientId;
        var readyPlayerUis = readyPlayerUiParent.GetComponentsInChildren<ReadyPlayerUI>();
        foreach (var playerUi in readyPlayerUis)
        {
            if (playerUi.playerData.Value.ClientId == clientId)
            {
                playerUi.ToggleReady();
                if(playerUi.playerData.Value.IsReady)
                    OnReadyUpServerRpc(IsHost);
                else
                    OnUnReadyUpServerRpc(IsHost);
                break;
            }
        }
    }

    public async void StartGame()
    {
        foreach (var client in NetworkManager.ConnectedClients)
        {
            var player = Instantiate(NetworkManager.NetworkConfig.PlayerPrefab);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.Key);
        }

        var lobbyApi = GameNetPortal.Instance.GetLobbyAPIInterface;
        await lobbyApi.UpdateLobby(lobbyApi.JoinedLobby.LobbyCode, new Dictionary<string, DataObject>(), true);
        LoadingScreen.Instance.LoadFake();
        NetworkManager.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }

    public async void LeaveLobby()
    {
        await GameNetPortal.Instance.UserDisconnect();
    }
}
