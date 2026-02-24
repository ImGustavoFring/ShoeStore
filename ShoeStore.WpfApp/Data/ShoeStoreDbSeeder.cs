using OfficeOpenXml;
using ShoeStore.WpfApp.Models;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ShoeStore.WpfApp.Data
{
    public class ShoeStoreDbSeeder
    {
        private readonly ShoeStoreDbContext _context;

        public ShoeStoreDbSeeder(ShoeStoreDbContext context)
        {
            _context = context;
        }

        public void Seed(string usersFilePath, string productsFilePath, string pickUpPointsFilePath, string ordersFilePath)
        {
            // Пересоздаём базу данных

            ExcelPackage.License.SetNonCommercialPersonal("Gustavo");

            // Словари для быстрого поиска справочников
            var roles = new Dictionary<string, Role>();
            var statuses = new Dictionary<string, Status>();
            var units = new Dictionary<string, Unit>();
            var suppliers = new Dictionary<string, Supplier>();
            var manufacturers = new Dictionary<string, Manufacturer>();
            var categories = new Dictionary<string, Category>();
            var articles = new Dictionary<string, Article>();
            var pickUpPointsByIndex = new Dictionary<int, PickUpPoint>();

            // 1. Заполнение справочников (кроме Article)
            // Роли из user_import.xlsx
            using (var package = new ExcelPackage(new FileInfo(usersFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                var roleNames = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string roleName = worksheet.Cells[row, 1].Text.Trim();
                    if (!string.IsNullOrEmpty(roleName))
                        roleNames.Add(roleName);
                }
                foreach (var name in roleNames)
                {
                    var role = new Role { Name = name };
                    roles[name] = role;
                    _context.Roles.Add(role);
                }
            }

            // Статусы заказов
            var orderStatuses = new[] { "Новый", "Завершен" };
            foreach (var name in orderStatuses)
            {
                var status = new Status { Name = name };
                statuses[name] = status;
                _context.Statuses.Add(status);
            }

            // Единицы измерения из Tovar.xlsx
            using (var package = new ExcelPackage(new FileInfo(productsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                var unitNames = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string unitName = worksheet.Cells[row, 3].Text.Trim(); // колонка C
                    if (!string.IsNullOrEmpty(unitName))
                        unitNames.Add(unitName);
                }
                foreach (var name in unitNames)
                {
                    var unit = new Unit { Name = name };
                    units[name] = unit;
                    _context.Units.Add(unit);
                }
            }

            // Поставщики из Tovar.xlsx
            using (var package = new ExcelPackage(new FileInfo(productsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                var supplierNames = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string supplierName = worksheet.Cells[row, 5].Text.Trim(); // колонка E
                    if (!string.IsNullOrEmpty(supplierName))
                        supplierNames.Add(supplierName);
                }
                foreach (var name in supplierNames)
                {
                    var supplier = new Supplier { Name = name };
                    suppliers[name] = supplier;
                    _context.Suppliers.Add(supplier);
                }
            }

            // Производители из Tovar.xlsx
            using (var package = new ExcelPackage(new FileInfo(productsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                var manufacturerNames = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string manufacturerName = worksheet.Cells[row, 6].Text.Trim(); // колонка F
                    if (!string.IsNullOrEmpty(manufacturerName))
                        manufacturerNames.Add(manufacturerName);
                }
                foreach (var name in manufacturerNames)
                {
                    var manufacturer = new Manufacturer { Name = name };
                    manufacturers[name] = manufacturer;
                    _context.Manufacturers.Add(manufacturer);
                }
            }

            // Категории из Tovar.xlsx
            using (var package = new ExcelPackage(new FileInfo(productsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                var categoryNames = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string categoryName = worksheet.Cells[row, 7].Text.Trim(); // колонка G
                    if (!string.IsNullOrEmpty(categoryName))
                        categoryNames.Add(categoryName);
                }
                foreach (var name in categoryNames)
                {
                    var category = new Category { Name = name };
                    categories[name] = category;
                    _context.Categories.Add(category);
                }
            }

            // Сохраняем все справочники, чтобы получить ID
            _context.SaveChanges();

            // 2. Пользователи
            using (var package = new ExcelPackage(new FileInfo(usersFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                for (int row = 2; row <= rowCount; row++)
                {
                    string roleName = worksheet.Cells[row, 1].Text.Trim();
                    string fullName = worksheet.Cells[row, 2].Text.Trim();
                    string login = worksheet.Cells[row, 3].Text.Trim();
                    string password = worksheet.Cells[row, 4].Text.Trim();

                    if (string.IsNullOrEmpty(roleName) || string.IsNullOrEmpty(fullName) ||
                        string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                        continue;

                    if (!roles.TryGetValue(roleName, out var role))
                        continue;

                    var user = new User
                    {
                        FullName = fullName,
                        Login = login,
                        Password = password,
                        RoleId = role.Id
                    };
                    _context.Users.Add(user);
                }
                _context.SaveChanges();
            }

            // Загружаем пользователей в память для поиска по ФИО
            var usersList = _context.Users.ToList();

            // 3. Пункты выдачи (в файле нет заголовка, начинаем с первой строки)
            using (var package = new ExcelPackage(new FileInfo(pickUpPointsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                for (int row = 1; row <= rowCount; row++)
                {
                    string address = worksheet.Cells[row, 1].Text.Trim();
                    if (string.IsNullOrEmpty(address)) continue;

                    var parts = address.Split(',').Select(p => p.Trim()).ToArray();
                    if (parts.Length < 3) continue;

                    if (!long.TryParse(parts[0], out long postCode))
                        continue;

                    string city = Regex.Replace(parts[1], @"^г\.?\s*", "", RegexOptions.IgnoreCase).Trim();
                    string street = Regex.Replace(parts[2], @"^ул\.?\s*", "", RegexOptions.IgnoreCase).Trim();

                    long house = 0;
                    if (parts.Length >= 4)
                    {
                        var match = Regex.Match(parts[3], @"\d+");
                        if (match.Success)
                            long.TryParse(match.Value, out house);
                    }

                    var point = new PickUpPoint
                    {
                        PostCode = postCode,
                        City = city,
                        Street = street,
                        House = house
                    };
                    _context.PickUpPoints.Add(point);
                    pickUpPointsByIndex[row] = point; // сохраняем соответствие индексу строки
                }
                _context.SaveChanges();
            }

            // 4. Товары: сначала артикулы, потом продукты
            using (var package = new ExcelPackage(new FileInfo(productsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                // Собираем уникальные артикулы
                var articleTitles = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string title = worksheet.Cells[row, 1].Text.Trim();
                    if (!string.IsNullOrEmpty(title))
                        articleTitles.Add(title);
                }
                foreach (var title in articleTitles)
                {
                    var article = new Article { Title = title };
                    articles[title] = article;
                    _context.Articles.Add(article);
                }
                _context.SaveChanges();

                // Создаём продукты
                for (int row = 2; row <= rowCount; row++)
                {
                    string articleTitle = worksheet.Cells[row, 1].Text.Trim();
                    string productName = worksheet.Cells[row, 2].Text.Trim();
                    string unitName = worksheet.Cells[row, 3].Text.Trim();
                    string priceStr = worksheet.Cells[row, 4].Text.Trim();
                    string supplierName = worksheet.Cells[row, 5].Text.Trim();
                    string manufacturerName = worksheet.Cells[row, 6].Text.Trim();
                    string categoryName = worksheet.Cells[row, 7].Text.Trim();
                    string discountStr = worksheet.Cells[row, 8].Text.Trim();
                    string quantityStr = worksheet.Cells[row, 9].Text.Trim();
                    string description = worksheet.Cells[row, 10].Text.Trim();
                    string photo = worksheet.Cells[row, 11].Text.Trim();

                    if (string.IsNullOrEmpty(articleTitle) || string.IsNullOrEmpty(productName))
                        continue;

                    if (!articles.TryGetValue(articleTitle, out var article) ||
                        !units.TryGetValue(unitName, out var unit) ||
                        !suppliers.TryGetValue(supplierName, out var supplier) ||
                        !manufacturers.TryGetValue(manufacturerName, out var manufacturer) ||
                        !categories.TryGetValue(categoryName, out var category))
                        continue;

                    if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                        continue;
                    if (!decimal.TryParse(discountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal discount))
                        discount = 0;
                    if (!long.TryParse(quantityStr, out long quantity))
                        quantity = 0;

                    var product = new Product
                    {
                        ArticleId = article.Id,
                        Name = productName,
                        UnitId = unit.Id,
                        Price = price,
                        SupplierId = supplier.Id,
                        ManufacturerId = manufacturer.Id,
                        CategoryId = category.Id,
                        Discount = discount,
                        QuantityInStock = quantity,
                        Description = description,
                        PhotoPath = string.IsNullOrEmpty(photo) ? null : photo
                    };
                    _context.Products.Add(product);
                }
                _context.SaveChanges();
            }

            // 5. Заказы
            using (var package = new ExcelPackage(new FileInfo(ordersFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                for (int row = 2; row <= rowCount; row++) // первая строка — заголовки
                {
                    string orderNumberStr = worksheet.Cells[row, 1].Text.Trim();
                    string articlesLine = worksheet.Cells[row, 2].Text.Trim();
                    string orderDateStr = worksheet.Cells[row, 3].Text.Trim();
                    string deliveryDateStr = worksheet.Cells[row, 4].Text.Trim();
                    string pickUpPointIndexStr = worksheet.Cells[row, 5].Text.Trim();
                    string customerFullName = worksheet.Cells[row, 6].Text.Trim();
                    string receiptCodeStr = worksheet.Cells[row, 7].Text.Trim();
                    string statusName = worksheet.Cells[row, 8].Text.Trim();

                    if (string.IsNullOrEmpty(orderNumberStr) || string.IsNullOrEmpty(articlesLine))
                        continue;

                    if (!long.TryParse(orderNumberStr, out long orderNumber) ||
                        !int.TryParse(pickUpPointIndexStr, out int pickUpPointIndex) ||
                        !long.TryParse(receiptCodeStr, out long receiptCode) ||
                        !statuses.TryGetValue(statusName, out var status))
                        continue;

                    var user = usersList.FirstOrDefault(u => u.FullName.Equals(customerFullName, StringComparison.OrdinalIgnoreCase));
                    if (user == null) continue;

                    if (!pickUpPointsByIndex.TryGetValue(pickUpPointIndex, out var pickUpPoint))
                        continue;

                    if (!TryParseDate(orderDateStr, out DateOnly orderDate) ||
                        !TryParseDate(deliveryDateStr, out DateOnly deliveryDate))
                        continue;

                    var order = new Order
                    {
                        Number = orderNumber,
                        OrderDate = orderDate,
                        DeliveryDate = deliveryDate,
                        PickUpPointId = pickUpPoint.Id,
                        UserId = user.Id,
                        ReceiptCode = receiptCode,
                        StatusId = status.Id,
                        OrderItems = new List<OrderItem>()
                    };

                    var itemParts = articlesLine.Split(',').Select(p => p.Trim()).ToArray();
                    if (itemParts.Length % 2 != 0) continue;

                    bool hasItems = false;
                    for (int i = 0; i < itemParts.Length; i += 2)
                    {
                        string artTitle = itemParts[i];
                        if (!long.TryParse(itemParts[i + 1], out long quantity))
                            continue;

                        if (!articles.TryGetValue(artTitle, out var article))
                            continue;

                        var product = _context.Products.FirstOrDefault(p => p.ArticleId == article.Id);
                        if (product == null) continue;

                        order.OrderItems.Add(new OrderItem
                        {
                            ArticleId = article.Id,
                            Quantity = quantity,
                            Price = product.Price
                        });
                        hasItems = true;
                    }

                    if (hasItems)
                        _context.Orders.Add(order);
                }
                _context.SaveChanges();
            }
        }

        private bool TryParseDate(string input, out DateOnly date)
        {
            date = default;
            if (string.IsNullOrEmpty(input)) return false;

            string[] formats = {
                "yyyy-MM-dd HH:mm:ss",
                "dd.MM.yyyy HH:mm:ss",
                "yyyy-MM-dd",
                "dd.MM.yyyy"
            };

            if (DateTime.TryParseExact(input, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dt))
            {
                date = DateOnly.FromDateTime(dt);
                return true;
            }

            // Коррекция для 30.02.2025
            if (input.StartsWith("30.02."))
            {
                string corrected = "28.02." + input.Substring(6);
                if (DateTime.TryParseExact(corrected, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    date = DateOnly.FromDateTime(dt);
                    return true;
                }
            }

            return false;
        }
    }
}