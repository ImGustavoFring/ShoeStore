using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using ShoeStore.WpfApp.Data;
using ShoeStore.WpfApp.Models;

namespace ShoeStore.WpfApp.Views
{
    public partial class ProductEditWindow : Window
    {
        private Product _editingProduct;
        private bool _isNew;
        private string _selectedImagePath;

        public ProductEditWindow(Product product)
        {
            InitializeComponent();
            _editingProduct = product;
            _isNew = product == null;
            LoadReferenceData();
            if (!_isNew) LoadProductData();
        }

        private void LoadReferenceData()
        {
            using var context = new ShoeStoreDbContext();

            CategoryComboBox.ItemsSource = context.Categories.OrderBy(c => c.Name).ToList();
            ManufacturerComboBox.ItemsSource = context.Manufacturers.OrderBy(m => m.Name).ToList();
            SupplierComboBox.ItemsSource = context.Suppliers.OrderBy(s => s.Name).ToList();
            UnitComboBox.ItemsSource = context.Units.OrderBy(u => u.Name).ToList();
            ArticleComboBox.ItemsSource = context.Articles.OrderBy(a => a.Title).ToList();
        }

        private void LoadProductData()
        {
            NameTextBox.Text = _editingProduct.Name;
            DescriptionTextBox.Text = _editingProduct.Description;
            PhotoPathTextBox.Text = _editingProduct.PhotoPath;
            CategoryComboBox.SelectedItem = _editingProduct.Category;
            ManufacturerComboBox.SelectedItem = _editingProduct.Manufacturer;
            SupplierComboBox.SelectedItem = _editingProduct.Supplier;
            UnitComboBox.SelectedItem = _editingProduct.Unit;
            PriceTextBox.Text = _editingProduct.Price.ToString("F2");
            DiscountTextBox.Text = _editingProduct.Discount.ToString("F2");
            QuantityTextBox.Text = _editingProduct.QuantityInStock.ToString();
            ArticleComboBox.SelectedItem = _editingProduct.Article;
            ArticleComboBox.Text = _editingProduct.Article.Title;
        }

        private void BrowseImageButton_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Image files|*.png;*.jpg;*.jpeg;*.bmp|All files|*.*"
            };
            if (ofd.ShowDialog() == true)
            {
                _selectedImagePath = ofd.FileName;
                PhotoPathTextBox.Text = _selectedImagePath;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (string.IsNullOrWhiteSpace(NameTextBox.Text) ||
                    !decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0 ||
                    !decimal.TryParse(DiscountTextBox.Text, out decimal discount) || discount < 0 || discount > 100 ||
                    !long.TryParse(QuantityTextBox.Text, out long quantity) || quantity < 0 ||
                    CategoryComboBox.SelectedItem == null || ManufacturerComboBox.SelectedItem == null ||
                    SupplierComboBox.SelectedItem == null || UnitComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Проверьте введённые данные", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Артикул
                Article selectedArticle = ArticleComboBox.SelectedItem as Article;
                string newArticleTitle = ArticleComboBox.Text.Trim();
                if (selectedArticle == null && string.IsNullOrWhiteSpace(newArticleTitle))
                {
                    MessageBox.Show("Выберите или введите артикул", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using var context = new ShoeStoreDbContext();

                Article article;

                if (selectedArticle != null)
                    article = context.Articles.Find(selectedArticle.Id);
                else
                {
                    if (context.Articles.Any(a => a.Title == newArticleTitle))
                    {
                        MessageBox.Show("Такой артикул уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    article = new Article { Title = newArticleTitle };

                    context.Articles.Add(article);

                    context.SaveChanges(); // получить Id
                }

                Product product;
                if (_isNew)
                {
                    product = new Product();
                    context.Products.Add(product);
                }
                else
                {
                    product = context.Products.Find(_editingProduct.Id);
                }

                product.ArticleId = article.Id;
                product.Name = NameTextBox.Text.Trim();
                product.Description = DescriptionTextBox.Text.Trim() ?? "";
                product.CategoryId = ((Category)CategoryComboBox.SelectedItem).Id;
                product.ManufacturerId = ((Manufacturer)ManufacturerComboBox.SelectedItem).Id;
                product.SupplierId = ((Supplier)SupplierComboBox.SelectedItem).Id;
                product.UnitId = ((Unit)UnitComboBox.SelectedItem).Id;
                product.Price = price;
                product.Discount = discount;
                product.QuantityInStock = quantity;

                context.SaveChanges(); // получить Id для изображения

                // Сохранение изображения
                if (!string.IsNullOrEmpty(_selectedImagePath))
                {
                    string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    Directory.CreateDirectory(imagesDir);
                    string ext = Path.GetExtension(_selectedImagePath);
                    string fileName = $"product_{product.Id}{ext}";
                    string dest = Path.Combine(imagesDir, fileName);

                    // Изменение размера до 300x200
                    using (var img = System.Drawing.Image.FromFile(_selectedImagePath))
                    using (var resized = new System.Drawing.Bitmap(300, 200))
                    {
                        using (var g = System.Drawing.Graphics.FromImage(resized))
                            g.DrawImage(img, 0, 0, 300, 200);
                        resized.Save(dest, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }

                    // Удаление старого фото, если есть
                    if (!_isNew && !string.IsNullOrEmpty(_editingProduct.PhotoPath))
                    {
                        string oldFull = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _editingProduct.PhotoPath);
                        if (File.Exists(oldFull) && oldFull != dest)
                            File.Delete(oldFull);
                    }

                    product.PhotoPath = $"Images/{fileName}";
                }
                else if (!_isNew && string.IsNullOrEmpty(_selectedImagePath) && PhotoPathTextBox.Text != _editingProduct.PhotoPath)
                {
                    // Пользователь очистил поле – удаляем старое фото
                    if (!string.IsNullOrEmpty(_editingProduct.PhotoPath))
                    {
                        string oldFull = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _editingProduct.PhotoPath);
                        if (File.Exists(oldFull)) File.Delete(oldFull);
                    }
                    product.PhotoPath = null;
                }

                context.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}