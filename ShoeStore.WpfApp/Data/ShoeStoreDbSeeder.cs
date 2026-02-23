using OfficeOpenXml;
using ShoeStore.WpfApp.Data;
using ShoeStore.WpfApp.Models;
using System.ComponentModel;
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
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();

            // Словари для быстрого поиска справочников
            var roles = new Dictionary<string, Role>();
            var statuses = new Dictionary<string, Status>();
            var units = new Dictionary<string, Unit>();
            var suppliers = new Dictionary<string, Supplier>();
            var manufacturers = new Dictionary<string, Manufacturer>();
            var categories = new Dictionary<string, Category>();
            var articles = new Dictionary<string, Article>();
            var pickUpPoints = new List<PickUpPoint>();
            var users = new List<User>();

            // 1. Заполнение ролей (из user_import.xlsx)
            using (var package = new ExcelPackage(new FileInfo(usersFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                var uniqueRoles = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string roleName = worksheet.Cells[row, 1].Text.Trim();
                    if (!string.IsNullOrEmpty(roleName))
                        uniqueRoles.Add(roleName);
                }

                foreach (var roleName in uniqueRoles)
                {
                    var role = new Role { Name = roleName };
                    _context.Roles.Add(role);
                    roles[roleName] = role;
                }
                _context.SaveChanges();
            }

            // 2. Статусы заказов
            var orderStatuses = new[] { "Новый", "Завершен" };
            foreach (var statusName in orderStatuses)
            {
                var status = new Status { Name = statusName };
                _context.Statuses.Add(status);
                statuses[statusName] = status;
            }
            _context.SaveChanges();

            // 3. Единицы измерения (из Tovar.xlsx)
            using (var package = new ExcelPackage(new FileInfo(productsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                var uniqueUnits = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string unitName = worksheet.Cells[row, 3].Text.Trim(); // колонка C
                    if (!string.IsNullOrEmpty(unitName))
                        uniqueUnits.Add(unitName);
                }

                foreach (var unitName in uniqueUnits)
                {
                    var unit = new Unit { Name = unitName };
                    _context.Units.Add(unit);
                    units[unitName] = unit;
                }
                _context.SaveChanges();
            }

            // 4. Поставщики
            using (var package = new ExcelPackage(new FileInfo(productsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                var uniqueSuppliers = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string supplierName = worksheet.Cells[row, 5].Text.Trim(); // колонка E
                    if (!string.IsNullOrEmpty(supplierName))
                        uniqueSuppliers.Add(supplierName);
                }

                foreach (var supplierName in uniqueSuppliers)
                {
                    var supplier = new Supplier { Name = supplierName };
                    _context.Suppliers.Add(supplier);
                    suppliers[supplierName] = supplier;
                }
                _context.SaveChanges();
            }

            // 5. Производители
            using (var package = new ExcelPackage(new FileInfo(productsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                var uniqueManufacturers = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string manufacturerName = worksheet.Cells[row, 6].Text.Trim(); // колонка F
                    if (!string.IsNullOrEmpty(manufacturerName))
                        uniqueManufacturers.Add(manufacturerName);
                }

                foreach (var manufacturerName in uniqueManufacturers)
                {
                    var manufacturer = new Manufacturer { Name = manufacturerName };
                    _context.Manufacturers.Add(manufacturer);
                    manufacturers[manufacturerName] = manufacturer;
                }
                _context.SaveChanges();
            }

            // 6. Категории
            using (var package = new ExcelPackage(new FileInfo(productsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                var uniqueCategories = new HashSet<string>();
                for (int row = 2; row <= rowCount; row++)
                {
                    string categoryName = worksheet.Cells[row, 7].Text.Trim(); // колонка G
                    if (!string.IsNullOrEmpty(categoryName))
                        uniqueCategories.Add(categoryName);
                }

                foreach (var categoryName in uniqueCategories)
                {
                    var category = new Category { Name = categoryName };
                    _context.Categories.Add(category);
                    categories[categoryName] = category;
                }
                _context.SaveChanges();
            }

            // 7. Пункты выдачи (парсим адреса)
            using (var package = new ExcelPackage(new FileInfo(pickUpPointsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                for (int row = 2; row <= rowCount; row++) // с первой строки данных (после заголовка)
                {
                    string address = worksheet.Cells[row, 1].Text.Trim();
                    if (string.IsNullOrEmpty(address)) continue;

                    // Разделяем по запятым
                    var parts = address.Split(',').Select(p => p.Trim()).ToArray();
                    if (parts.Length < 3) continue; // минимум индекс, город, улица

                    // Индекс
                    if (!long.TryParse(parts[0], out long postCode))
                        continue;

                    // Город (убираем "г. " или "г.")
                    string city = Regex.Replace(parts[1], @"^г\.?\s*", "", RegexOptions.IgnoreCase).Trim();

                    // Улица (убираем "ул. ")
                    string street = Regex.Replace(parts[2], @"^ул\.?\s*", "", RegexOptions.IgnoreCase).Trim();

                    // Дом (последняя часть, если есть)
                    long house = 0;
                    if (parts.Length >= 4)
                    {
                        string housePart = parts[3];
                        // Извлекаем первое число
                        var match = Regex.Match(housePart, @"\d+");
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
                    pickUpPoints.Add(point);
                }
                _context.SaveChanges();
            }

            // 8. Пользователи
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
                        Role = role
                    };
                    _context.Users.Add(user);
                    users.Add(user);
                }
                _context.SaveChanges();
            }

            // 9. Товары (Article и Product)
            using (var package = new ExcelPackage(new FileInfo(productsFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                for (int row = 2; row <= rowCount; row++)
                {
                    string articleTitle = worksheet.Cells[row, 1].Text.Trim(); // Артикул
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

                    // Артикул (уникальный)
                    if (!articles.ContainsKey(articleTitle))
                    {
                        var article = new Article { Title = articleTitle };
                        _context.Articles.Add(article);
                        articles[articleTitle] = article;
                        _context.SaveChanges(); // сохраняем, чтобы получить Id
                    }

                    // Единица измерения
                    if (!units.TryGetValue(unitName, out var unit))
                        continue;

                    // Поставщик
                    if (!suppliers.TryGetValue(supplierName, out var supplier))
                        continue;

                    // Производитель
                    if (!manufacturers.TryGetValue(manufacturerName, out var manufacturer))
                        continue;

                    // Категория
                    if (!categories.TryGetValue(categoryName, out var category))
                        continue;

                    // Парсинг чисел
                    if (!decimal.TryParse(priceStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal price))
                        continue;
                    if (!decimal.TryParse(discountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal discount))
                        discount = 0;
                    if (!long.TryParse(quantityStr, out long quantity))
                        quantity = 0;

                    var product = new Product
                    {
                        Article = articles[articleTitle],
                        Name = productName,
                        Unit = unit,
                        Price = price,
                        Supplier = supplier,
                        Manufacturer = manufacturer,
                        Category = category,
                        Discount = discount,
                        QuantityInStock = quantity,
                        Description = description,
                        PhotoPath = string.IsNullOrEmpty(photo) ? null : photo
                    };
                    _context.Products.Add(product);
                }
                _context.SaveChanges();
            }

            // 10. Заказы
            using (var package = new ExcelPackage(new FileInfo(ordersFilePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;
                for (int row = 2; row <= rowCount; row++)
                {
                    string orderNumberStr = worksheet.Cells[row, 1].Text.Trim();
                    string articlesLine = worksheet.Cells[row, 2].Text.Trim();
                    string orderDateStr = worksheet.Cells[row, 3].Text.Trim();
                    string deliveryDateStr = worksheet.Cells[row, 4].Text.Trim();
                    string pickUpPointIdStr = worksheet.Cells[row, 5].Text.Trim();
                    string customerFullName = worksheet.Cells[row, 6].Text.Trim();
                    string receiptCodeStr = worksheet.Cells[row, 7].Text.Trim();
                    string statusName = worksheet.Cells[row, 8].Text.Trim();

                    if (string.IsNullOrEmpty(orderNumberStr) || string.IsNullOrEmpty(articlesLine))
                        continue;

                    if (!long.TryParse(orderNumberStr, out long orderNumber))
                        continue;
                    if (!long.TryParse(pickUpPointIdStr, out long pickUpPointId))
                        continue;
                    if (!long.TryParse(receiptCodeStr, out long receiptCode))
                        continue;
                    if (!statuses.TryGetValue(statusName, out var status))
                        continue;

                    // Поиск пользователя по ФИО
                    var user = users.FirstOrDefault(u => u.FullName.Equals(customerFullName, StringComparison.OrdinalIgnoreCase));
                    if (user == null)
                        continue; // пропускаем заказ, если пользователь не найден

                    // Поиск пункта выдачи по Id (предполагаем, что Id соответствуют порядку)
                    var pickUpPoint = pickUpPoints.FirstOrDefault(p => p.Id == pickUpPointId);
                    if (pickUpPoint == null)
                        continue;

                    // Парсинг дат с поддержкой разных форматов
                    if (!TryParseDate(orderDateStr, out DateOnly orderDate))
                        continue;
                    if (!TryParseDate(deliveryDateStr, out DateOnly deliveryDate))
                        continue;

                    var order = new Order
                    {
                        Number = orderNumber,
                        OrderDate = orderDate,
                        DeliveryDate = deliveryDate,
                        PickUpPoint = pickUpPoint,
                        User = user,
                        ReceiptCode = receiptCode,
                        Status = status,
                        OrderItems = new List<OrderItem>()
                    };

                    // Разбор артикулов и количества
                    var parts = articlesLine.Split(',').Select(p => p.Trim()).ToArray();
                    if (parts.Length % 2 != 0)
                        continue; // нечётное количество элементов — ошибка

                    for (int i = 0; i < parts.Length; i += 2)
                    {
                        string articleTitle = parts[i];
                        if (!long.TryParse(parts[i + 1], out long quantity))
                            continue;

                        if (!articles.TryGetValue(articleTitle, out var article))
                            continue;

                        // Получаем товар по артикулу (предполагаем один Product на Article)
                        var product = _context.Products.FirstOrDefault(p => p.ArticleId == article.Id);
                        if (product == null)
                            continue;

                        order.OrderItems.Add(new OrderItem
                        {
                            Article = article,
                            Quantity = quantity,
                            Price = product.Price // цена на момент заполнения
                        });
                    }

                    if (order.OrderItems.Any())
                    {
                        _context.Orders.Add(order);
                    }
                }
                _context.SaveChanges();
            }
        }

        private bool TryParseDate(string input, out DateOnly date)
        {
            date = default;
            if (string.IsNullOrEmpty(input)) return false;

            // Пробуем разные форматы
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

            // Спецобработка для "30.02.2025" — заменяем на 28.02.2025
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