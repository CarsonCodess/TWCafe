using UnityEngine;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject singleplayerScreen;
    [SerializeField] private GameObject multiplayerScreen;
    [SerializeField] private GameObject optionsScreen;

    public void Start(){
        titleScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.up, 0f);
        singleplayerScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.down, 0f);
        multiplayerScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.left, 0f);
        optionsScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.right, 0f);
    }

    public void StartSinglePlayer()
    {
        GameManager.Instance.SetGameType(GameType.Singleplayer);
        GameNetPortal.Instance.StartSingleplayerGame();
    }
    
    public void LoadSingleplayerScreen()
    {
        DisableAllScreens();
        singleplayerScreen.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 1f);
    }
    
    public void LoadMultiplayerScreen()
    {
        DisableAllScreens();
        multiplayerScreen.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 1f);
    }

    public void LoadOptionsScreen()
    {
        DisableAllScreens();
        optionsScreen.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 1f);
    }
    
    public void LoadTitleScreen()
    {
        DisableAllScreens();
        titleScreen.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, 1f);
    }
    
    public void Quit()
    {
        Application.Quit();
    }
    public void DisableAllScreens()
    {
        titleScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.up, 1f);
        singleplayerScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.down, 1f);
        multiplayerScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.left, 1f);
        optionsScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.right, 1f);
    }
}
