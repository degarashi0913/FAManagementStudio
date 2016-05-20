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
    [ValueConversion(typeof(TableKind), typeof(string))]
    public class TableIconConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch ((TableKind)value)
            {
                case TableKind.Table:
                    return @"../Image/Table.png";
                case TableKind.View:
                    return @"../Image/View.png";
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
