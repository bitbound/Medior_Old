using CommunityToolkit.Diagnostics;
using Medior.BaseTypes;
using Medior.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.ViewModels
{
    public class ElevatorViewModel : ViewModelBase
    {
        private string? _commandLine;
        private readonly IProcessEx _processEx;
        private readonly IResourceExtractor _resourceExtractor;
        private readonly ILogger<ElevatorViewModel> _logger;

        public string? CommandLine
        {
            get => _commandLine;
            set => SetProperty(ref _commandLine, value);
        }

        public ElevatorViewModel(IProcessEx processEx, 
            IResourceExtractor resourceExtractor,
            ILogger<ElevatorViewModel> logger)
        {
            _processEx = processEx;
            _resourceExtractor = resourceExtractor;
            _logger = logger;
        }

        public async Task<Result> LaunchAsSystem(string commandLine)
        {
            try
            {
                Guard.IsNotNullOrWhiteSpace(commandLine, nameof(commandLine));

                var targetPath = Path.Combine(Path.GetTempPath(), "Medior", "bin", "paexec.exe");
                var result = await _resourceExtractor.ExtractPaExec(targetPath);

                if (!result.IsSuccess)
                {
                    return result;
                }

                var expanded = Environment.ExpandEnvironmentVariables(commandLine);
                var parts = expanded.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var filename = parts.First();
                var args = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : "";

                var psi = new ProcessStartInfo()
                {
                    FileName = targetPath,
                    Arguments = $"-s -i {expanded}",
                    Verb = "RunAs",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };

                var proc = _processEx.Start(psi);

                if (proc is null)
                {
                    return Result.Fail("Failed to launch the process.");
                }

                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to launch process as admin.");
                return Result.Fail(ex.Message);
            }
        }
    }
}
