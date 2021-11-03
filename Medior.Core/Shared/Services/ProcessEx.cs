using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
