using Game.Networking.Core;
using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyCodeField;

    public async void Join()
    {
        await GameNetPortal.Instance.StartClient(lobbyCodeField.text);
    }
    
    public async void Create()
    {
        await GameNetPortal.Instance.StartHost(lobbyCodeField.text);
    }
}
