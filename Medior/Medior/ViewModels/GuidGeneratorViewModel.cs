using Medior.BaseTypes;

namespace Medior.ViewModels
{
    public class GuidGeneratorViewModel : ViewModelBase
    {
        private string _currentGuid = Guid.NewGuid().ToString();

        public string CurrentGuid
        {
            get => _currentGuid;
            set => SetProperty(ref _currentGuid, value);
        }
    }
}
