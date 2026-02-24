using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ShoeStore.WpfApp.Converters
{
    public class DiscountToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal discount && discount > 0)
                return Brushes.Red;
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}