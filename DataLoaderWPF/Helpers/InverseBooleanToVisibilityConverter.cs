using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DataLoaderWPF.Helpers
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((bool)value)
            {
                case true:
                    return Visibility.Hidden;
                default:
                    return Visibility.Visible;
            }            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((Visibility)value)
            {
                case Visibility.Visible:
                    return false;
                default:
                    return true;
            }
        }
    }
}
