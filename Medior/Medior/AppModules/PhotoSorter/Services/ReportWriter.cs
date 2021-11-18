using Medior.AppModules.PhotoSorter.Models;
using Medior.Services;
using System.Threading.Tasks;

namespace Medior.AppModules.PhotoSorter.Services
{
    public interface IReportWriter
    {
        Task<string> WriteReport(JobReport report);
        Task<string> WriteReports(IEnumerable<JobReport> reports);
    }

    public class ReportWriter : IReportWriter
    {
        private readonly IChrono _chrono;
        private readonly IFileSystem _fileSystem;

        private string LogPath => Path.Combine(Path.GetTempPath(),
            "Medior",
            $"PhotoSorter_Report_{_chrono.Now:yyyy-MM-dd HH.mm.ss.fff}.log");

        public ReportWriter(IChrono chrono, IFileSystem fileSystem)
        {
            _chrono = chrono;
            _fileSystem = fileSystem;
        }

        public async Task<string> WriteReports(IEnumerable<JobReport> reports)
        {
            var logPath = LogPath;
            foreach (var report in reports)
            {
                await WriteReportInternal(report, logPath);
            }
            return logPath;
        }

        public async Task<string> WriteReport(JobReport report)
        {
            var logPath = LogPath;
            await WriteReportInternal(report, logPath);
            return logPath;
        }

        private async Task WriteReportInternal(JobReport report, string logPath)
        {
            var errors = new List<OperationResult>();
            var wasSkipped = new List<OperationResult>();
            var noExif = new List<OperationResult>();
            var successes = new List<OperationResult>();

            foreach (var result in report.Results)
            {
                if (result.HadError)
                {
                    errors.Add(result);
                }
                if (result.WasSkipped)
                {
                    wasSkipped.Add(result);
                }
                if (!result.FoundExifData)
                {
                    noExif.Add(result);
                }
                if (result.IsSuccess)
                {
                    successes.Add(result);
                }
            }

            var reportLines = new List<string>
            {
                $"Job Name: {report.JobName}",
                $"Operation: {report.Operation}",
                $"Dry Run: {report.DryRun}",
                $"Total Files: {report.Results.Count}",
                $"Successes: {successes.Count}",
                $"Errors: {errors.Count}",
                $"Skipped: {wasSkipped.Count}",
                $"No Exif: {noExif.Count}"
            };

            reportLines.AddRange(new[]
            {
                "",
                "#### Error Files ####",
                ""
            });
            foreach (var result in errors)
            {
                reportLines.Add($"Pre-Operation Path: {result.PreOperationPath}\t" +
                    $"Post-Operation Path: {result.PostOperationPath}\t");
            }

            reportLines.AddRange(new[]
            {
                "",
                "#### Skipped Files ####",
                ""
            });
            foreach (var result in wasSkipped)
            {
                reportLines.Add($"Pre-Operation Path: {result.PreOperationPath}\t" +
                    $"Post-Operation Path: {result.PostOperationPath}\t");
            }

            reportLines.AddRange(new[]
            {
                "",
                "#### No EXIF Files ####",
                ""
            });
            foreach (var result in noExif)
            {
                reportLines.Add($"Pre-Operation Path: {result.PreOperationPath}\t" +
                    $"Post-Operation Path: {result.PostOperationPath}\t");
            }

            reportLines.AddRange(new[]
            {
                "",
                "#### Success Files ####",
                ""
            });
            foreach (var result in successes)
            {
                reportLines.Add($"Pre-Operation Path: {result.PreOperationPath}\t" +
                    $"Post-Operation Path: {result.PostOperationPath}\t");
            }

            await _fileSystem.AppendAllLinesAsync(logPath, reportLines);
        }
    }
}
