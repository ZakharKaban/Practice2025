using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace BarberShop
{
    public partial class UserProfilePage : Page
    {
        public UserProfilePage()
        {
            InitializeComponent();
            Loaded += UserProfilePage_Loaded;
        }

        private void UserProfilePage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!CurrentSession.IsAuthenticated)
            {
                MessageBox.Show("Необходимо войти в систему.", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var user = CurrentSession.CurrentUser;

            txtFirstName.Text = user.FirstName;
            txtLastName.Text = user.LastName;
            txtPhone.Text = user.PhoneNumber;
            txtEmail.Text = user.Email;
            txtRole.Text = user.UserRoles != null ? user.UserRoles.RoleName : string.Empty;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            txtError.Text = string.Empty;

            var firstName = txtFirstName.Text.Trim();
            var lastName = txtLastName.Text.Trim();
            var phone = txtPhone.Text.Trim();
            var email = txtEmail.Text.Trim();

            if (string.IsNullOrWhiteSpace(firstName))
            {
                txtError.Text = "Введите имя.";
                return;
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                txtError.Text = "Введите фамилию.";
                return;
            }

            if (!ValidatePhone(phone))
            {
                txtError.Text = "Введите корректный телефон.";
                return;
            }

            if (!ValidateEmail(email))
            {
                txtError.Text = "Введите корректный email.";
                return;
            }

            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    var userId = CurrentSession.CurrentUser.UserID;
                    var user = db.Users.FirstOrDefault(u => u.UserID == userId);
                    if (user == null)
                    {
                        txtError.Text = "Пользователь не найден.";
                        return;
                    }

                    var exists = db.Users.Any(u =>
                        (u.Email == email || u.PhoneNumber == phone) &&
                        u.UserID != userId);

                    if (exists)
                    {
                        txtError.Text = "Пользователь с таким email или телефоном уже существует.";
                        return;
                    }

                    user.FirstName = firstName;
                    user.LastName = lastName;
                    user.PhoneNumber = phone;
                    user.Email = email;

                    db.SaveChanges();

                    CurrentSession.CurrentUser = user;

                    MessageBox.Show("Данные успешно сохранены.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения профиля: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private bool ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return false;
            }

            var digits = new string(phone.Where(char.IsDigit).ToArray());
            return digits.Length >= 10;
        }
    }
}


