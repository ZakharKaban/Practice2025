using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Practice2025One.AppData;
using Practice2025One.Class;
using Practice2025One.Windows;

namespace Practice2025One.Pages
{
    public partial class UsersManagementPage : Page
    {
        public UsersManagementPage()
        {
            InitializeComponent();
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (!CurrentUser.IsAdmin())
            {
                MessageBox.Show("У вас нет доступа к этой странице.", 
                              "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                AppFrame.MainFrame.Navigate(new CatalogPage());
                return;
            }
            
            LoadUsers();
        }
        
        private void LoadUsers()
        {
            try
            {
                var users = AppConnect.Model1.Users
                    .OrderBy(u => u.UserID)
                    .ToList()
                    .Select(u => new UserManagementViewModel
                    {
                        UserID = u.UserID,
                        FullName = u.FullName,
                        Email = u.Email,
                        Phone = u.Phone,
                        RoleName = u.Roles != null ? u.Roles.RoleName : "Не определена",
                        RoleID = u.Roles != null ? u.Roles.RoleID : 0,
                        IsActive = u.IsActive == 1,
                        CreatedAt = u.CreatedAt
                    }).ToList();
                
                UsersListView.ItemsSource = users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            UserEditWindow window = new UserEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadUsers();
            }
        }
        
        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (UsersListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования.", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            UserManagementViewModel selected = UsersListView.SelectedItem as UserManagementViewModel;
            if (selected != null)
            {
                UserEditWindow window = new UserEditWindow(selected.UserID);
                if (window.ShowDialog() == true)
                {
                    LoadUsers();
                }
            }
        }
        
        private void ChangeRoleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UsersListView.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пользователя для изменения роли.", 
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                UserManagementViewModel selected = UsersListView.SelectedItem as UserManagementViewModel;
                if (selected == null) return;
                
                ChangeRoleWindow window = new ChangeRoleWindow(selected.UserID);
                if (window.ShowDialog() == true)
                {
                    LoadUsers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (UsersListView.SelectedItem == null)
                {
                    MessageBox.Show("Выберите пользователя для удаления.", 
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                UserManagementViewModel selected = UsersListView.SelectedItem as UserManagementViewModel;
                if (selected == null) return;
                
                // Нельзя удалить самого себя
                if (selected.UserID == CurrentUser.User.UserID)
                {
                    MessageBox.Show("Нельзя удалить самого себя.", 
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого пользователя?", 
                                           "Подтверждение", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    var user = AppConnect.Model1.Users.FirstOrDefault(u => u.UserID == selected.UserID);
                    if (user != null)
                    {
                        AppConnect.Model1.DeleteObject(user);
                        AppConnect.Model1.SaveChanges();
                        LoadUsers();
                        MessageBox.Show("Пользователь успешно удален.", 
                                      "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    public class UserManagementViewModel
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string RoleName { get; set; }
        public int RoleID { get; set; }
        public bool IsActive { get; set; }
        public string CreatedAt { get; set; }
    }
}

