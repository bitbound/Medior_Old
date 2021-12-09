using Medior.BaseTypes;
using Medior.Models.Messages;
using Medior.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IAuthService
    {
        string Email { get; }
        bool IsSignedIn { get; }
        Task<Result<AuthenticationResult>> EditProfile(IntPtr windowHandle);
        Task<Result<AuthenticationResult>> GetTokenSilently(IntPtr windowHandle, bool fallbackToInteractive);
        Task<Result<AuthenticationResult>> SignInInteractive(IntPtr windowHandle);
        void SignOut();
    }

    public class AuthService : IAuthService
    {
        private static readonly string _clientId = "1d50d95c-e211-4499-9248-fa4e5dbff323";
        private static readonly string _policyEditProfile = "b2c_1_edit_profile";
        private static readonly string _policyResetPassword = "b2c_1_reset_password";
        private static readonly string _policySignUpSignIn = "b2c_1_signup_signin";
        private static readonly string _tenantName = "mediorapp";
        private readonly IChrono _chrono;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ILogger<AuthService> _logger;
        private readonly IPublicClientApplication _publicClientApp;
        private AuthenticationResult? _lastAuthResult;

        public AuthService(
            IMessagePublisher messagePublisher,
            IChrono chrono,
            ILogger<AuthService> logger)
        {
            _messagePublisher = messagePublisher;
            _chrono = chrono;
            _logger = logger;

            _publicClientApp = PublicClientApplicationBuilder.Create(_clientId)
                .WithB2CAuthority(AuthoritySignUpSignIn)
                .WithRedirectUri(RedirectUri)
#if DEBUG
                .WithLogging(Log, Microsoft.Identity.Client.LogLevel.Verbose, true)
#else
                .WithLogging(Log, Microsoft.Identity.Client.LogLevel.Info, false)
#endif
                .Build();

            TokenCacheHelper.Bind(_publicClientApp.UserTokenCache);
        }

        public string Email => _lastAuthResult
            ?.ClaimsPrincipal
            ?.FindFirst("emails")
            ?.Value ?? string.Empty;

        public bool IsSignedIn =>
                    _lastAuthResult?.AccessToken is not null &&
            _lastAuthResult.ExpiresOn > _chrono.Now;
        private string[] ApiScopes => new[] { $"https://{Tenant}/medior-api/app.user" };
        private string AuthorityBase => $"https://{AzureAdB2CHostname}/tfp/{Tenant}/";
        private string AuthorityEditProfile => $"{AuthorityBase}{_policyEditProfile}";
        private string AuthorityResetPassword => $"{AuthorityBase}{_policyResetPassword}";
        private string AuthoritySignUpSignIn => $"{AuthorityBase}{_policySignUpSignIn}";
        private string AzureAdB2CHostname => $"{_tenantName}.b2clogin.com";
        private string RedirectUri => $"https://{_tenantName}.b2clogin.com/oauth2/nativeclient";
        private string Tenant => $"{_tenantName}.onmicrosoft.com";

        public async Task<Result<AuthenticationResult>> EditProfile(IntPtr windowHandle)
        {
            try
            {
                var authResult = await _publicClientApp.AcquireTokenInteractive(ApiScopes)
                    .WithParentActivityOrWindow(windowHandle)
                    .WithB2CAuthority(AuthorityEditProfile)
                    .WithPrompt(Prompt.NoPrompt)
                    .ExecuteAsync(CancellationToken.None);

                return Result.Ok(authResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing profile.");
                UpdateSignInState(false);
                return Result.Fail<AuthenticationResult>("An error occurred while editing profile.");
            }
        }

        public async Task<Result<AuthenticationResult>> GetTokenSilently(IntPtr windowHandle, bool fallbackToInteractive)
        {
            var accounts = await _publicClientApp.GetAccountsAsync();

            try
            {
                _lastAuthResult = await _publicClientApp
                    .AcquireTokenSilent(ApiScopes, accounts.FirstOrDefault())
                    .ExecuteAsync();

                return Result.Ok(_lastAuthResult);
            }
            catch (MsalUiRequiredException ex)
            {
                _logger.LogError(ex, "Failed to acquire token silently. Interactive logon required.");
                if (fallbackToInteractive)
                {
                    return await SignInInteractive(windowHandle);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to acquire token silently.");
            }

            UpdateSignInState(false);
            return Result.Fail<AuthenticationResult>("Failed to acquire token silently.");
        }

        public async Task<Result<AuthenticationResult>> SignInInteractive(IntPtr windowHandle)
        {
            try
            {
                _lastAuthResult = await _publicClientApp
                    .AcquireTokenInteractive(ApiScopes)
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
                        _lastAuthResult = await _publicClientApp.AcquireTokenInteractive(ApiScopes)
                            .WithParentActivityOrWindow(windowHandle)
                            .WithPrompt(Prompt.SelectAccount)
                            .WithB2CAuthority(AuthorityResetPassword)
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

            if (_lastAuthResult is null)
            {
                UpdateSignInState(false);
                return Result.Fail<AuthenticationResult>("Failed to acquire auth token.");
            }

            return Result.Ok(_lastAuthResult);
        }

        public void SignOut()
        {
            _lastAuthResult = null;
            UpdateSignInState(false);
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

        private void UpdateSignInState(bool isSignedIn)
        {
            _messagePublisher.Messenger.Send(new SignInStateMessage(isSignedIn));
        }
    }
}
