using Microsoft.Toolkit.Mvvm.ComponentModel;
using System.ComponentModel;

namespace Medior.BaseTypes
{
    public abstract class ViewModelBase : ObservableObject
    {
        private bool _isSignedIn;

        public virtual bool IsSignedIn
        {
            get => _isSignedIn;
            set => SetProperty(ref _isSignedIn, value);
        }

        public void InvokePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
