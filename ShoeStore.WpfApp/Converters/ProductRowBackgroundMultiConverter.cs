using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace ShoeStore.WpfApp.Converters
{
    public class ProductRowBackgroundMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is decimal discount && values[1] is long quantity)
            {
                if (quantity == 0)
                    return new SolidColorBrush(Colors.LightBlue);
                if (discount > 15)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2E8B57"));
            }
            return Brushes.Transparent;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}