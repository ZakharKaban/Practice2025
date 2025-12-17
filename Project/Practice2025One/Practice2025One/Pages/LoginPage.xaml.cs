using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Practice2025One.AppData;
using Practice2025One.Class;

namespace Practice2025One.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }
        
        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string login = LoginTextBox.Text.Trim();
                string password = PasswordBox.Password;
                
                if (string.IsNullOrWhiteSpace(login))
                {
                    ShowError("Введите email или телефон");
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(password))
                {
                    ShowError("Введите пароль");
                    return;
                }
                
                if (UserManager.Login(login, password))
                {
                    // Обновляем навигацию в главном окне
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.RefreshNavigation();
                    }
                    
                    // Переходим на каталог
                    AppFrame.MainFrame.Navigate(new CatalogPage());
                }
                else
                {
                    ShowError("Неверный email/телефон или пароль");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка входа: {ex.Message}");
            }
        }
        
        private void RegisterLink_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.MainFrame.Navigate(new RegisterPage());
        }
        
        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}

