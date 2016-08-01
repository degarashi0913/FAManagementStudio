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
    [ValueConversion(typeof(ConstraintsKind), typeof(string))]
    public class ConstraintsKeyToIconConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((ConstraintsKind)value)
            {
                case ConstraintsKind.Primary:
                    return @"../Image/PrimaryKey.png";
                case ConstraintsKind.Foreign:
                    return @"../Image/ForeigenKey.png";
                case ConstraintsKind.Primary | ConstraintsKind.Foreign:
                    return @"../Image/PFKey.png";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
