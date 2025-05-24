﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace FAManagementStudio.Views.Converters;

[ValueConversion(typeof(Enum), typeof(bool))]
public class RadioButtonConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return false;
        return value.ToString() == parameter.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null) return Binding.DoNothing;
        if ((bool)value)
        {
            return Enum.Parse(targetType, parameter.ToString()!);
        }
        return Binding.DoNothing;
    }
}
