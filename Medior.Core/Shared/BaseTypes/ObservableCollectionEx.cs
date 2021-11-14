using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Medior.Core.Shared.BaseTypes
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        public void InvokeCollectionChanged()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void InvokePropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
