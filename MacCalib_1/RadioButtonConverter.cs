using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace MacCalib_1.Converters
{
    public class RadioButtonConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool b && b)
                return parameter?.ToString();

            return BindingOperations.DoNothing;
        }
    }
}
