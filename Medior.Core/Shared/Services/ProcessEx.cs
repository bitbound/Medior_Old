using System.Diagnostics;

namespace Medior.Core.Shared.Services
{
    public interface IProcessEx
    {
        void Start(string fileName);
    }

    public class ProcessEx : IProcessEx
    {
        public void Start(string fileName)
        {
            Process.Start(fileName);
        }
    }
}
