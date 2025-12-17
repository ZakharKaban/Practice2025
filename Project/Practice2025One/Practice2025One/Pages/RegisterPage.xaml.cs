using System;
using System.Windows;
using System.Windows.Controls;
using Practice2025One.AppData;
using Practice2025One.Class;

namespace Practice2025One.Pages
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
        }
        
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fullName = FullNameTextBox.Text.Trim();
                string email = EmailTextBox.Text.Trim();
                string phone = PhoneTextBox.Text.Trim();
                string password = PasswordBox.Password;
                string confirmPassword = ConfirmPasswordBox.Password;
                
                // Валидация
                if (!Validators.ValidateRequired(fullName))
                {
                    ShowError("Введите ФИО");
                    return;
                }
                
                if (!Validators.ValidateEmail(email))
                {
                    ShowError("Введите корректный email");
                    return;
                }
                
                if (!Validators.ValidatePhone(phone))
                {
                    ShowError("Введите корректный номер телефона");
                    return;
                }
                
                if (!Validators.ValidatePassword(password))
                {
                    ShowError("Пароль должен содержать минимум 6 символов");
                    return;
                }
                
                if (password != confirmPassword)
                {
                    ShowError("Пароли не совпадают");
                    return;
                }
                
                if (UserManager.Register(fullName, email, phone, password))
                {
                    MessageBox.Show("Регистрация успешна! Теперь войдите в систему.", 
                                   "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    AppFrame.MainFrame.Navigate(new LoginPage());
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка регистрации: {ex.Message}");
            }
        }
        
        private void LoginLink_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.MainFrame.Navigate(new LoginPage());
        }
        
        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}

