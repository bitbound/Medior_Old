using Microsoft.Extensions.Logging;
using Medior.Core.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Medior.Core.Shared.Services
{
    public interface IConfigService
    {
        MediorConfig Current { get; }
        MediorConfig GetConfig();
        MediorConfig GetConfig(string configPath);
        void SaveConfig();
    }

    public class ConfigService : IConfigService
    {
        public static string DefaultConfigPath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                Environment.UserName,
                "Medior",
                "Config.json");

        //public static string DefaultConfigPath =>
        //    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        //        "Medior",
        //        "Config.json");

        private static MediorConfig? _config;


        private readonly IFileSystem _fileSystem;
        private readonly ILogger<ConfigService> _logger;

        public ConfigService(IFileSystem fileSystem, ILogger<ConfigService> logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public MediorConfig Current => _config ?? (_config = GetConfig());

        public MediorConfig GetConfig()
        {
            return GetConfig(DefaultConfigPath);
        }

        public MediorConfig GetConfig(string configPath)
        {
            try
            {
                if (_config is not null)
                {
                    return _config;
                }

                if (string.IsNullOrWhiteSpace(configPath))
                {
                    throw new ArgumentNullException(nameof(configPath));
                }

                if (_fileSystem.FileExists(configPath))
                {
                    var configString = _fileSystem.ReadAllText(configPath);
                    _config = JsonSerializer.Deserialize<MediorConfig>(configString);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Medior config.");
            }
            return _config ?? new();
        }

        public void SaveConfig()
        {
            try
            {
                if (_config is null)
                {
                    return;
                }

                var dirName = Path.GetDirectoryName(DefaultConfigPath);
                if (dirName is null)
                {
                    throw new DirectoryNotFoundException(nameof(DefaultConfigPath));
                }

                _fileSystem.CreateDirectory(dirName);
                var serializedConfig = JsonSerializer.Serialize(_config);
                 _fileSystem.WriteAllText(DefaultConfigPath, serializedConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving Medior config.");
            }
        }
    }
}
