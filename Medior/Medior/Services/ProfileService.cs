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
        private readonly IFileSystem _fileSystem;
        private readonly JsonSerializerOptions _indentedJson = new() { WriteIndented = true };
        private readonly ILogger<ProfileService> _logger;

        private readonly string _profilePath = Path.Combine(AppFolders.AppData, "Profile.json");

        public ProfileService(IFileSystem fileSystem, ILogger<ProfileService> logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            Profile = Load();
        }

        public Profile Profile { get; private set; }

        public async Task<Result> Export(string path)
        {
            try
            {
                var profile = JsonSerializer.Serialize(Profile, _indentedJson);
                await _fileSystem.WriteAllTextAsync(path, profile);
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while exporting profile.");
            }
            return Result.Fail("Failed to export profile.  Make sure you have access to the target directory.");
        }

        public async Task<Result> Import(string path)
        {
            try
            {
                var content = await _fileSystem.ReadAllTextAsync(path);
                var profile = JsonSerializer.Deserialize<Profile>(content);
                if (profile is not null)
                {
                    Profile = profile;
                    await Save();
                    return Result.Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing profile.");
            }
            return Result.Fail("Failed to import profile.  Please make sure the file is accessible and contains valid JSON.");
        }

        public async Task Save()
        {
            try
            {
                if (!File.Exists(_profilePath))
                {
                    _fileSystem.CreateDirectory(Path.GetDirectoryName(_profilePath) ?? "");
                }

                await _fileSystem.WriteAllTextAsync(_profilePath, JsonSerializer.Serialize(Profile, _indentedJson));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving profile.");
            }
        }

        private Profile Load()
        {
            try
            {
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
        }
    }
}
