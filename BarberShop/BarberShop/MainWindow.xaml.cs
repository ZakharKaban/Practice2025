using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BarberShop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeByRole();
            OpenServicesCatalog();
        }

        private void InitializeByRole()
        {
            if (CurrentSession.CurrentUser != null)
            {
                txtWelcome.Text = $"Добро пожаловать, {CurrentSession.CurrentUser.FirstName} {CurrentSession.CurrentUser.LastName}!";

                var roleName = CurrentSession.CurrentUser.UserRoles != null
                    ? CurrentSession.CurrentUser.UserRoles.RoleName
                    : string.Empty;

                if (!string.IsNullOrWhiteSpace(roleName))
                {
                    txtWelcome.Text += $" (Роль: {roleName})";
                }

                if (CurrentSession.IsInRole("Менеджер") || CurrentSession.IsInRole("Manager") ||
                    CurrentSession.IsInRole("Администратор") || CurrentSession.IsInRole("Administrator") ||
                    CurrentSession.IsInRole("Admin"))
                {
                    menuManagement.Visibility = Visibility.Visible;
                }

                if (CurrentSession.IsInRole("Администратор") || CurrentSession.IsInRole("Administrator") ||
                    CurrentSession.IsInRole("Admin"))
                {
                    menuUsers.Visibility = Visibility.Visible;
                }
            }
            else
            {
                txtWelcome.Text = "Вы вошли как гость.";
            }
        }

        private void Menu_Profile_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UserProfilePage());
        }

        private void Menu_Logout_Click(object sender, RoutedEventArgs e)
        {
            CurrentSession.CurrentUser = null;
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            Close();
        }

        private void Menu_Services_Click(object sender, RoutedEventArgs e)
        {
            OpenServicesCatalog();
        }

        private void Menu_Cart_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CartPage());
        }

        private void Menu_Orders_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new OrdersHistoryPage());
        }

        private void Menu_Management_AllTables_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Management.AllTablesPage());
        }

        private void Menu_Management_Users_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Management.UsersManagementPage());
        }

        private void OpenServicesCatalog()
        {
            MainFrame.Navigate(new ServicesCatalogPage());
        }
    }
}
