namespace Medior.Services
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
