using Medior.Core.PhotoSorter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace Medior.Services
{
    public interface IAppSettings
    {
        IList<SortJob> SortJobs { get; set; }
        IList<Guid> FavoriteModules { get; set; }
    }

    public class AppSettings : IAppSettings
    {
        private const string SortJobsKey = "SortJobs";
        private const string FavoriteModulesKey = "FavoriteModules";

        public IList<SortJob> SortJobs
        {
            get
            {
                return GetJsonObjectRoaming<List<SortJob>>(SortJobsKey);
            }
            set
            {
                
                if (value is null)
                {
                    ApplicationData.Current.RoamingSettings.Values[SortJobsKey] = value;
                    return;
                }
                
                ApplicationData.Current.RoamingSettings.Values[SortJobsKey] = JsonSerializer.Serialize(value);
            }
        }

        public IList<Guid> FavoriteModules
        {
            get
            {
                var favorites = ApplicationData.Current.RoamingSettings.Values[FavoriteModulesKey];
                if (favorites is IList<Guid> favoritesList)
                {
                    return favoritesList;
                }
                return new List<Guid>();
            }
            set
            {
                ApplicationData.Current.RoamingSettings.Values[FavoriteModulesKey] = JsonSerializer.Serialize(value);
            }
        }

        private T GetJsonObjectRoaming<T>(string key)
            where T: new()
        {
            try
            {
                var serialized = ApplicationData.Current.RoamingSettings.Values[key]?.ToString();

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
