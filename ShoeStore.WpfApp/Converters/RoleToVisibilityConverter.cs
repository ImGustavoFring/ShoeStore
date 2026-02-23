using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ShoeStore.WpfApp.Converters
{
    public class RoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            string role = value.ToString();
            string param = parameter.ToString();

            if (param == "Admin" && role == "Администратор") return Visibility.Visible;
            if (param == "ManagerOrAdmin" && (role == "Менеджер" || role == "Администратор")) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}