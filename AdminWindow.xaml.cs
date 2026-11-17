using System;
using System.Collections.Generic;
using System.Windows;

namespace WpfApp1
{
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            LoadAdminRoutes();
        }

        private async void LoadAdminRoutes()
        {
            try
            {
                string serverResponse = await TcpClientService.SendRequestAsync("GET_ROUTES");
                if (string.IsNullOrEmpty(serverResponse) || serverResponse.StartsWith("ERROR") || serverResponse == "EMPTY")
                {
                    lvAdminRoutes.ItemsSource = null;
                    return;
                }

                List<RouteModel> routes = new List<RouteModel>();
                string[] items = serverResponse.Split(';');

                foreach (var item in items)
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;
                    int firstDash = item.IndexOf('-');
                    int dateStartIdx = item.IndexOf("-202");

                    if (firstDash > 0 && dateStartIdx > firstDash)
                    {
                        routes.Add(new RouteModel
                        {
                            DepartureCity = item.Substring(0, firstDash),
                            ArrivalCity = item.Substring(firstDash + 1, dateStartIdx - (firstDash + 1)),
                            DepartureTime = item.Substring(dateStartIdx + 1)
                        });
                    }
                }
                lvAdminRoutes.ItemsSource = routes;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void BtnAddRoute_Click(object sender, RoutedEventArgs e)
        {
            string dep = txtDepCity.Text.Trim();
            string arr = txtArrCity.Text.Trim();
            string time = txtTime.Text.Trim();

            if (string.IsNullOrEmpty(dep) || string.IsNullOrEmpty(arr) || string.IsNullOrEmpty(time))
            {
                MessageBox.Show("Заповніть усі поля!");
                return;
            }

            string response = await TcpClientService.SendRequestAsync($"ADD_ROUTE;{dep};{arr};{time}");
            if (response == "SUCCESS")
            {
                MessageBox.Show("Маршрут додано!");
                LoadAdminRoutes(); // Оновлюємо список
            }
        }

        private async void BtnDeleteRoute_Click(object sender, RoutedEventArgs e)
        {
            if (lvAdminRoutes.SelectedItem is RouteModel selectedRoute)
            {
                string response = await TcpClientService.SendRequestAsync($"DELETE_ROUTE;{selectedRoute.DepartureCity};{selectedRoute.ArrivalCity};{selectedRoute.DepartureTime}");
                if (response == "SUCCESS")
                {
                    MessageBox.Show("Маршрут видалено!");
                    LoadAdminRoutes(); // Оновлюємо список
                }
            }
            else
            {
                MessageBox.Show("Оберіть маршрут для видалення!");
            }
        }

        private async void BtnDeleteUser_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsernameToDelete.Text.Trim();

            if (string.IsNullOrEmpty(username))
            {
                MessageBox.Show("Введіть логін користувача для видалення!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (username.ToLower() == "admin")
            {
                MessageBox.Show("Не можна видалити головного адміністратора!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string response = await TcpClientService.SendRequestAsync($"DELETE_USER;{username}");

            if (response == "SUCCESS")
            {
                MessageBox.Show($"Користувача '{username}' успішно видалено з бази даних!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                txtUsernameToDelete.Clear();
            }
            else
            {
                MessageBox.Show("Помилка видалення або такого користувача не існує.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}