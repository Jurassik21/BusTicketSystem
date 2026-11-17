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

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
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

                // Надсилаємо запит на сервер
                string response = await TcpClientService.SendRequestAsync($"LOGIN;{login};{password}");

                // Перевірка на відсутність сервера (Тест №1)
                if (response.StartsWith("ERROR"))
                {
                    MessageBox.Show($"Відсутній зв'язок із сервером!\n\nДеталі: {response}",
                                    "Помилка підключення", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (response.StartsWith("SUCCESS"))
                {
                    string role = response.Split(':')[1];

                    if (role == "Admin")
                    {
                        AdminWindow adminWin = new AdminWindow();
                        adminWin.Show();
                    }
                    else
                    {
                        RouteWindow routeWin = new RouteWindow(login);
                        routeWin.Show();
                    }
                    this.Close();
                }
                else
                {
                    // Невірний логін або пароль (Тест №2)
                    MessageBox.Show("Невірний логін або пароль!", "Помилка авторизації", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Сталася непередбачена помилка: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnOpenRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow regWin = new RegisterWindow();
            regWin.ShowDialog();
        }
    }
}