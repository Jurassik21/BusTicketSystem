using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace WpfApp1
{
    public partial class SeatSelectionWindow : Window
    {
        private readonly string _currentUser;
        private readonly string _routeInfo;

        public SeatSelectionWindow(string username, string routeInfo)
        {
            InitializeComponent();
            _currentUser = username;
            _routeInfo = routeInfo;
            // Підключаємо подію завантаження вікна
            this.Loaded += SeatSelectionWindow_Loaded;
        }

        private async void SeatSelectionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string response = await TcpClientService.SendRequestAsync($"GET_BOOKED_SEATS;{_routeInfo}");
                if (response != "EMPTY" && !response.StartsWith("ERROR"))
                {
                    string[] bookedSeats = response.Split(',');
                    DisableBookedSeats(this, bookedSeats);
                }
            }
            catch { }
        }

        // Рекурсивний пошук кнопок місць на екрані та їх вимкнення
        private void DisableBookedSeats(DependencyObject parent, string[] bookedSeats)
        {
            foreach (object child in LogicalTreeHelper.GetChildren(parent))
            {
                if (child is Button btn && btn.Content != null)
                {
                    if (bookedSeats.Contains(btn.Content.ToString()))
                    {
                        btn.IsEnabled = false;
                        btn.Background = System.Windows.Media.Brushes.DarkGray; // Робимо кнопку сірою
                        btn.ToolTip = "Місце вже заброньовано";
                    }
                }
                else if (child is DependencyObject childObj)
                {
                    DisableBookedSeats(childObj, bookedSeats);
                }
            }
        }

        private async void Seat_Click(object sender, RoutedEventArgs e)
        {
            await ProcessBookingAsync(sender);
        }

        private async void BtnBook_Click(object sender, RoutedEventArgs e)
        {
            await ProcessBookingAsync(sender);
        }

        private async Task ProcessBookingAsync(object sender)
        {
            try
            {
                if (sender is Button btn && int.TryParse(btn.Content?.ToString(), out int seatNumber))
                {
                    string response = await TcpClientService.SendRequestAsync($"BOOK_SEAT;{_currentUser};{_routeInfo};{seatNumber}");
                    if (response == "SUCCESS")
                    {
                        MessageBox.Show($"Місце №{seatNumber} успішно заброньовано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                        RouteWindow rw = new RouteWindow(_currentUser);
                        rw.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(response, "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }
        private void BtnBookedSeat_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Неможливо вибрати це місце, оскільки воно вже зайняте!",
                            "Місце заброньовано", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

}