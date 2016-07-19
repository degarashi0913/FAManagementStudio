using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;

namespace FAManagementStudio.Views.Converters
{
    [ValueConversion(typeof(bool), typeof(double))]
    public class ImageOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? 1.0 : 0.3;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    [ValueConversion(typeof(bool), typeof(double))]
    public class ImageButtonBackColorConverter : IValueConverter
    {
        private SolidColorBrush[] color = new[] { new SolidColorBrush(), new SolidColorBrush(Colors.Yellow) };
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? color[1] : color[0];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
