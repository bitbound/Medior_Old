using System.Diagnostics;

namespace Medior.Core.Shared.Services
{
    public interface IProcessEx
    {
        void Start(string fileName);
        void Start(ProcessStartInfo startInfo);
    }

    public class ProcessEx : IProcessEx
    {
        public void Start(string fileName)
        {
            Process.Start("explorer.exe");
        }

        public void Start(ProcessStartInfo startInfo)
        {
            Process.Start(startInfo);
        }
    }
}
