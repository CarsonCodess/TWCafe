using UnityEngine;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject singleplayerScreen;
    [SerializeField] private GameObject multiplayerScreen;
    [SerializeField] private GameObject optionsScreen;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Ease transitionEase = Ease.OutBack;

    public void Start(){
        titleScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.up, 0f).SetEase(transitionEase);
        singleplayerScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.down, 0f).SetEase(transitionEase);
        multiplayerScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.left, 0f).SetEase(transitionEase);
        optionsScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.right, 0f).SetEase(transitionEase);
    }

    public void StartSinglePlayer()
    {
        GameManager.Instance.SetGameType(GameType.Singleplayer);
        GameNetPortal.Instance.StartSingleplayerGame();
    }
    
    public void LoadSingleplayerScreen()
    {
        DisableAllScreens();
        singleplayerScreen.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, transitionDuration).SetEase(transitionEase);
    }
    
    public void LoadMultiplayerScreen()
    {
        DisableAllScreens();
        multiplayerScreen.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, transitionDuration).SetEase(transitionEase);
    }

    public void LoadOptionsScreen()
    {
        DisableAllScreens();
        optionsScreen.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, transitionDuration).SetEase(transitionEase);
    }
    
    public void LoadTitleScreen()
    {
        DisableAllScreens();
        titleScreen.GetComponent<RectTransform>().DOAnchorPos(Vector2.zero, transitionDuration).SetEase(transitionEase);
    }
    
    public void Quit()
    {
        Application.Quit();
    }
    public void DisableAllScreens()
    {
        titleScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.up, transitionDuration).SetEase(transitionEase);
        singleplayerScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.down, transitionDuration).SetEase(transitionEase);
        multiplayerScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.left, transitionDuration).SetEase(transitionEase);
        optionsScreen.GetComponent<RectTransform>().DOAnchorPos(new Vector2(Screen.width, Screen.height) * Vector2.right, transitionDuration).SetEase(transitionEase);
    }
}
