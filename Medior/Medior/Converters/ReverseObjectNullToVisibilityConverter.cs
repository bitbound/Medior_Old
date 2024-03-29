﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Medior.Converters
{
    public class ReverseObjectNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string stringValue &&
                string.IsNullOrWhiteSpace(stringValue))
            {
                return Visibility.Visible;
            }

            if (value is null)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
