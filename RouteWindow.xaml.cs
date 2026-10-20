using BusClient;
using System;
using System.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class RouteWindow : Window
    {
        public RouteWindow()
        {
            InitializeComponent();
            // Встановлюємо поточну дату за замовчуванням
            dpTravelDate.SelectedDate = DateTime.Today;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string selectedDate = dpTravelDate.SelectedDate?.ToString("yyyy-MM-dd") ?? "не обрано";

                List<RouteModel> routes = new List<RouteModel>
                {
                    new RouteModel { DepartureCity = "Львів", ArrivalCity = "Київ", DepartureTime = $"{selectedDate}" },
                    new RouteModel { DepartureCity = "Львів", ArrivalCity = "Тернопіль", DepartureTime = $"{selectedDate}" }
                };
                lvRoutes.ItemsSource = routes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }

        private void LvRoutes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lvRoutes.SelectedItem is RouteModel selectedRoute)
            {
                SeatSelectionWindow seatWin = new SeatSelectionWindow();
                seatWin.Show();
                this.Close();
            }
        }
    }
}