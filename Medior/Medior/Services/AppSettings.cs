using Medior.Core.PhotoSorter.Models;
using Medior.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using Windows.Storage;

namespace Medior.Services
{
    public interface IAppSettings
    {
        IList<SortJob> SortJobs { get; set; }
    }

    public class AppSettings : IAppSettings
    {
        private const string MediorConfigKey = "MediorConfig";

        public MediorConfig Config { get; } = GetJsonObject<MediorConfig>(MediorConfigKey);

        public IList<SortJob> SortJobs
        {
            get
            {
                return Config.SortJobs;
            }
            set
            {

                Config.SortJobs = value ?? new List<SortJob>();
                ApplicationData.Current.LocalSettings.Values[MediorConfigKey] = JsonSerializer.Serialize(Config);
            }
        }

        private static T GetJsonObject<T>(string key)
            where T: new()
        {
            try
            {
                var serialized = ApplicationData.Current.LocalSettings.Values[key]?.ToString();

                if (serialized is null)
                {
                    return new();
                }

                return JsonSerializer.Deserialize<T>(serialized) ?? new();
            }
            catch
            {
                return new();
            }
        }
    }
}
