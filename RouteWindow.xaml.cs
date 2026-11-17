using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class RouteWindow : Window
    {
        private string _currentUser;

        // Конструктор за замовчуванням
        public RouteWindow() : this("Guest") { }

        // Конструктор із передачею імені користувача (викликається з LoginWindow)
        public RouteWindow(string username)
        {
            InitializeComponent();
            _currentUser = username;

            // Встановлюємо сьогоднішню дату в календар за замовчуванням
            if (dpTravelDate != null)
            {
                dpTravelDate.SelectedDate = DateTime.Today;
            }

            // Завантажуємо маршрути для сьогоднішньої дати при старті
            LoadRoutes();
        }

        // Обробник натискання кнопки "Показати доступні рейси"
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            LoadRoutes();
        }

        private async void LoadRoutes()
        {
            try
            {
                string serverResponse = await TcpClientService.SendRequestAsync("GET_ROUTES");
                if (string.IsNullOrEmpty(serverResponse) || serverResponse.StartsWith("ERROR") || serverResponse == "EMPTY")
                {
                    lvRoutes.ItemsSource = null;
                    return;
                }

                // Отримуємо дату з календаря. Якщо нічого не обрано - беремо сьогоднішню
                string selectedDate = dpTravelDate.SelectedDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");

                List<RouteModel> routes = new List<RouteModel>();
                string[] items = serverResponse.Split(';');

                foreach (var item in items)
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;

                    int firstDash = item.IndexOf('-');
                    int dateStartIdx = item.IndexOf("-202"); // Шукаємо рік (наприклад, -2026)

                    if (firstDash > 0 && dateStartIdx > firstDash)
                    {
                        string depCity = item.Substring(0, firstDash);
                        string arrCity = item.Substring(firstDash + 1, dateStartIdx - (firstDash + 1));
                        string dateTime = item.Substring(dateStartIdx + 1);

                        // ФІЛЬТРАЦІЯ: Додаємо маршрут, ТІЛЬКИ якщо його дата збігається з обраною в календарі
                        if (dateTime.StartsWith(selectedDate))
                        {
                            routes.Add(new RouteModel
                            {
                                DepartureCity = depCity,
                                ArrivalCity = arrCity,
                                DepartureTime = dateTime
                            });
                        }
                    }
                }

                // Оновлюємо список на екрані
                lvRoutes.ItemsSource = routes;

                // Якщо на обрану дату рейсів немає, можна показати повідомлення (за бажанням)
                if (routes.Count == 0)
                {
                    MessageBox.Show($"На обрану дату ({selectedDate}) доступних рейсів не знайдено.", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження маршрутів: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LvRoutes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvRoutes.SelectedItem is RouteModel selectedRoute)
            {
                string routeInfo = $"{selectedRoute.DepartureCity}-{selectedRoute.ArrivalCity} ({selectedRoute.DepartureTime})";
                SeatSelectionWindow seatWin = new SeatSelectionWindow(_currentUser, routeInfo);
                seatWin.Show();
                this.Close();
            }
        }
    }
}