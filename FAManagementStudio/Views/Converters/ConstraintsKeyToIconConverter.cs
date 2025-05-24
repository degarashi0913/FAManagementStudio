using FAManagementStudio.Common;
using System;
using System.Globalization;
using System.Windows.Data;

namespace FAManagementStudio.Views.Converters;

[ValueConversion(typeof(ConstraintsKind), typeof(string))]
public class ConstraintsKeyToIconConverter : IValueConverter
{

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => (ConstraintsKind)value switch
        {
            ConstraintsKind.Primary => @"../Image/PrimaryKey.png",
            ConstraintsKind.Foreign => @"../Image/ForeignKey.png",
            ConstraintsKind.Primary | ConstraintsKind.Foreign => @"../Image/PFKey.png",
            _ => null,
        };

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
