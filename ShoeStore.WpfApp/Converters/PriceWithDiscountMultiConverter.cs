using System;
using System.Globalization;
using System.Windows.Data;

namespace ShoeStore.WpfApp.Converters
{
    public class PriceWithDiscountMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is decimal price && values[1] is decimal discount)
            {
                decimal result = price * (1 - discount / 100);
                return result.ToString("N2", culture);
            }
            return "0.00";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}