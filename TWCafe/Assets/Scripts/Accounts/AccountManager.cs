using System;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AccountManager : Singleton<AccountManager>
{
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_Text errorMessage;
    [SerializeField] private GameObject titleScreen;

    protected void Start()
    {
        if (PlayerPrefs.HasKey("_PASSWORD"))
        {
            var username = PlayerPrefs.GetString("_USERNAME");
            var password = PlayerPrefs.GetString("_PASSWORD");
            LoginInternal(username, password);
        }
    }

    public void Login()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            LoginInternal(usernameField.text, passwordField.text);
        }
    }

    private async void LoginInternal(string username, string password)
    {
        try
        {
            if (!PlayerPrefs.HasKey("_PASSWORD"))
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            }
        }
        catch (AuthenticationException e)
        {
            errorMessage.text = $"{e.Message}";
            return;
        }
        catch (RequestFailedException e)
        {
            errorMessage.text = $"{e.Message}";
            return;
        }
        
        PlayerPrefs.SetString("_USERNAME", usernameField.text);
        PlayerPrefs.SetString("_PASSWORD", passwordField.text);
        titleScreen.SetActive(true);
        gameObject.SetActive(false);
    }

    public async void CreateAccount()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(usernameField.text,
                    passwordField.text);
            }
            catch (AuthenticationException e)
            {
                errorMessage.text = $"{e.Message}";
                return;
            }
            catch (RequestFailedException e)
            {
                errorMessage.text = $"{e.Message}";
                return;
            }
        }
        
        titleScreen.SetActive(true);
        gameObject.SetActive(false);
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("_USERNAME");
        PlayerPrefs.DeleteKey("_PASSWORD");
        titleScreen.SetActive(false);
        gameObject.SetActive(true);
        AuthenticationService.Instance.SignOut(true);
    }
}
