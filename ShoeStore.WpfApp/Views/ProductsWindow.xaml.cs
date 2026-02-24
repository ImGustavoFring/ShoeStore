using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ShoeStore.WpfApp.Data;
using ShoeStore.WpfApp.Models;

namespace ShoeStore.WpfApp.Views
{
    public partial class ProductsWindow : Window
    {
        private User _currentUser;
        private string _currentUserRole;
        private string _currentUserFullName;
        private ObservableCollection<Product> _allProducts;
        private bool _isEditWindowOpen = false;
        private bool _isLoaded = false;

        public string CurrentUserRole => _currentUserRole;
        public string CurrentUserFullName => _currentUserFullName;
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        public ProductsWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _currentUserRole = user?.Role?.Name ?? "Гость";
            _currentUserFullName = user?.FullName ?? "Гость";
            DataContext = this;

            LoadSuppliers();
            LoadProducts();
            _isLoaded = true;
            ApplyFilter(); // применить фильтр после полной загрузки
        }

        private void LoadSuppliers()
        {
            using (var context = new ShoeStoreDbContext())
            {
                var suppliers = context.Suppliers.OrderBy(s => s.Name).ToList();

                // Создаём новый список, начинающийся с "Все поставщики"
                var supplierList = new List<Supplier> { new Supplier { Id = 0, Name = "Все поставщики" } };
                supplierList.AddRange(suppliers);

                SupplierFilterComboBox.ItemsSource = supplierList;
                SupplierFilterComboBox.DisplayMemberPath = "Name";
                SupplierFilterComboBox.SelectedIndex = 0;
            }
        }

        private void LoadProducts()
        {
            try
            {
                using (var context = new ShoeStoreDbContext())
                {
                    var list = context.Products
                        .Include(p => p.Article)
                        .Include(p => p.Category)
                        .Include(p => p.Manufacturer)
                        .Include(p => p.Supplier)
                        .Include(p => p.Unit)
                        .ToList();
                    _allProducts = new ObservableCollection<Product>(list);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                _allProducts = new ObservableCollection<Product>(); // пустая коллекция вместо null
            }
        }

        private void ApplyFilter()
        {
            if (_allProducts == null) return;

            var filtered = _allProducts.AsEnumerable();

            // Поиск
            string search = SearchTextBox?.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(search))
            {
                filtered = filtered.Where(p =>
                    (p.Article?.Title?.ToLower().Contains(search) ?? false) ||
                    (p.Name?.ToLower().Contains(search) ?? false) ||
                    (p.Description?.ToLower().Contains(search) ?? false) ||
                    (p.Category?.Name?.ToLower().Contains(search) ?? false) ||
                    (p.Manufacturer?.Name?.ToLower().Contains(search) ?? false) ||
                    (p.Supplier?.Name?.ToLower().Contains(search) ?? false));
            }

            // Фильтр по поставщику
            if (SupplierFilterComboBox?.SelectedItem is Supplier selected && selected.Id != 0)
                filtered = filtered.Where(p => p.SupplierId == selected.Id);

            // Сортировка
            string sortBy = (SortComboBox?.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "По названию";
            switch (sortBy)
            {
                case "По названию": filtered = filtered.OrderBy(p => p.Name); break;
                case "По цене (возр.)": filtered = filtered.OrderBy(p => p.Price * (1 - p.Discount / 100)); break;
                case "По цене (убыв.)": filtered = filtered.OrderByDescending(p => p.Price * (1 - p.Discount / 100)); break;
                case "По количеству (возр.)": filtered = filtered.OrderBy(p => p.QuantityInStock); break;
                case "По количеству (убыв.)": filtered = filtered.OrderByDescending(p => p.QuantityInStock); break;
            }

            Products.Clear();
            foreach (var p in filtered)
                Products.Add(p);
        }

        // Обработчики фильтрации
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoaded) ApplyFilter();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded) ApplyFilter();
        }

        private void SupplierFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoaded) ApplyFilter();
        }

        // CRUD
        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_isEditWindowOpen) { ShowEditWarning(); return; }
            var win = new ProductEditWindow(null) { Owner = this };
            try { _isEditWindowOpen = true; if (win.ShowDialog() == true) LoadProducts(); }
            finally { _isEditWindowOpen = false; }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is not Product selected)
            {
                MessageBox.Show("Выберите товар", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (_isEditWindowOpen) { ShowEditWarning(); return; }
            var win = new ProductEditWindow(selected) { Owner = this };
            try { _isEditWindowOpen = true; if (win.ShowDialog() == true) LoadProducts(); }
            finally { _isEditWindowOpen = false; }
        }

        private void ShowEditWarning() =>
            MessageBox.Show("Окно редактирования уже открыто", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsDataGrid.SelectedItem is not Product selected) return;
            if (MessageBox.Show($"Удалить товар \"{selected.Name}\"?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using var context = new ShoeStoreDbContext();
                bool hasOrders = await context.OrderItems.AnyAsync(oi => oi.ArticleId == selected.ArticleId);
                if (hasOrders)
                {
                    MessageBox.Show("Товар присутствует в заказах и не может быть удалён", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Удаление файла изображения
                if (!string.IsNullOrEmpty(selected.PhotoPath))
                {
                    string full = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, selected.PhotoPath);
                    if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
                }

                var product = await context.Products.Include(p => p.Article)
                    .FirstOrDefaultAsync(p => p.Id == selected.Id);
                if (product != null)
                {
                    context.Products.Remove(product);
                    context.Articles.Remove(product.Article);
                    await context.SaveChangesAsync();
                }
                LoadProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ProductsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentUserRole == "Администратор" && ProductsDataGrid.SelectedItem is Product)
                EditProduct_Click(sender, null);
        }

        private void OpenOrders_Click(object sender, RoutedEventArgs e) =>
            MessageBox.Show("Работа с заказами не входит в базовый уровень", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            Close();
        }
    }
}