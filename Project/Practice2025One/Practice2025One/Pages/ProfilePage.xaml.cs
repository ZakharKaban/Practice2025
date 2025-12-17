using System;
using System.Windows;
using System.Windows.Controls;
using Practice2025One.AppData;
using Practice2025One.Class;
using System.Linq;
namespace Practice2025One.Pages
{
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsGuest())
            {
                MessageBox.Show("Для просмотра личного кабинета необходимо войти в систему.", 
                              "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                AppFrame.MainFrame.Navigate(new LoginPage());
                return;
            }
            
            LoadUserData();
        }
        
        private void LoadUserData()
        {
            try
            {
                var user = AppConnect.Model1.Users
                    .FirstOrDefault(u => u.UserID == CurrentUser.User.UserID);
                
                if (user != null)
                {
                    FullNameTextBox.Text = user.FullName;
                    EmailTextBox.Text = user.Email;
                    PhoneTextBox.Text = user.Phone;
                    RoleTextBox.Text = CurrentUser.Role?.RoleName ?? "Не определена";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fullName = FullNameTextBox.Text.Trim();
                string phone = PhoneTextBox.Text.Trim();
                
                // Валидация
                if (!Validators.ValidateRequired(fullName))
                {
                    ShowError("Введите ФИО");
                    return;
                }
                
                if (!Validators.ValidatePhone(phone))
                {
                    ShowError("Введите корректный номер телефона");
                    return;
                }
                
                if (UserManager.UpdateUser(CurrentUser.User.UserID, fullName, phone))
                {
                    ShowSuccess("Данные успешно сохранены!");
                    
                    // Обновляем информацию в главном окне
                    var mainWindow = Application.Current.MainWindow as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.RefreshNavigation();
                    }
                }
                else
                {
                    ShowError("Ошибка сохранения данных");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
        }
        
        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
            SuccessTextBlock.Visibility = Visibility.Collapsed;
        }
        
        private void ShowSuccess(string message)
        {
            SuccessTextBlock.Text = message;
            SuccessTextBlock.Visibility = Visibility.Visible;
            ErrorTextBlock.Visibility = Visibility.Collapsed;
        }
    }
}

