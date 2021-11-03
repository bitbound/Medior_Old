using Medior.Core.PhotoSorter.Enums;
using System.Text.Json.Serialization;

namespace Medior.Core.PhotoSorter.Models
{
    public class SortJob
    {
        public string DestinationFile { get; init; } = string.Empty;
        public string NoExifDirectory { get; init; } = string.Empty;

        public string[] ExcludeExtensions { get; init; } = Array.Empty<string>();

        public string[] IncludeExtensions { get; init; } = Array.Empty<string>();

        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// The operation to perform on the original files.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SortOperation Operation { get; init; }

        /// <summary>
        /// The action to take when destination file already exists.
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OverwriteAction OverwriteAction { get; init; }

        public string SourceDirectory { get; init; } = string.Empty;
    }
}
