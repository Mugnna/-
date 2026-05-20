using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoParkManager.Data;
using AutoParkManager.Models;

namespace AutoParkManager
{
    public partial class ExitWindow : Window
    {
        private AppDbContext _context;
        private ParkingSession _currentSession;
        private decimal _currentAmount;

        public ExitWindow(AppDbContext context)
        {
            try
            {
                InitializeComponent();
                _context = context;
                LoadActiveSessions();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке окна: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadActiveSessions()
        {
            try
            {
                var active = _context.ParkingSessions
                    .Where(s => s.ExitTime == null && !s.IsPaid)
                    .Select(s => new { s.CarNumber, s.CarBrand, s.EntryTime })
                    .ToList();

                ActiveCarsList.Items.Clear();

                if (active.Count == 0)
                {
                    ActiveCarsList.Items.Add("Нет автомобилей на стоянке");
                }
                else
                {
                    foreach (var item in active)
                    {
                        ActiveCarsList.Items.Add($"{item.CarNumber} | {item.CarBrand} | {item.EntryTime:HH:mm}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                ActiveCarsList.Items.Add("Ошибка загрузки данных");
            }
        }

        private void ActiveCarsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (ActiveCarsList.SelectedItem == null) return;

                string selected = ActiveCarsList.SelectedItem.ToString();
                if (selected == "Нет автомобилей на стоянке" || selected == "Ошибка загрузки данных")
                {
                    return;
                }

                string[] parts = selected.Split('|');
                if (parts.Length >= 1)
                {
                    string carNumber = parts[0].Trim();
                    CarNumberBox.Text = carNumber;
                    FindSession(carNumber);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе автомобиля: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string carNumber = CarNumberBox.Text.Trim().ToUpper();
                if (string.IsNullOrEmpty(carNumber))
                {
                    MessageBox.Show("Введите номер автомобиля или выберите из списка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                FindSession(carNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FindSession(string carNumber)
        {
            try
            {
                _currentSession = _context.ParkingSessions
                    .FirstOrDefault(s => s.CarNumber == carNumber && s.ExitTime == null && !s.IsPaid);

                if (_currentSession == null)
                {
                    MessageBox.Show($"Автомобиль {carNumber} не найден на стоянке", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime exitTime = DateTime.Now;
                TimeSpan duration = exitTime - _currentSession.EntryTime;
                double hours = duration.TotalHours;
                if (hours < 0.25) hours = 0;

                var tariff = _context.Tariffs.FirstOrDefault(t => !t.IsNightTariff);
                _currentAmount = tariff != null ? (decimal)hours * tariff.PricePerHour : 0;

                if (_currentSession.ClientId != null)
                {
                    var client = _context.Clients.Find(_currentSession.ClientId);
                    if (client?.AbonementEndDate >= DateTime.Now)
                    {
                        MessageBox.Show($"Клиент {client.FullName} имеет активный абонемент. Стоимость = 0 руб.", "Абонемент", MessageBoxButton.OK, MessageBoxImage.Information);
                        _currentAmount = 0;
                    }
                }

                EntryTimeText.Text = _currentSession.EntryTime.ToString("dd.MM.yyyy HH:mm");
                ExitTimeText.Text = exitTime.ToString("dd.MM.yyyy HH:mm");
                DurationText.Text = $"{duration.Hours}ч {duration.Minutes}мин ({hours:F2} ч)";
                AmountText.Text = $"{_currentAmount:C}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске сессии: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SavePaymentAndReceipt(string paymentMethod)
        {
            try
            {
                if (_currentSession == null)
                {
                    MessageBox.Show("Сначала найдите автомобиль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                DateTime exitTime = DateTime.Now;
                TimeSpan duration = exitTime - _currentSession.EntryTime;
                decimal hours = (decimal)duration.TotalHours;

                var payment = new Payment
                {
                    SessionId = _currentSession.SessionId,
                    CarNumber = _currentSession.CarNumber,
                    EntryTime = _currentSession.EntryTime,
                    ExitTime = exitTime,
                    DurationHours = hours,
                    Amount = _currentAmount,
                    PaymentDate = DateTime.Now
                };
                _context.Payments.Add(payment);

                _currentSession.ExitTime = exitTime;
                _currentSession.TotalCost = _currentAmount;
                _currentSession.IsPaid = true;

                _context.SaveChanges();

                string checkContent = $@"
====================================
         АВТОСТОЯНКА
====================================
Номер авто:   {_currentSession.CarNumber}
Время въезда: {_currentSession.EntryTime:dd.MM.yyyy HH:mm}
Время выезда: {exitTime:dd.MM.yyyy HH:mm}
Длительность: {duration.Hours}ч {duration.Minutes}мин
Стоимость:    {_currentAmount:C}
Способ оплаты: {paymentMethod}
====================================";

                string fileName = $"Check_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                File.WriteAllText(fileName, checkContent);

                MessageBox.Show($"Оплата принята!\nЧек сохранён: {fileName}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении платежа: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PayCash_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSession == null)
            {
                MessageBox.Show("Сначала найдите автомобиль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SavePaymentAndReceipt("Наличные");
        }

        private void PayCard_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSession == null)
            {
                MessageBox.Show("Сначала найдите автомобиль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            SavePaymentAndReceipt("Банковская карта");
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
