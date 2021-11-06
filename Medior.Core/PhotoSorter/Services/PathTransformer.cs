using Medior.Core.Shared.Services;

namespace Medior.Core.PhotoSorter.Services
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
        public const string Day = "{dd}";
        public const string Extension = "{extension}";
        public const string Filename = "{filename}";
        public const string Hour = "{HH}";
        public const string Minute = "{mm}";
        public const string Second = "{ss}";
        public const string Millisecond = "{fff}";
        public const string Month = "{MM}";
        public const string Year = "{yyyy}";

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
                .Replace(Year, dateTaken.Year.ToString().PadLeft(4, '0'))
                .Replace(Month, dateTaken.Month.ToString().PadLeft(2, '0'))
                .Replace(Day, dateTaken.Day.ToString().PadLeft(2, '0'))
                .Replace(Hour, dateTaken.Hour.ToString().PadLeft(2, '0'))
                .Replace(Minute, dateTaken.Minute.ToString().PadLeft(2, '0'))
                .Replace(Second, dateTaken.Second.ToString().PadLeft(2, '0'))
                .Replace(Millisecond, dateTaken.Millisecond.ToString().PadLeft(3, '0'))
                .Replace(Camera, camera?.Trim())
                .Replace(Filename, Path.GetFileNameWithoutExtension(sourceFile))
                .Replace(Extension, Path.GetExtension(sourceFile)[1..]);
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
