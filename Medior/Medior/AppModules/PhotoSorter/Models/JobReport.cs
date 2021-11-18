using Medior.AppModules.PhotoSorter.Enums;

namespace Medior.AppModules.PhotoSorter.Models
{
    public class JobReport
    {
        public string JobName { get; set; } = string.Empty;
        public SortOperation Operation { get; set; }
        public List<OperationResult> Results { get; set; } = new();
        public bool DryRun { get; set; }
        public string ReportPath { get; set; } = string.Empty;
    }
}
