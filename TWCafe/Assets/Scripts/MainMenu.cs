using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject titleScreen;
    [SerializeField] private GameObject singleplayerScreen;
    [SerializeField] private GameObject multiplayerScreen;
    [SerializeField] private GameObject optionsScreen;

    public void LoadSingleplayerScreen()
    {
        DisableAllScreens();
        singleplayerScreen.SetActive(true);
    }
    
    public void LoadMultiplayerScreen()
    {
        DisableAllScreens();
        multiplayerScreen.SetActive(true);
    }

    public void LoadOptionsScreen()
    {
        DisableAllScreens();
        optionsScreen.SetActive(true);
    }
    
    public void LoadTitleScreen()
    {
        DisableAllScreens();
        titleScreen.SetActive(true);
    }
    
    public void Quit()
    {
        Application.Quit();
    }

    private void DisableAllScreens()
    {
        titleScreen.SetActive(false);
        singleplayerScreen.SetActive(false);
        multiplayerScreen.SetActive(false);
        optionsScreen.SetActive(false);
    }
}
