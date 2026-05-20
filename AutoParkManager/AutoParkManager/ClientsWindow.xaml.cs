using System;
using System.Linq;
using System.Windows;
using AutoParkManager.Data;
using AutoParkManager.Models;

namespace AutoParkManager
{
    public partial class ClientsWindow : Window
    {
        private AppDbContext _context;

        public ClientsWindow(AppDbContext context)
        {
            InitializeComponent();
            _context = context;
            LoadClients();
        }

        private void LoadClients()
        {
            var clients = _context.Clients.ToList();
            var abonements = _context.Abonements.ToDictionary(a => a.AbonementId, a => a.Name);

            var result = clients.Select(c => new
            {
                c.ClientId,
                c.FullName,
                c.Phone,
                c.CarNumber,
                c.CarBrand,
                AbonementName = c.AbonementId.HasValue && abonements.ContainsKey(c.AbonementId.Value)
                    ? abonements[c.AbonementId.Value]
                    : "Нет",
                c.AbonementEndDate
            }).ToList();

            ClientsGrid.ItemsSource = result;
        }

        private void AddClient_Click(object sender, RoutedEventArgs e)
        {
            // Создаём диалоговое окно для ввода данных клиента
            var dialog = new ClientInputDialog();
            if (dialog.ShowDialog() == true)
            {
                Client client = new Client
                {
                    FullName = dialog.FullName,
                    Phone = dialog.Phone,
                    CarNumber = dialog.CarNumber.ToUpper(),
                    Email = "",
                    CarBrand = ""
                };

                _context.Clients.Add(client);
                _context.SaveChanges();
                LoadClients();
                MessageBox.Show("Клиент добавлен!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void AddAbonement_Click(object sender, RoutedEventArgs e)
        {
            if (ClientsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента из списка", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            dynamic selected = ClientsGrid.SelectedItem;
            int clientId = selected.ClientId;
            Client client = _context.Clients.Find(clientId);

            if (client == null) return;

            var abonement = _context.Abonements.FirstOrDefault();
            if (abonement == null)
            {
                MessageBox.Show("Абонементы не настроены", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            client.AbonementId = abonement.AbonementId;
            client.AbonementStartDate = DateTime.Now;
            client.AbonementEndDate = DateTime.Now.AddDays(abonement.ValidDays);

            _context.SaveChanges();
            LoadClients();
            MessageBox.Show($"Абонемент оформлен до {client.AbonementEndDate:dd.MM.yyyy}", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

    }
}