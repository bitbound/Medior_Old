using CommunityToolkit.Diagnostics;
using Medior.BaseTypes;
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
        private readonly IEnvironmentService _environment;
        private readonly IAuthService _authService;
        private readonly IHttpClientFactory _httpFactory;

        public ApiService(
            IEnvironmentService environmentService,
            IAuthService authService,
            ILogger<ApiService> logger,
            IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _environment = environmentService;
            _authService = authService;
            _httpFactory = httpClientFactory;
        }

        private string ApiEndpoint
        {
            get
            {
                if (_environment.IsDebug)
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
                }

                return Result.Ok(httpResult.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while testing auth.");
            }

            return Result.Fail<HttpStatusCode>("Failed to call API.");
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
