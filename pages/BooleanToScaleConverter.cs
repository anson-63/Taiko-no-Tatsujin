using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Taiko.pages
{
    public class BooleanToScaleConverter : IValueConverter
    {
        public double Half { get; set; } = 0.5;
        public double Full { get; set; } = 1.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (value is bool b && b) ? Half : Full;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
