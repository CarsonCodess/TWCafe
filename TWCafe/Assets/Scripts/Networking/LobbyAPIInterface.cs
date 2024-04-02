using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

    public enum LobbyConnectionStatus
    {
        Success,
        Failed,
        Connecting,
        Creating,
        Deleting,
        Disconnect
    }

/// <summary>
/// Wrapper for all the interactions with the Lobby API.
/// </summary>
public class LobbyAPIInterface
{
    public Lobby JoinedLobby;

    public async Task<Lobby> CreateLobby(string requesterUasId, string lobbyName, int maxPlayers, bool isPrivate,
        Dictionary<string, PlayerDataObject> hostUserData, Dictionary<string, DataObject> lobbyData)
    {
        var createOptions = new CreateLobbyOptions()
        {
            IsPrivate = isPrivate,
            Player = new Unity.Services.Lobbies.Models.Player(requesterUasId, null, hostUserData),
            Data = lobbyData
        };

        Lobby lobby;

        try
        {
            lobby = await Lobbies.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);
        }
        catch (Exception e)
        {
            var reason = $"{e.Message} ({e.InnerException?.Message})";
            Debug.LogError($"Lobby Error: {reason}, {e}");
            throw;
        }
        
        JoinedLobby = lobby;
        return lobby;
    }

    public async Task DeleteLobby(string lobbyId)
    {
        try
        {
            await Lobbies.Instance.DeleteLobbyAsync(lobbyId);
        }
        catch (Exception e)
        {
            var reason = $"{e.Message} ({e.InnerException?.Message})";
            Debug.LogError($"Lobby Error: {reason}");
            throw;
        }

        JoinedLobby = null;
    }

    public async Task<Lobby> JoinLobbyByCode(string requesterUasId, string lobbyCode,
        Dictionary<string, PlayerDataObject> localUserData)
    {
        var joinOptions = new JoinLobbyByCodeOptions()
        {
            Player = new Unity.Services.Lobbies.Models.Player(requesterUasId, null, localUserData)
        };
        try
        {
            var lobby = await Lobbies.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
            JoinedLobby = lobby;
        }
        catch (Exception e)
        {
            var reason = $"{e.Message} ({e.InnerException?.Message})";
            Debug.LogError($"Lobby Error: {reason}");
            throw;
        }

        return JoinedLobby;
    }

    public async Task RemovePlayerFromLobby(string requesterUasId, string lobbyId)
    {
        try
        {
            await Lobbies.Instance.RemovePlayerAsync(lobbyId, requesterUasId);
            JoinedLobby = null;
        }
        catch (LobbyServiceException e) when (e is {Reason: LobbyExceptionReason.PlayerNotFound})
        {
            // If Player is not found, they have already left the lobby or have been kicked out. No need to throw here
            Debug.Log("Player has already left the lobby.");
        }
    }
    
    public async Task<Lobby> UpdateLobby(string lobbyId, Dictionary<string, DataObject> data, bool shouldLock)
    {
        var updateOptions = new UpdateLobbyOptions()
        {
            Data = data,
            IsLocked = shouldLock
        };

        try
        {
            return await Lobbies.Instance.UpdateLobbyAsync(lobbyId, updateOptions);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e.Reason);
            throw;
        }
    }

    public async void SendHeartbeatPing(string lobbyId)
    {
        try
        {
            await Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
        }
        catch (Exception e)
        {
            var reason = $"{e.Message} ({e.InnerException?.Message})";
            Debug.LogError($"Lobby Error: {reason}");
            throw;
        }
    }
}