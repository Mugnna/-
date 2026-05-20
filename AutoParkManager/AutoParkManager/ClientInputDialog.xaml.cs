using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AutoParkManager
{
    public partial class ClientInputDialog : Window
    {
        public string FullName { get; private set; } = string.Empty;
        public string Phone { get; private set; } = string.Empty;
        public string CarNumber { get; private set; } = string.Empty;
        public string CarBrand { get; private set; } = string.Empty;

        public ClientInputDialog()
        {
            InitializeComponent();
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
            FullName = FullNameBox.Text.Trim();
            Phone = PhoneBox.Text.Trim();
            CarNumber = CarNumberBox.Text.Trim().ToUpper();
            CarBrand = CarBrandCombo.Text.Trim();

            if (string.IsNullOrEmpty(FullName))
            {
                MessageBox.Show("Введите ФИО клиента!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(Phone))
            {
                MessageBox.Show("Введите телефон!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(CarNumber))
            {
                MessageBox.Show("Введите номер автомобиля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!IsValidCarNumber(CarNumber))
            {
                MessageBox.Show("Некорректный формат номера! Используйте формат: А123ВС77", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

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