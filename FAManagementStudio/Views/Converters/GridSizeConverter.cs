using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace FAManagementStudio.Views.Converters
{
    [ValueConversion(typeof(double), typeof(double))]
    public class GridHightSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tmp = (double)value - 58d;
            return tmp < 20 ? 20 : tmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(double), typeof(double))]
    public class GridWidthSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tmp = (double)value - 38d;
            return tmp < 40 ? 40 : tmp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
