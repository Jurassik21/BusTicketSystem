using System;
using System.Collections.Generic;
using System.Windows;

namespace WpfApp1
{
    public partial class AdminWindow : Window
    {
        private List<RouteModel> adminRoutes = new List<RouteModel>();

        public AdminWindow()
        {
            InitializeComponent();
            LoadRoutes();
        }

        private void LoadRoutes()
        {
            adminRoutes = new List<RouteModel>
            {
                new RouteModel { DepartureCity = "Львів", ArrivalCity = "Київ", DepartureTime = "2026-10-15 08:00" },
                new RouteModel { DepartureCity = "Львів", ArrivalCity = "Тернопіль", DepartureTime = "2026-10-15 10:30" }
            };
            lvAdminRoutes.ItemsSource = adminRoutes;
        }

        private void BtnAddRoute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dep = txtDeparture.Text.Trim();
                string arr = txtArrival.Text.Trim();
                string time = txtTime.Text.Trim();

                if (string.IsNullOrEmpty(dep) || string.IsNullOrEmpty(arr))
                {
                    MessageBox.Show("Будь ласка, заповніть міста відправлення та прибуття!", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                adminRoutes.Add(new RouteModel { DepartureCity = dep, ArrivalCity = arr, DepartureTime = time });

                lvAdminRoutes.ItemsSource = null;
                lvAdminRoutes.ItemsSource = adminRoutes;

                txtDeparture.Clear();
                txtArrival.Clear();
                MessageBox.Show("Маршрут успішно додано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }

        private void BtnDeleteRoute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (lvAdminRoutes.SelectedItem is RouteModel selectedRoute)
                {
                    adminRoutes.Remove(selectedRoute);

                    // Оновлюємо відображення списку
                    lvAdminRoutes.ItemsSource = null;
                    lvAdminRoutes.ItemsSource = adminRoutes;

                    MessageBox.Show("Маршрут успішно видалено!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Будь ласка, виберіть маршрут зі списку для видалення!", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWin = new LoginWindow();
            loginWin.Show();
            this.Close();
        }
    }
}