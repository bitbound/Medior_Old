using Medior.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public class AuthService
    {
        private static readonly string _tenantName = "fabrikamb2c";
        private static readonly string _tenant = $"{_tenantName}.onmicrosoft.com";
        private static readonly string _azureAdB2CHostname = $"{_tenantName}.b2clogin.com";

        private static readonly string _clientId = "841e1190-d73a-450c-9d68-f5cf16b78e81";

        private static readonly string _redirectUri = $"https://{_tenantName}.b2clogin.com/oauth2/nativeclient";

        private static string _policySignUpSignIn = "b2c_1_susi";
        private static string _policyEditProfile = "b2c_1_edit_profile";
        private static string _policyResetPassword = "b2c_1_reset";

        private static string[] _apiScopes = { $"https://{_tenant}/helloapi/demo.read" };

        private static string _authorityBase = $"https://{_azureAdB2CHostname}/tfp/{_tenant}/";
        private static string _authoritySignUpSignIn = $"{_authorityBase}{_policySignUpSignIn}";
        private static string _authorityEditProfile = $"{_authorityBase}{_policyEditProfile}";
        private static string _authorityResetPassword = $"{_authorityBase}{_policyResetPassword}";

        private readonly ILogger<AuthService> _logger;

        public IPublicClientApplication PublicClientApp { get; private set; }

        public AuthService(ILogger<AuthService> logger)
        {
            _logger = logger;

            PublicClientApp = PublicClientApplicationBuilder.Create(_clientId)
                .WithB2CAuthority(_authoritySignUpSignIn)
                .WithRedirectUri(_redirectUri)
#if DEBUG
                .WithLogging(Log, Microsoft.Identity.Client.LogLevel.Verbose, true)
#else
                .WithLogging(Log, Microsoft.Identity.Client.LogLevel.Info, false)
#endif
                .Build();

            TokenCacheHelper.Bind(PublicClientApp.UserTokenCache);
        }

        private void Log(Microsoft.Identity.Client.LogLevel level, string message, bool containsPii)
        {
            if (message is null)
            {
                return;
            }

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
        }
    }
}
