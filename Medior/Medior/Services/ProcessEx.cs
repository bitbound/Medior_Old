using System.Diagnostics;

namespace Medior.Services
{
    public interface IProcessEx
    {
        Process Start(string fileName);
        Process Start(string fileName, string arguments);
        Process? Start(ProcessStartInfo startInfo);
    }

    public class ProcessEx : IProcessEx
    {
        public Process Start(string fileName)
        {
            return Process.Start("explorer.exe");
        }

        public Process Start(string fileName, string arguments)
        {
            return Process.Start("explorer.exe", arguments);
        }

        public Process? Start(ProcessStartInfo startInfo)
        {
            if (startInfo is null)
            {
                throw new ArgumentNullException(nameof(startInfo));
            }
            return Process.Start(startInfo);
        }
    }
}
