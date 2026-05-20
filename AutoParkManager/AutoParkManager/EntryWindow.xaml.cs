using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AutoParkManager.Data;
using AutoParkManager.Models;

namespace AutoParkManager
{
    public partial class EntryWindow : Window
    {
        private AppDbContext _context;

        public EntryWindow(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
            LoadClients();
        }

        private void LoadClients()
        {
            ClientCombo.Items.Clear();
            ClientCombo.Items.Add(new { Id = 0, DisplayText = "Нет (разовый)" });

            var clients = _context.Clients.ToList();
            foreach (var c in clients)
            {
                ClientCombo.Items.Add(new { Id = c.ClientId, DisplayText = $"{c.FullName} ({c.CarNumber})" });
            }
            ClientCombo.SelectedIndex = 0;
        }

        private void LoadCarNumbers_Click(object sender, RoutedEventArgs e)
        {
            var numbers = _context.Clients.Select(c => c.CarNumber).Distinct().ToList();

            var dialog = new Window
            {
                Title = "Выберите автомобиль",
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = (Brush)new BrushConverter().ConvertFrom("#1E1E2E"),
                Foreground = Brushes.White
            };

            var listBox = new ListBox
            {
                Background = (Brush)new BrushConverter().ConvertFrom("#2D2D3D"),
                Foreground = Brushes.White,
                FontSize = 14
            };

            foreach (var num in numbers)
            {
                listBox.Items.Add(num);
            }

            listBox.MouseDoubleClick += (s, args) =>
            {
                if (listBox.SelectedItem != null)
                {
                    CarNumberBox.Text = listBox.SelectedItem.ToString();
                    dialog.Close();
                }
            };

            dialog.Content = listBox;
            dialog.ShowDialog();
        }

        private void CarNumberBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"[A-Za-zА-Яа-я0-9]");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void CarNumberBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox != null && textBox.Text != null)
            {
                string oldText = textBox.Text;
                string newText = oldText.ToUpper();
                if (oldText != newText)
                {
                    int cursorPos = textBox.SelectionStart;
                    textBox.Text = newText;
                    textBox.SelectionStart = cursorPos;
                }
            }
        }

        private bool IsValidCarNumber(string number)
        {
            if (string.IsNullOrEmpty(number)) return false;
            Regex regex = new Regex(@"^[А-Я]{1}\d{3}[А-Я]{2}\d{2,3}$", RegexOptions.IgnoreCase);
            return regex.IsMatch(number);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string carNumber = CarNumberBox.Text.Trim().ToUpper();

            if (string.IsNullOrEmpty(carNumber))
            {
                MessageBox.Show("Введите госномер автомобиля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidCarNumber(carNumber))
            {
                MessageBox.Show("Некорректный формат номера! Используйте формат: А123ВС77", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var active = _context.ParkingSessions.FirstOrDefault(s => s.CarNumber == carNumber && s.ExitTime == null && !s.IsPaid);
            if (active != null)
            {
                MessageBox.Show($"Автомобиль {carNumber} уже находится на стоянке", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int? clientId = null;
            if (ClientCombo.SelectedIndex > 0)
            {
                dynamic selected = ClientCombo.SelectedItem;
                clientId = selected.Id;
            }

            var session = new ParkingSession
            {
                CarNumber = carNumber,
                CarBrand = CarBrandCombo.Text.Trim(),
                ClientId = clientId,
                EntryTime = DateTime.Now,
                IsPaid = false
            };

            _context.ParkingSessions.Add(session);
            _context.SaveChanges();

            MessageBox.Show($"Автомобиль {carNumber} зарегистрирован!\nВремя въезда: {session.EntryTime:HH:mm:ss}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
