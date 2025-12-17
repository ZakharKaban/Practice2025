using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BarberShop.Management
{
    public partial class UsersManagementPage : Page
    {
        private BarberShopDBEntities1 _db;

        public UsersManagementPage()
        {
            InitializeComponent();
            Loaded += UsersManagementPage_Loaded;
        }

        private void UsersManagementPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!CurrentSession.IsInRole("Администратор") &&
                !CurrentSession.IsInRole("Administrator") &&
                !CurrentSession.IsInRole("Admin"))
            {
                MessageBox.Show("Управление пользователями доступно только администратору.", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _db = new BarberShopDBEntities1();

            var users = _db.Users
                .Select(u => new UserManagementViewModel
                {
                    UserID = u.UserID,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    Email = u.Email,
                    RoleID = u.RoleID,
                    RoleName = u.UserRoles.RoleName,
                    IsActive = u.IsActive ?? true
                })
                .ToList();

            dgUsers.ItemsSource = users;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var list = dgUsers.ItemsSource.Cast<UserManagementViewModel>().ToList();

                foreach (var item in list)
                {
                    var user = _db.Users.FirstOrDefault(u => u.UserID == item.UserID);
                    if (user == null)
                    {
                        continue;
                    }

                    user.FirstName = item.FirstName;
                    user.LastName = item.LastName;
                    user.PhoneNumber = item.PhoneNumber;
                    user.Email = item.Email;
                    user.IsActive = item.IsActive;

                    if (user.RoleID != item.RoleID)
                    {
                        user.RoleID = item.RoleID;
                    }
                }

                _db.SaveChanges();
                MessageBox.Show("Изменения сохранены.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgUsers.SelectedItem as UserManagementViewModel;
            if (selected == null)
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (selected.UserID == CurrentSession.CurrentUser.UserID)
            {
                MessageBox.Show("Нельзя удалить самого себя.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var user = _db.Users.FirstOrDefault(u => u.UserID == selected.UserID);
                if (user == null)
                {
                    return;
                }

                _db.Users.Remove(user);
                _db.SaveChanges();

                UsersManagementPage_Loaded(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления пользователя: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class UserManagementViewModel
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}


