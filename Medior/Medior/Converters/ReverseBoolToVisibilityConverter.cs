using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Medior.Converters
{
    public class ReverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool boolValue)
            {
                if (boolValue)
                {
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility visValue)
            {
                if (visValue == Visibility.Collapsed)
                {
                    return true;
                }

                return false;
            }

            return false;
        }
    }
}
