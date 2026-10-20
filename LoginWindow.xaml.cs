using BusClient;
using System;
using System.Windows;

namespace WpfApp1
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = txtLogin.Text.Trim();
                string password = txtPassword.Password.Trim();

                if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Будь ласка, заповніть усі поля!", "Попередження", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Перевірка на адміністратора
                if (login == "admin" && password == "admin")
                {
                    AdminWindow adminWin = new AdminWindow();
                    adminWin.Show();
                    this.Close();
                }
                else
                {
                    RouteWindow routeWin = new RouteWindow();
                    routeWin.Show();
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сталася помилка при авторизації: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOpenRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow regWin = new RegisterWindow();
            regWin.ShowDialog();
        }
    }
}