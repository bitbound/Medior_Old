using Medior.Services;
using Medior.Shared.Services;
using System.IO;

namespace Medior.AppModules.PhotoSorter.Services
{
    public interface IPathTransformer
    {
        string TransformPath(string sourcePath, string destinationPath);
        string TransformPath(string sourcePath, string destinationPath, DateTime dateTaken, string? camera);
        string GetUniqueFilePath(string destinationFile);
        string TransformPath(string sourcePath, string destinationPath, DateTime fileCreated);
    }

    public class PathTransformer : IPathTransformer
    {
        public const string Camera = "{camera}";
        public const string Day = "{day}";
        public const string Extension = "{extension}";
        public const string Filename = "{filename}";
        public const string Hour = "{hour}";
        public const string Minute = "{minute}";
        public const string Second = "{second}";
        public const string Millisecond = "{millisecond}";
        public const string Month = "{month}";
        public const string Year = "{year}";

        private readonly IFileSystem _fileSystem;

        public PathTransformer(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string GetUniqueFilePath(string destinationFile)
        {
            var uniquePath = destinationFile;

            for (var i = 0; true; i++)
            {
                if (!_fileSystem.FileExists(uniquePath))
                {
                    break;
                }

                var filename =
                    Path.GetFileNameWithoutExtension(destinationFile) +
                    $"_{i}" +
                    Path.GetExtension(destinationFile);

                var dirName = Path.GetDirectoryName(destinationFile);

                if (string.IsNullOrWhiteSpace(dirName))
                {
                    throw new DirectoryNotFoundException();
                }

                uniquePath = Path.Combine(dirName, filename);
            }

            return uniquePath;
        }

        public string TransformPath(string sourceFile, string destinationFile, DateTime dateTaken, string? camera)
        {
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            if (string.IsNullOrWhiteSpace(destinationFile))
            {
                throw new ArgumentNullException(nameof(destinationFile));
            }

            return destinationFile
                .Replace(Year, dateTaken.Year.ToString().PadLeft(4, '0'), StringComparison.OrdinalIgnoreCase)
                .Replace(Month, dateTaken.Month.ToString().PadLeft(2, '0'), StringComparison.OrdinalIgnoreCase)
                .Replace(Day, dateTaken.Day.ToString().PadLeft(2, '0'), StringComparison.OrdinalIgnoreCase)
                .Replace(Hour, dateTaken.Hour.ToString().PadLeft(2, '0'), StringComparison.OrdinalIgnoreCase)
                .Replace(Minute, dateTaken.Minute.ToString().PadLeft(2, '0'), StringComparison.OrdinalIgnoreCase)
                .Replace(Second, dateTaken.Second.ToString().PadLeft(2, '0'), StringComparison.OrdinalIgnoreCase)
                .Replace(Millisecond, dateTaken.Millisecond.ToString().PadLeft(3, '0'), StringComparison.OrdinalIgnoreCase)
                .Replace(Camera, camera?.Trim(), StringComparison.OrdinalIgnoreCase)
                .Replace(Filename, Path.GetFileNameWithoutExtension(sourceFile), StringComparison.OrdinalIgnoreCase)
                .Replace(Extension, Path.GetExtension(sourceFile)[1..], StringComparison.OrdinalIgnoreCase);
        }

        public string TransformPath(string sourceFile, string destinationFile)
        {
            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                throw new ArgumentNullException(nameof(sourceFile));
            }

            if (string.IsNullOrWhiteSpace(destinationFile))
            {
                throw new ArgumentNullException(nameof(destinationFile));
            }

            return destinationFile
                .Replace(Filename, Path.GetFileNameWithoutExtension(sourceFile))
                .Replace(Extension, Path.GetExtension(sourceFile)[1..]);
        }

        public string TransformPath(string sourcePath, string destinationPath, DateTime fileCreated)
        {
            return TransformPath(sourcePath, destinationPath, fileCreated, string.Empty);
        }
    }
}
