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

    public async void Login()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(usernameField.text, passwordField.text);
            }
            catch (AuthenticationException e)
            {
                errorMessage.text = $"{e.Message} [ERROR_CODE: {e.ErrorCode}]";
                return;
            }
            catch (RequestFailedException e)
            {
                errorMessage.text = $"{e.Message} [ERROR_CODE: {e.ErrorCode}]";
                return;
            }
        }
        
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
                errorMessage.text = $"{e.Message} [ERROR_CODE: {e.ErrorCode}]";
                return;
            }
            catch (RequestFailedException e)
            {
                errorMessage.text = $"{e.Message} [ERROR_CODE: {e.ErrorCode}]";
                return;
            }
        }
        
        titleScreen.SetActive(true);
        gameObject.SetActive(false);
    }
}
