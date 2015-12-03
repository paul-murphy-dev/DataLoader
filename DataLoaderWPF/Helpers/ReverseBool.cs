using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace DataLoaderWPF.Helpers
{
    [ValueConversion(typeof(bool), typeof(bool))]
    public class ReverseBool : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((bool)value)
            {
                case true:
                    return false;
                default:
                    return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((bool)value)
            {
                case false:
                    return true;
                default:
                    return false;
            }
        }
    }
}
