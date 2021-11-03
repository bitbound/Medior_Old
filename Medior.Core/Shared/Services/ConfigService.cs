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
        Task<MediorConfig> GetConfig(string configPath);
        Task<MediorConfig> GetSortConfig();
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
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Medior", "Config.json");
            }
        }
        public async Task<MediorConfig> GetSortConfig()
        {
            try
            {
                return await GetConfig(DefaultConfigPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sort config.");
            }
            return new MediorConfig();
        }

        public async Task<MediorConfig> GetConfig(string configPath)
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
            return JsonSerializer.Deserialize<MediorConfig>(configString) ?? new();
        }
    }
}
