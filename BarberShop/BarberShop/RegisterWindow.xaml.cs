using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace BarberShop
{
    /// <summary>
    /// Окно регистрации нового пользователя.
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            txtError.Text = string.Empty;

            var firstName = txtFirstName.Text.Trim();
            var lastName = txtLastName.Text.Trim();
            var phone = txtPhone.Text.Trim();
            var email = txtEmail.Text.Trim();
            var password = txtPassword.Password;
            var passwordRepeat = txtPasswordRepeat.Password;

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
                txtError.Text = "Введите корректный номер телефона (минимум 10 цифр).";
                return;
            }

            if (!ValidateEmail(email))
            {
                txtError.Text = "Введите корректный email.";
                return;
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                txtError.Text = "Пароль должен содержать не менее 6 символов.";
                return;
            }

            if (password != passwordRepeat)
            {
                txtError.Text = "Пароли не совпадают.";
                return;
            }

            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    var exists = db.Users.Any(u => u.Email == email || u.PhoneNumber == phone);
                    if (exists)
                    {
                        txtError.Text = "Пользователь с таким email или телефоном уже существует.";
                        return;
                    }

                    var defaultRole = db.UserRoles.FirstOrDefault(r =>
                        r.RoleName == "Пользователь" || r.RoleName == "User");

                    if (defaultRole == null)
                    {
                        defaultRole = db.UserRoles.OrderBy(r => r.RoleID).FirstOrDefault();
                    }

                    if (defaultRole == null)
                    {
                        txtError.Text = "Не найдена роль по умолчанию для пользователя.";
                        return;
                    }

                    var user = new Users
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        PhoneNumber = phone,
                        Email = email,
                        PasswordHash = password,
                        RoleID = defaultRole.RoleID,
                        RegistrationDate = DateTime.Now,
                        IsActive = true
                    };

                    db.Users.Add(user);
                    db.SaveChanges();

                    MessageBox.Show("Регистрация прошла успешно. Теперь вы можете войти в систему.",
                        "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                    DialogResult = true;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при регистрации: " + ex.Message, "Ошибка",
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


