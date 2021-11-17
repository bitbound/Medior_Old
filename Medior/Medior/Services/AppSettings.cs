using Medior.PhotoSorter.Models;
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
        private const string SortJobsKey = "SortJobs";

        public IList<SortJob> SortJobs
        {
            get
            {
                return GetJsonObject<List<SortJob>>(SortJobsKey);
            }
            set
            {

                SetJsonObject(SortJobsKey, value);
            }
        }

        private T GetJsonObject<T>(string key)
            where T : new()
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

        private void SetJsonObject<T>(string key, T value)
        {
            if (value is null)
            {
                ApplicationData.Current.RoamingSettings.Values[key] = value;
                return;
            }

            ApplicationData.Current.RoamingSettings.Values[key] = JsonSerializer.Serialize(value);
        }
    }
}
