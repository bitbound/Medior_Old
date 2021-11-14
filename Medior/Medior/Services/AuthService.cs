using Medior.Core.Shared.BaseTypes;
using Medior.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IAuthService
    {
        Task<Result<AuthenticationResult>> GetTokenSilently(IntPtr windowHandle);
        Task<Result<AuthenticationResult>> SignInInteractive(IntPtr windowHandle);
        Task<Result<AuthenticationResult>> EditProfile(IntPtr windowHandle);
    }

    public class AuthService : IAuthService
    {
        private static readonly string _azureAdB2CHostname = $"{_tenantName}.b2clogin.com";
        private static readonly string _clientId = "1d50d95c-e211-4499-9248-fa4e5dbff323";
        private static readonly string _redirectUri = $"https://{_tenantName}.b2clogin.com/oauth2/nativeclient";
        private static readonly string _tenant = $"{_tenantName}.onmicrosoft.com";
        private static readonly string _tenantName = "mediorapp";
        private static readonly string[] _apiScopes = { $"https://{_tenant}/medior-api/app.user" };
        private static readonly string _authorityBase = $"https://{_azureAdB2CHostname}/tfp/{_tenant}/";
        private static readonly string _authorityEditProfile = $"{_authorityBase}{_policyEditProfile}";
        private static readonly string _authorityResetPassword = $"{_authorityBase}{_policyResetPassword}";
        private static readonly string _authoritySignUpSignIn = $"{_authorityBase}{_policySignUpSignIn}";
        private static readonly string _policyEditProfile = "b2c_1_edit_profile";
        private static readonly string _policyResetPassword = "b2c_1_reset_password";
        private static readonly string _policySignUpSignIn = "b2c_1_signup_signin";
        private readonly ILogger<AuthService> _logger;

        private readonly IPublicClientApplication _publicClientApp;

        public AuthService(ILogger<AuthService> logger)
        {
            _logger = logger;

            _publicClientApp = PublicClientApplicationBuilder.Create(_clientId)
                .WithB2CAuthority(_authoritySignUpSignIn)
                .WithRedirectUri(_redirectUri)
#if DEBUG
                .WithLogging(Log, Microsoft.Identity.Client.LogLevel.Verbose, true)
#else
                .WithLogging(Log, Microsoft.Identity.Client.LogLevel.Info, false)
#endif
                .Build();

            TokenCacheHelper.Bind(_publicClientApp.UserTokenCache);
        }

        public async Task<Result<AuthenticationResult>> GetTokenSilently(IntPtr windowHandle)
        {
            var accounts = await _publicClientApp.GetAccountsAsync(_policySignUpSignIn);

            try
            {
                var authResult = await _publicClientApp
                    .AcquireTokenSilent(_apiScopes, accounts.FirstOrDefault())
                    .ExecuteAsync();

                return Result.Ok(authResult);
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogError(ex, "Failed to acquire token silently. Interactive logon required.");
                return await SignInInteractive(windowHandle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acquire token silently.");
            }

            return Result.Fail<AuthenticationResult>("Failed to acquire token silently.");
        }

        public async Task<Result<AuthenticationResult>> EditProfile(IntPtr windowHandle)
        {
            try
            {
                var authResult = await _publicClientApp.AcquireTokenInteractive(_apiScopes)
                    .WithParentActivityOrWindow(windowHandle)
                    .WithB2CAuthority(_authorityEditProfile)
                    .WithPrompt(Prompt.NoPrompt)
                    .ExecuteAsync(CancellationToken.None);

                return Result.Ok(authResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing profile.");
                return Result.Fail<AuthenticationResult>("An error occurred while editing profile.");
            }
        }
        public async Task<Result<AuthenticationResult>> SignInInteractive(IntPtr windowHandle)
        {
            AuthenticationResult? authResult = null;
            try
            {
                authResult = await _publicClientApp.AcquireTokenInteractive(_apiScopes)
                    .WithParentActivityOrWindow(windowHandle)
                    .ExecuteAsync();
            }
            catch (MsalException ex)
            {
                try
                {
                    _logger.LogError(ex, "Error acquiring token.");

                    if (ex.Message.Contains("AADB2C90118"))
                    {
                        authResult = await _publicClientApp.AcquireTokenInteractive(_apiScopes)
                            .WithParentActivityOrWindow(windowHandle)
                            .WithPrompt(Prompt.SelectAccount)
                            .WithB2CAuthority(_authorityResetPassword)
                            .ExecuteAsync();
                    }
                }
                catch (Exception ex2)
                {
                    _logger.LogError(ex2, "Error acquiring token.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error acquiring token.");
            }

            if (authResult is null)
            {
                return Result.Fail<AuthenticationResult>("Failed to acquire auth token.");
            }

            return Result.Ok(authResult);
        }
        private void Log(Microsoft.Identity.Client.LogLevel level, string message, bool containsPii)
        {
            if (message is null)
            {
                return;
            }

#pragma warning disable CA2254 // Template should be a static expression
            switch (level)
            {
                case Microsoft.Identity.Client.LogLevel.Error:
                    _logger.LogError(message);
                    break;
                case Microsoft.Identity.Client.LogLevel.Warning:
                    _logger.LogWarning(message);
                    break;
                case Microsoft.Identity.Client.LogLevel.Info:
                    _logger.LogInformation(message);
                    break;
                case Microsoft.Identity.Client.LogLevel.Verbose:
                    _logger.LogDebug(message);
                    break;
                default:
                    break;
            }
#pragma warning restore CA2254 // Template should be a static expression
        }
    }
}
