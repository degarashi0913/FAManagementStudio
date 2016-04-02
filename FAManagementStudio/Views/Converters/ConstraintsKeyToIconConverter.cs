using FAManagementStudio.Common;
using FAManagementStudio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace FAManagementStudio.Views.Converters
{
    [ValueConversion(typeof(ConstraintsKeyKind), typeof(string))]
    public class ConstraintsKeyToIconConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ConstraintsKeyKind)value)
            {
                case ConstraintsKeyKind.Primary:
                    return @"../Image/PrimaryKey.png";
                case ConstraintsKeyKind.Foreign:
                    return @"../Image/ForeigenKey.png";
                default:
                    return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
