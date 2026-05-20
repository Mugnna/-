using System;
using System.Linq;
using System.Windows;
using AutoParkManager.Data;
using AutoParkManager.Models;

namespace AutoParkManager
{
    public partial class LoginWindow : Window
    {
        private AppDbContext _context;

        public LoginWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Простая проверка (без хэширования)
            User user = null;

            if (login == "admin" && password == "admin123")
            {
                user = _context.Users.FirstOrDefault(u => u.Login == "admin");
            }
            else if (login == "operator" && password == "oper123")
            {
                user = _context.Users.FirstOrDefault(u => u.Login == "operator");
            }

            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MainWindow mainWindow = new MainWindow(user);
            mainWindow.Show();
            this.Close();
        }
    }
}