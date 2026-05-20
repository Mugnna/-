using System;
using System.Linq;
using System.Windows;
using System.IO;
using AutoParkManager.Data;

namespace AutoParkManager
{
    public partial class ReportsWindow : Window
    {
        private AppDbContext _context;

        public ReportsWindow(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
        }

        private void Log(string message)
        {
            LogTextBox.Text = $"{DateTime.Now:HH:mm:ss} - {message}\n{LogTextBox.Text}";
        }

        private void RevenueReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log("Начинаем формирование отчёта по выручке...");

                // Сначала загружаем данные, потом группируем в памяти
                var allPayments = _context.Payments.ToList();

                var payments = allPayments
                    .GroupBy(p => p.PaymentDate.Date)
                    .Select(g => new { Date = g.Key, Total = g.Sum(p => p.Amount), Count = g.Count() })
                    .OrderByDescending(x => x.Date)
                    .ToList();

                Log($"Найдено {payments.Count} записей");

                string fileName = $"Revenue_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                using (StreamWriter sw = new StreamWriter(fullPath))
                {
                    sw.WriteLine("ОТЧЁТ ПО ВЫРУЧКЕ");
                    sw.WriteLine("=================");
                    sw.WriteLine();
                    sw.WriteLine($"{"Дата",12} {"Выручка",15} {"Кол-во оплат",15}");
                    sw.WriteLine(new string('-', 45));

                    foreach (var p in payments)
                    {
                        sw.WriteLine($"{p.Date:dd.MM.yyyy,12} {p.Total,15:C} {p.Count,15}");
                    }

                    sw.WriteLine();
                    sw.WriteLine($"Итого: {payments.Sum(p => p.Total):C}");
                }

                Log($"Отчёт сохранён: {fullPath}");
                System.Diagnostics.Process.Start(fullPath);
            }
            catch (Exception ex)
            {
                Log($"ОШИБКА: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AbonementsReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log("Начинаем формирование отчёта по абонементам...");

                // Этот отчёт уже работал, оставляем как есть
                var activeAbonements = _context.Clients
                    .Where(c => c.AbonementId != null && c.AbonementEndDate >= DateTime.Now)
                    .Select(c => new { c.FullName, c.Phone, c.CarNumber, c.AbonementEndDate })
                    .ToList();

                Log($"Найдено {activeAbonements.Count} активных абонементов");

                string fileName = $"Abonements_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                using (StreamWriter sw = new StreamWriter(fullPath))
                {
                    sw.WriteLine("ОТЧЁТ ПО АКТИВНЫМ АБОНЕМЕНТАМ");
                    sw.WriteLine("==============================");
                    sw.WriteLine();

                    foreach (var a in activeAbonements)
                    {
                        sw.WriteLine($"Клиент: {a.FullName}");
                        sw.WriteLine($"Телефон: {a.Phone}");
                        sw.WriteLine($"Автомобиль: {a.CarNumber}");
                        sw.WriteLine($"Действителен до: {a.AbonementEndDate:dd.MM.yyyy}");
                        sw.WriteLine(new string('-', 40));
                    }

                    sw.WriteLine($"Всего активных абонементов: {activeAbonements.Count}");
                }

                Log($"Отчёт сохранён: {fullPath}");
                System.Diagnostics.Process.Start(fullPath);
            }
            catch (Exception ex)
            {
                Log($"ОШИБКА: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ParkingLoadReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Log("Начинаем формирование отчёта по загрузке...");

                // Сначала загружаем данные, потом группируем в памяти
                DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);
                var allSessions = _context.ParkingSessions
                    .Where(s => s.EntryTime >= thirtyDaysAgo)
                    .ToList();

                var sessions = allSessions
                    .GroupBy(s => s.EntryTime.Hour)
                    .Select(g => new { Hour = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Hour)
                    .ToList();

                Log($"Найдено {sessions.Count} часовых интервалов");

                string fileName = $"ParkingLoad_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                string fullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);

                using (StreamWriter sw = new StreamWriter(fullPath))
                {
                    sw.WriteLine("ОТЧЁТ ПО ЗАГРУЗКЕ ПАРКОВКИ (последние 30 дней)");
                    sw.WriteLine("=============================================");
                    sw.WriteLine();
                    sw.WriteLine($"{"Час",-10} {"Количество въездов",20}");
                    sw.WriteLine(new string('-', 35));

                    foreach (var s in sessions)
                    {
                        // ИСПРАВЛЕННАЯ СТРОКА
                        sw.WriteLine($"{s.Hour}:00,{s.Count,-10} {s.Count,20}");
                    }

                    sw.WriteLine();
                    sw.WriteLine($"Всего въездов: {sessions.Sum(s => s.Count)}");
                }

                Log($"Отчёт сохранён: {fullPath}");
                System.Diagnostics.Process.Start(fullPath);
            }
            catch (Exception ex)
            {
                Log($"ОШИБКА: {ex.Message}");
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}