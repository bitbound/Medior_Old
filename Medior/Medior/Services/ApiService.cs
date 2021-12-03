using CommunityToolkit.Diagnostics;
using Medior.BaseTypes;
using Medior.Models.Messages;
using Medior.Utilities;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IApiService
    {
        Task<Result<HttpStatusCode>> TestAuth();
    }

    public class ApiService : IApiService
    {
        private readonly ILogger<ApiService> _logger;
        private readonly IMessagePublisher _messagePublisher;
        private readonly IAuthService _authService;
        private readonly IHttpClientFactory _httpFactory;

        public ApiService(
            IAuthService authService,
            IMessagePublisher messagePublisher,
            ILogger<ApiService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _messagePublisher = messagePublisher;
            _authService = authService;
            _httpFactory = httpClientFactory;
        }

        private string ApiEndpoint
        {
            get
            {
                if (EnvironmentHelper.IsDebug)
                {
                    return "https://localhost:7282";
                }
                return "https://api.medior.com";
            }
        }

        public async Task<Result<HttpStatusCode>> TestAuth()
        {
            try
            {
                var result = await GetConfiguredClient();
                if (!result.IsSuccess)
                {
                    return Result.Fail<HttpStatusCode>(result.Error ?? "");
                }

                Guard.IsNotNull(result.Value, nameof(result.Value));

                using var client = result.Value;

                var httpResult = await client.GetAsync($"{ApiEndpoint}/Auth/Check");

                if (httpResult.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogWarning("Auth check failed. Sign in required.");
                    _authService.SignOut();
                    _messagePublisher.Messenger.Send(new SignInStateMessage(false));
                    return Result.Fail<HttpStatusCode>("Auth check failed.  Sign-in required.");
                }

                _messagePublisher.Messenger.Send(new SignInStateMessage(true));
                return Result.Ok(httpResult.StatusCode);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError(ex, "HTTP error while testing auth.");
                return Result.Fail<HttpStatusCode>("Sign-in failed.  Please try again.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while testing auth.");
                return Result.Fail<HttpStatusCode>("Failed to contact the server.  " +
                    "Check your internet connection or try again later.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while testing auth.");
            }

            return Result.Fail<HttpStatusCode>("An unknown error occurred.");
        }

        private async Task<Result<HttpClient>> GetConfiguredClient()
        {
            var result = await _authService.GetTokenSilently(IntPtr.Zero, false);
            if (!result.IsSuccess)
            {
                return Result.Fail<HttpClient>("Authentication failed.");
            }

            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {result.Value?.AccessToken}");
            return Result.Ok(client);
        }
    }
}
