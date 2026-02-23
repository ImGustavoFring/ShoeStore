using System.Windows;

namespace ShoeStore.WpfApp.Views
{
    public partial class ProductsWindow : Window
    {
        public ProductsWindow()
        {
            InitializeComponent();
        }

        // Пустые обработчики для кнопок (реализовать позже)
        private void AddProduct_Click(object sender, RoutedEventArgs e) { }
        private void EditProduct_Click(object sender, RoutedEventArgs e) { }
        private void DeleteProduct_Click(object sender, RoutedEventArgs e) { }
        private void OpenOrders_Click(object sender, RoutedEventArgs e) { }
    }
}