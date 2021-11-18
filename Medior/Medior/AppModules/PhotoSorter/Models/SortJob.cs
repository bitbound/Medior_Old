using Medior.AppModules.PhotoSorter.Enums;
using Medior.Interfaces;
using System.Text.Json.Serialization;

namespace Medior.AppModules.PhotoSorter.Models
{
    public class SortJob : IdModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string DestinationFile { get; set; } = string.Empty;
        public string NoExifDirectory { get; set; } = string.Empty;

        public string[] ExcludeExtensions { get; set; } = Array.Empty<string>();

        public string[] IncludeExtensions { get; set; } = Array.Empty<string>();

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The operation to perform on the original files.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SortOperation Operation { get; set; }

        /// <summary>
        /// The action to take when destination file already exists.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OverwriteAction OverwriteAction { get; set; }

        public string SourceDirectory { get; set; } = string.Empty;
    }
}
