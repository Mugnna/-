using System;
using System.Linq;
using System.Windows;
using System.Data.Entity;
using AutoParkManager.Data;
using AutoParkManager.Models;

namespace AutoParkManager
{
    public partial class MainWindow : Window
    {
        private AppDbContext _context;
        private User _currentUser;
        private System.Timers.Timer _statsTimer;

        public MainWindow(User user)
        {
            InitializeComponent();
            _currentUser = user;
            _context = new AppDbContext();

            if (_currentUser.Role == "Operator")
            {
                ClientsBtn.Visibility = Visibility.Collapsed;
                ReportsBtn.Visibility = Visibility.Collapsed;
            }

            LoadActiveSessions();
            LoadStatistics();

            // Обновление статистики каждые 30 секунд
            _statsTimer = new System.Timers.Timer(30000);
            _statsTimer.Elapsed += (s, e) => Dispatcher.Invoke(() => LoadStatistics());
            _statsTimer.Start();
        }

        private void LoadStatistics()
        {
            try
            {
                // Активные автомобили на стоянке
                int activeCars = _context.ParkingSessions.Count(s => s.ExitTime == null && !s.IsPaid);
                ActiveCarsCount.Text = activeCars.ToString();

                // Выручка за сегодня
                DateTime today = DateTime.Today;
                decimal todayRevenue = _context.Payments
                    .Where(p => p.PaymentDate >= today)
                    .Sum(p => (decimal?)p.Amount) ?? 0;
                TodayRevenue.Text = $"{todayRevenue:N0} ₽";

                // Всего машин за сегодня (въезды)
                int todayVisits = _context.ParkingSessions.Count(s => s.EntryTime >= today);
                TodayVisits.Text = todayVisits.ToString();

                // Активные абонементы
                int activeAbonements = _context.Clients.Count(c => c.AbonementId != null && c.AbonementEndDate >= DateTime.Now);
                ActiveAbonementsCount.Text = activeAbonements.ToString();
            }
            catch (Exception ex)
            {
                // Если ошибка — просто не показываем статистику
                System.Diagnostics.Debug.WriteLine($"Ошибка статистики: {ex.Message}");
            }
        }

        private void LoadActiveSessions()
        {
            try
            {
                var sessions = _context.ParkingSessions
                    .Include(s => s.Client)
                    .Where(s => s.ExitTime == null && !s.IsPaid)
                    .ToList();

                var result = sessions.Select(s => new
                {
                    s.SessionId,
                    s.CarNumber,
                    s.CarBrand,
                    ClientName = s.Client != null ? s.Client.FullName : "Разовый",
                    s.EntryTime,
                    CurrentCost = CalculateCost(s.EntryTime, DateTime.Now)
                }).ToList();

                ActiveSessionsGrid.ItemsSource = result;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки сессий: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private decimal CalculateCost(DateTime entry, DateTime exit)
        {
            double hours = (exit - entry).TotalHours;
            if (hours <= 0.25) return 0;

            var tariff = _context.Tariffs.FirstOrDefault(t => !t.IsNightTariff);
            if (tariff == null) return 0;

            return Math.Round((decimal)hours * tariff.PricePerHour, 2);
        }

        private void Entry_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EntryWindow(_context);
            if (dialog.ShowDialog() == true)
            {
                LoadActiveSessions();
                LoadStatistics();
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ExitWindow(_context);
            if (dialog.ShowDialog() == true)
            {
                LoadActiveSessions();
                LoadStatistics();
            }
        }

        private void Clients_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ClientsWindow(_context);
            if (dialog.ShowDialog() == true)
            {
                LoadStatistics();
            }
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ReportsWindow(_context);
            dialog.ShowDialog();
        }

        protected override void OnClosed(EventArgs e)
        {
            _statsTimer?.Stop();
            _statsTimer?.Dispose();
            base.OnClosed(e);
        }
    }
}