using System;
using System.Linq;
using System.Windows;
using Practice2025One.AppData;
using Practice2025One.Class;

namespace Practice2025One.Windows
{
    public partial class UserEditWindow : Window
    {
        private int? userId;
        
        public UserEditWindow(int? userId = null)
        {
            InitializeComponent();
            this.userId = userId;
            
            if (userId.HasValue)
            {
                LoadUser();
                Title = "Редактирование пользователя";
            }
            else
            {
                Title = "Добавление пользователя";
                IsActiveCheckBox.IsChecked = true;
            }
        }
        
        private void LoadUser()
        {
            try
            {
                var user = AppConnect.Model1.Users.FirstOrDefault(u => u.UserID == userId.Value);
                if (user == null)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }
                
                FullNameTextBox.Text = user.FullName;
                EmailTextBox.Text = user.Email;
                PhoneTextBox.Text = user.Phone;
                IsActiveCheckBox.IsChecked = user.IsActive == 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователя: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validators.ValidateRequired(FullNameTextBox.Text))
                {
                    ShowError("Введите ФИО");
                    return;
                }
                
                if (!Validators.ValidateEmail(EmailTextBox.Text))
                {
                    ShowError("Введите корректный email");
                    return;
                }
                
                if (!Validators.ValidatePhone(PhoneTextBox.Text))
                {
                    ShowError("Введите корректный номер телефона");
                    return;
                }
                
                Users user;
                if (userId.HasValue)
                {
                    user = AppConnect.Model1.Users.FirstOrDefault(u => u.UserID == userId.Value);
                    if (user == null)
                    {
                        ShowError("Пользователь не найден");
                        return;
                    }
                    
                    // Проверка уникальности email
                    if (user.Email != EmailTextBox.Text && 
                        AppConnect.Model1.Users.Any(u => u.Email == EmailTextBox.Text && u.UserID != userId.Value))
                    {
                        ShowError("Пользователь с таким email уже существует");
                        return;
                    }
                    
                    // Проверка уникальности телефона
                    if (user.Phone != PhoneTextBox.Text && 
                        AppConnect.Model1.Users.Any(u => u.Phone == PhoneTextBox.Text && u.UserID != userId.Value))
                    {
                        ShowError("Пользователь с таким телефоном уже существует");
                        return;
                    }
                }
                else
                {
                    // Проверка уникальности email
                    if (AppConnect.Model1.Users.Any(u => u.Email == EmailTextBox.Text))
                    {
                        ShowError("Пользователь с таким email уже существует");
                        return;
                    }
                    
                    // Проверка уникальности телефона
                    if (AppConnect.Model1.Users.Any(u => u.Phone == PhoneTextBox.Text))
                    {
                        ShowError("Пользователь с таким телефоном уже существует");
                        return;
                    }
                    
                    user = Users.CreateUsers(0, FullNameTextBox.Text.Trim(), EmailTextBox.Text.Trim(), 
                        PhoneTextBox.Text.Trim(), "", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
                        IsActiveCheckBox.IsChecked == true ? 1 : 0);
                    
                    // Роль "Пользователь" по умолчанию
                    var userRole = AppConnect.Model1.Roles.FirstOrDefault(r => r.RoleName == "Пользователь");
                    if (userRole != null)
                    {
                        EFHelper.SetUserRole(user, userRole);
                    }
                    
                    AppConnect.Model1.AddToUsers(user);
                }
                
                user.FullName = FullNameTextBox.Text.Trim();
                user.Email = EmailTextBox.Text.Trim();
                user.Phone = PhoneTextBox.Text.Trim();
                user.IsActive = IsActiveCheckBox.IsChecked == true ? 1 : 0;
                
                // Обновление пароля, если указан
                if (!string.IsNullOrWhiteSpace(PasswordBox.Password))
                {
                    if (!Validators.ValidatePassword(PasswordBox.Password))
                    {
                        ShowError("Пароль должен содержать минимум 6 символов");
                        return;
                    }
                    user.PasswordHash = UserManager.HashPassword(PasswordBox.Password);
                }
                
                AppConnect.Model1.SaveChanges();
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}

