using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using MetadataExtractor.Formats.QuickTime;
using Medior.Services;
using Medior.Extensions;
using Medior.BaseTypes;
using Medior.AppModules.PhotoSorter.Models;

namespace Medior.AppModules.PhotoSorter.Services
{
    public interface IMetadataReader
    {
        Result<DateTime> ParseExifDateTime(string exifDateTime);
        Result<ExifData> TryGetExifData(string filePath);
    }

    public class MetadataReader : IMetadataReader
    {
        private readonly IFileSystem _fileSystem;

        public MetadataReader(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// Formats an EXIF DateTime to a format that can be parsed in .NET.
        /// </summary>
        /// <param name="exifDateTime"></param>
        /// <returns></returns>
        public Result<DateTime> ParseExifDateTime(string exifDateTime)
        {
            if (string.IsNullOrWhiteSpace(exifDateTime))
            {
                return Result.Fail<DateTime>($"Parameter {nameof(exifDateTime)} cannot be empty.");
            }

            if (exifDateTime.Count(character => character == ':') < 2)
            {
                return Result.Fail<DateTime>($"Parameter {nameof(exifDateTime)} appears to be invalid.");
            }

            var dateArray = exifDateTime
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Apply(split => split[0] = split[0].Replace(':', '-'));

            if (!DateTime.TryParse(string.Join(' ', dateArray), out var dateTaken))
            {
                return Result.Fail<DateTime>("Unable to parse DateTime metadata value.");
            }

            return Result.Ok(dateTaken);
        }


        public Result<ExifData> TryGetExifData(string filePath)
        {
            try
            {
                if (!_fileSystem.FileExists(filePath))
                {
                    return Result.Fail<ExifData>("File could not be found.");
                }

                if (!TryGetDateTime(filePath, out var dateTaken))
                {
                    return Result.Fail<ExifData>("DateTime is missing from metadata.");
                }

                TryGetCameraModel(filePath, out var camera);

                return Result.Ok(new ExifData()
                {
                    DateTaken = dateTaken,
                    CameraModel = camera?.Trim()
                });
            }
            catch
            {
                return Result.Fail<ExifData>("Error while reading metadata.");
            }
        }

        private bool TryGetCameraModel(string filePath, out string camera)
        {
            camera = string.Empty;

            if (!TryGetExifDirectory<ExifIfd0Directory>(filePath, out var directory))
            {
                return false;
            }

            camera = directory
                ?.GetString(ExifDirectoryBase.TagModel)
                ?.Trim()
                ?? string.Empty;

            return !string.IsNullOrWhiteSpace(camera);
        }

        private bool TryGetDateTime(string filePath, out DateTime dateTaken)
        {
            dateTaken = default;

            if (!TryGetExifDirectory<ExifSubIfdDirectory>(filePath, out var directory) ||
                directory is null)
            {
                if (!TryGetExifDirectory<QuickTimeMovieHeaderDirectory>(filePath, out var qtDir) ||
                    qtDir is null)
                {
                    return false;
                }

                return qtDir.TryGetDateTime(QuickTimeMovieHeaderDirectory.TagCreated, out dateTaken);
            }

            if (directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out dateTaken) ||
                directory.TryGetDateTime(ExifDirectoryBase.TagDateTimeDigitized, out dateTaken) ||
                directory.TryGetDateTime(ExifDirectoryBase.TagDateTime, out dateTaken))
            {
                return true;
            }

            return false;
        }

        private bool TryGetExifDirectory<T>(string filePath, out T? directory)
                    where T : class
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);

            directory = directories
                ?.OfType<T>()
                ?.FirstOrDefault();

            return directory is not null;
        }
    }
}
