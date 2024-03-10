using Game.Networking.Core;
using TMPro;
using UnityEngine;

public class MultiplayerMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyCodeField;
    [SerializeField] private TMP_InputField createLobbyField;

    public async void Join()
    {
        await GameNetPortal.Instance.StartClient(lobbyCodeField.text);
    }
    
    public async void Create()
    {
        await GameNetPortal.Instance.StartHost(createLobbyField.text);
    }
}
