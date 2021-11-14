namespace Medior.Core.PhotoSorter.Models
{
    public class OperationResult
    {
        public bool IsSuccess => !WasSkipped && !HadError && FoundExifData;
        public bool WasSkipped { get; set; }
        public bool HadError { get; set; }
        public bool FoundExifData { get; set; }
        public string PreOperationPath { get; set; } = string.Empty;
        public string PostOperationPath { get; set; } = string.Empty;

    }
}
