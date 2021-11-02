using Microsoft.Extensions.Logging;
using Medior.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Medior.Core.Services
{
    public interface IConfigService
    {
        Task<SortConfig> GetConfig(string configPath);
        Task<SortConfig> GetSortConfig();
    }

    public class ConfigService : IConfigService
    {
        private readonly ILogger<ConfigService> _logger;

        public ConfigService(ILogger<ConfigService> logger)
        {
            _logger = logger;
        }

        private string DefaultConfigPath
        {
            get
            {
                if (OperatingSystem.IsWindows())
                {
                    return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Medior", "Config.json");
                }
                else
                {
                    return Path.Combine(Path.GetTempPath(), "Medior", "Config.json");
                }
            }
        }
        public async Task<SortConfig> GetSortConfig()
        {
            try
            {
                return await GetConfig(DefaultConfigPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sort config.");
            }
            return new SortConfig();
        }

        public async Task<SortConfig> GetConfig(string configPath)
        {

            if (string.IsNullOrWhiteSpace(configPath))
            {
                throw new ArgumentNullException(nameof(configPath));
            }

            if (!File.Exists(configPath))
            {
                return new();
            }

            var configString = await File.ReadAllTextAsync(configPath);
            return JsonSerializer.Deserialize<SortConfig>(configString) ?? new();
        }
    }
}
