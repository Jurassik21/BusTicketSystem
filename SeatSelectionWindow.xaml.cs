using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public partial class SeatSelectionWindow : Window
    {
        private int selectedSeat = 0;

        public SeatSelectionWindow()
        {
            InitializeComponent();
        }

        private void Seat_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                selectedSeat = int.Parse(btn.Content.ToString().Replace("Місце ", ""));
                MessageBox.Show($"Обрано місце №{selectedSeat}", "Інформація", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnBook_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedSeat == 0)
                {
                    MessageBox.Show("Будь ласка, оберіть місце!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                MessageBox.Show("Квиток заброньовано успішно! :)", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}");
            }
        }
    }
}