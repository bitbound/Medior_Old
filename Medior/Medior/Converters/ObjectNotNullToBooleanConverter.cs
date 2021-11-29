using Microsoft.UI.Xaml.Data;

namespace Medior.Converters
{
    public class ObjectNotNullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue &&
             string.IsNullOrWhiteSpace(stringValue))
            {
                return true;
            }

            if (value is null)
            {
                return false;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
