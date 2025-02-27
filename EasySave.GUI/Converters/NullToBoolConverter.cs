using System;
using System.Globalization;
using System.Windows.Data;

namespace EasySave.GUI.Converters
{
    public class NullToBoolConverter : IValueConverter
    {
        // Returns true if the value is not null.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}