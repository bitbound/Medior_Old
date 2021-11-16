using CommunityToolkit.Diagnostics;
using System.Diagnostics;

namespace Medior.Core.Shared.Services
{
    public interface IProcessEx
    {
        Process Start(string fileName);
        Process? Start(ProcessStartInfo startInfo);
    }

    public class ProcessEx : IProcessEx
    {
        public Process Start(string fileName)
        {
            return Process.Start("explorer.exe");
        }

        public Process? Start(ProcessStartInfo startInfo)
        {
            Guard.IsNotNull(startInfo, nameof(startInfo));
            return Process.Start(startInfo);
        }
    }
}
