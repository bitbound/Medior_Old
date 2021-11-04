namespace Medior.Core.Shared.Services
{
    public interface IFileSystem
    {
        Task AppendAllLinesAsync(string path, IEnumerable<string> lines);
        void CopyFile(string sourceFile, string destinationFile, bool overwrite);
        Stream CreateFile(string filePath);
        bool FileExists(string path);
        void MoveFile(string sourceFile, string destinationFile, bool overwrite);
        Task<string> ReadAllTextAsync(string path);
    }

    public class FileSystem : IFileSystem
    {
        public Task AppendAllLinesAsync(string path, IEnumerable<string> lines)
        {
            return File.AppendAllLinesAsync(path, lines);
        }

        public void CopyFile(string sourceFile, string destinationFile, bool overwrite)
        {
            File.Copy(sourceFile, destinationFile, overwrite);
        }

        public Stream CreateFile(string filePath)
        {
            return File.Create(filePath);
        }

        public bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public void MoveFile(string sourceFile, string destinationFile, bool overwrite)
        {
            File.Move(sourceFile, destinationFile, overwrite);
        }

        public Task<string> ReadAllTextAsync(string path)
        {
            return File.ReadAllTextAsync(path);
        }
    }
}
