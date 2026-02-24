using System;
using System.IO;
using System.Windows;
using ShoeStore.WpfApp.Data;
using ShoeStore.WpfApp.Views;

namespace ShoeStore.WpfApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // При первом запуске заполняем базу данных из Excel-файлов
            using (var context = new ShoeStoreDbContext())
            {
                if (!context.Users.Any())
                {
                    var seeder = new ShoeStoreDbSeeder(context);
                    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                    seeder.Seed(
                        Path.Combine(baseDir, "Assets", "user_import.xlsx"),
                        Path.Combine(baseDir, "Assets", "Tovar.xlsx"),
                        Path.Combine(baseDir, "Assets", "PickUpPoints.xlsx"),
                        Path.Combine(baseDir, "Assets", "Orders.xlsx"));
                }
            }

            // Запускаем окно входа
            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}