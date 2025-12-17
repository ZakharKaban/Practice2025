using System;
using System.Linq;
using System.Windows;

namespace BarberShop
{
    /// <summary>
    /// Окно авторизации пользователя.
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            txtError.Text = string.Empty;

            var login = txtLogin.Text.Trim();
            var password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(login))
            {
                txtError.Text = "Введите email или телефон.";
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                txtError.Text = "Введите пароль.";
                return;
            }

            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    var user = db.Users
                        .FirstOrDefault(u =>
                            (u.Email == login || u.PhoneNumber == login) &&
                            u.PasswordHash == password &&
                            (u.IsActive == null || u.IsActive == true));

                    if (user == null)
                    {
                        txtError.Text = "Неверный логин или пароль.";
                        return;
                    }

                    CurrentSession.CurrentUser = user;

                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при входе: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Owner = this;
            registerWindow.ShowDialog();
        }
    }
}


