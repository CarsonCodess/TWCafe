using TMPro;
using UnityEngine;

public class MultiplayerMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField lobbyCodeField;

    public async void Join()
    {
        GameManager.Instance.SetGameType(GameType.Multiplayer);
        await GameNetPortal.Instance.StartClient(lobbyCodeField.text);
    }
    
    public async void Create()
    {
        GameManager.Instance.SetGameType(GameType.Multiplayer);
        await GameNetPortal.Instance.CreateGame();
    }
}
