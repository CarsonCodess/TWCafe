using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationAPIInterface
{
    public async Task InitializeAndSignInAsync(InitializationOptions initializationOptions = null)
    {
        try
        {
            await UnityServices.InitializeAsync(initializationOptions);

            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (Exception e)
        {
            var reason = $"{e.Message} ({e.InnerException?.Message})";
            Debug.LogError($"Authentication Error: {reason}, {e}");
            throw;
        }
    }
}