using Medior.BaseTypes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Services
{
    public interface IResourceExtractor
    {
        Task<Result> ExtractPaExec(string outputFilePath);
    }

    public class ResourceExtractor : IResourceExtractor
    {
        private readonly string _paExecResourcePath = "Medior.Resources.paexec.exe";
        private readonly IFileSystem _fileSystem;
        private readonly ILogger<ResourceExtractor> _logger;

        public ResourceExtractor(IFileSystem fileSystem, ILogger<ResourceExtractor> logger)
        {
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public async Task<Result> ExtractPaExec(string outputFilePath)
        {
            try
            {
                if (!_fileSystem.FileExists(outputFilePath))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(outputFilePath) ?? "");
                }

                using var fs = new FileStream(outputFilePath, FileMode.Create);
                using var mrs = typeof(ResourceExtractor).Assembly.GetManifestResourceStream(_paExecResourcePath);

                if (mrs is null)
                {
                    _logger.LogWarning("PaExec not found in resources.");
                    return Result.Fail("PaExec not found in resources.");
                }

                await mrs.CopyToAsync(fs);

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extract PaExec.");
                return Result.Fail("Failed to extract PaExec.");
            }
        }
    }
}
