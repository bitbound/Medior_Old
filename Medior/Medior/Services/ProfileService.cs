using Medior.AppModules.PhotoSorter.Models;
using Medior.BaseTypes;
using Medior.Models;
using Medior.Utilities;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IProfileService
    {
        public Profile Profile { get; }
        Task<Result> Export(string path);

        Task<Result> Import(string path);

        Task Save();
    }

    public class ProfileService : IProfileService
    {
        private static readonly string _configuration = EnvironmentHelper.IsDebug ? "_Debug" : string.Empty;
        private readonly IApiService _apiService;
        private readonly IFileSystem _fileSystem;
        private readonly JsonSerializerOptions _indentedJson = new() { WriteIndented = true };
        private readonly ILogger<ProfileService> _logger;
        private readonly SemaphoreSlim _profileLock = new(1, 1);
        private readonly string _profilePath = Path.Combine(AppFolders.AppData, $"Profile{_configuration}.json");

        public ProfileService(
            IFileSystem fileSystem,
            IApiService apiService,
            ILogger<ProfileService> logger)
        {
            _fileSystem = fileSystem;
            _apiService = apiService;
            _logger = logger;
            Profile = Load();
            _ = SyncWithServer();
        }

        public Profile Profile { get; private set; }

        public async Task<Result> Export(string path)
        {
            try
            {
                await _profileLock.WaitAsync();
                var profile = JsonSerializer.Serialize(Profile, _indentedJson);
                await _fileSystem.WriteAllTextAsync(path, profile);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while exporting profile.");
            }
            finally
            {
                _profileLock.Release();
            }
            return Result.Fail("Failed to export profile.  Make sure you have access to the target directory.");
        }

        public async Task<Result> Import(string path)
        {
            try
            {
                await _profileLock.WaitAsync();
                var content = await _fileSystem.ReadAllTextAsync(path);
                var profile = JsonSerializer.Deserialize<Profile>(content);
                if (profile is not null)
                {
                    Profile = profile;
                    await SaveInternal();
                    return Result.Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing profile.");
            }
            finally
            {
                _profileLock.Release();
            }
            return Result.Fail("Failed to import profile.  Please make sure the file is accessible and contains valid JSON.");
        }

        public async Task Save()
        {
            try
            {
                await _profileLock.WaitAsync();

                await SaveInternal();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving profile.");
            }
            finally
            {
                _profileLock.Release();
            }
        }

        private Profile Load()
        {
            try
            {
                _profileLock.Wait();
                if (!_fileSystem.FileExists(_profilePath))
                {
                    return new();
                }

                var profileString = _fileSystem.ReadAllText(_profilePath);
                return JsonSerializer.Deserialize<Profile>(profileString) ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while loading profile.");
                return new();
            }
            finally
            {
                _profileLock.Release();
            }
        }

        private async Task SaveInternal()
        {
            await SyncWithServer();

            Profile.LastSaved = DateTimeOffset.Now;

            if (!File.Exists(_profilePath))
            {
                _fileSystem.CreateDirectory(Path.GetDirectoryName(_profilePath) ?? "");
            }

            await _fileSystem.WriteAllTextAsync(_profilePath, JsonSerializer.Serialize(Profile, _indentedJson));
        }

        private async Task SyncWithServer()
        {
            if (!Profile.IsCloudSyncEnabled)
            {
                return;
            }

            try
            {
                var result = await _apiService.GetProfile();

                if (!result.IsSuccess)
                {
                    _logger.LogWarning("Failed to retrieve profile from server.  Error: {err}", result.Error);
                    return;
                }

                var profile = result.Value;

                if (profile?.LastSaved > Profile.LastSaved)
                {
                    Profile = profile;
                }
                else
                {
                    await _apiService.UploadProfile(Profile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while syncing profile with server.");
            }
        }
    }
}
