using CommunityToolkit.Diagnostics;
using Medior.BaseTypes;
using Medior.Models.Messages;
using Medior.Utilities;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IApiService
    {
        Task<Result<HttpStatusCode>> TestAuth();
        Task<Result<string>> ShareImage(string filename, byte[] imageBytes);
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
                var args = Environment.GetCommandLineArgs().ToList();
                var index = args.IndexOf("--medior-api");
                if (index > -1)
                {
                    return args[index + 1];
                }

                if (EnvironmentHelper.IsDebug)
                {
                    return "https://localhost:7282";
                }
                return "https://medior-api.lucency.co";
            }
        }

        public async Task<Result<string>> ShareImage(string filename, byte[] imageBytes)
        {
            try
            {
                Guard.IsNotNull(filename, nameof(filename));
                Guard.IsNotNull(imageBytes, nameof(imageBytes));

                using var client = await GetConfiguredClient();

                var content = new MultipartFormDataContent();
                var byteContent = new ByteArrayContent(imageBytes);
                content.Add(byteContent, "formFile", filename);

                var httpResult = await client.PostAsync($"{ApiEndpoint}/Files", content);

                if (!httpResult.IsSuccessStatusCode)
                {
                    return Result.Fail<string>($"Upload failed with status code: {httpResult.StatusCode}");
                }

                var fileId = await httpResult.Content.ReadAsStringAsync();
                return Result.Ok($"{ApiEndpoint}/Files/{fileId}");
            }
            catch (AuthenticationException ex)
            {
                return Result.Fail<string>(ex.Message ?? "");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.RequestEntityTooLarge) 
            {
                return Result.Fail<string>("The image size is too large.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image upload failed.");
                return Result.Fail<string>("Image upload failed.");
            }
        }

        public async Task<Result<HttpStatusCode>> TestAuth()
        {
            try
            {
                using var client = await GetConfiguredClient();

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
            catch (AuthenticationException ex)
            {
                return Result.Fail<HttpStatusCode>(ex.Message ?? "");
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

        private async Task<HttpClient> GetConfiguredClient()
        {
            var result = await _authService.GetTokenSilently(IntPtr.Zero, false);
            if (!result.IsSuccess)
            {
                throw new AuthenticationException("Authentication failed.");
            }

            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {result.Value?.AccessToken}");
            return client;
        }
    }
}
