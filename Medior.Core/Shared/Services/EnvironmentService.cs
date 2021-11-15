using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Core.Shared.Services
{
    public interface IEnvironmentService
    {
        bool IsDebug { get; }
    }

    public class EnvironmentService : IEnvironmentService
    {
        public bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
