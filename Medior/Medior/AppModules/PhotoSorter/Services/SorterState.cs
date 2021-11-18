namespace Medior.AppModules.PhotoSorter.Services
{
    public interface ISorterState
    {
        string ConfigPath { get; init; }
        bool DryRun { get; init; }
        string JobName { get; init; }
        bool Once { get; init; }
    }

    public class SorterState : ISorterState
    {
        public string ConfigPath { get; init; } = string.Empty;
        public string JobName { get; init; } = string.Empty;
        public bool DryRun { get; init; }
        public bool Once { get; init; }
    }
}
