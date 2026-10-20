using System;
using System.Windows;

namespace WpfApp1
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtRegLogin.Text) || string.IsNullOrEmpty(txtRegPassword.Password))
                {
                    MessageBox.Show("Заповніть обов'язкові поля!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (txtRegPassword.Password != txtRegConfirmPassword.Password)
                {
                    MessageBox.Show("Паролі не збігаються!", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                MessageBox.Show("Реєстрація успішна!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
