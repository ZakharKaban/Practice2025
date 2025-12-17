using System;
using System.Linq;
using System.Windows;
using Practice2025One.AppData;
using Practice2025One.Class;

namespace Practice2025One.Windows
{
    public partial class ChangeRoleWindow : Window
    {
        private int userId;
        
        public ChangeRoleWindow(int userId)
        {
            InitializeComponent();
            this.userId = userId;
            LoadRoles();
            LoadUserRole();
        }
        
        private void LoadRoles()
        {
            try
            {
                var roles = AppConnect.Model1.Roles.ToList();
                RoleComboBox.ItemsSource = roles;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadUserRole()
        {
            try
            {
                var user = AppConnect.Model1.Users.FirstOrDefault(u => u.UserID == userId);
                if (user != null)
                {
                    if (!user.RolesReference.IsLoaded)
                    {
                        user.RolesReference.Load();
                    }
                    Roles role = user.Roles;
                    if (role == null && user.RolesReference.EntityKey != null)
                    {
                        int roleId = (int)user.RolesReference.EntityKey.EntityKeyValues[0].Value;
                        role = AppConnect.Model1.Roles.FirstOrDefault(r => r.RoleID == roleId);
                    }
                    if (role != null)
                    {
                        RoleComboBox.SelectedItem = role;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки роли пользователя: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (RoleComboBox.SelectedItem == null)
                {
                    ShowError("Выберите роль");
                    return;
                }
                
                Roles selectedRole = RoleComboBox.SelectedItem as Roles;
                var user = AppConnect.Model1.Users.FirstOrDefault(u => u.UserID == userId);
                
                if (user == null)
                {
                    ShowError("Пользователь не найден");
                    return;
                }
                
                EFHelper.SetUserRole(user, selectedRole);
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

