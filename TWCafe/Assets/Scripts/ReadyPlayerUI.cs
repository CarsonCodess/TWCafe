using System;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.UI;

public class ReadyPlayerUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text readyText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color readyColor;
    [SerializeField] private Color notReadyColor;
    public NetworkVariable<PlayerReadyData> playerData = new NetworkVariable<PlayerReadyData>();

    [ClientRpc]
    public void InitializeClientRpc(ulong clientId)
    {
        if(clientId != NetworkManager.LocalClientId)
            return;
        SetPlayerDataServerRpc(new PlayerReadyData()
        {
            PlayerName = AuthenticationService.Instance.PlayerId,
            IsReady = false,
            ClientId = NetworkManager.LocalClientId
        });
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerDataServerRpc(PlayerReadyData data)
    {
        playerData.Value = data;
    }
    
    public void ToggleReady()
    {
        SetPlayerDataServerRpc(new PlayerReadyData()
        {
            PlayerName = AuthenticationService.Instance.PlayerId,
            IsReady = !playerData.Value.IsReady,
            ClientId = NetworkManager.LocalClientId
        });
    }

    private void Update()
    {
        playerNameText.text = playerData.Value.PlayerName.Value;
        readyText.text = playerData.Value.IsReady ? "Ready!" : "Not Ready";
        backgroundImage.color = playerData.Value.IsReady ? readyColor : notReadyColor;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        playerData.Dispose();
    }
}
