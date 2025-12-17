using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Practice2025One.AppData;
using Practice2025One.Class;
using Practice2025One.Pages;

namespace Practice2025One
{


    public partial class MainWindow : Window
    {
       

        // Инициализация при загрузке
        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
           
        }

        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void MainFrame_Loaded(object sender, RoutedEventArgs e)
        {
            AppFrame.MainFrame = MainFrame;
            UpdateNavigationMenu();
            NavigateToLogin();
        }
        
        private void NavigateToLogin()
        {
            MainFrame.Navigate(new LoginPage());
        }
        
        private void UpdateNavigationMenu()
        {
            NavigationMenu.Items.Clear();
            NavigationMenu.DisplayMemberPath = "Name";
            
            if (CurrentUser.IsGuest())
            {
                NavigationMenu.Items.Add(new NavItem { Name = "Вход", Page = "Login" });
                NavigationMenu.Items.Add(new NavItem { Name = "Регистрация", Page = "Register" });
            }
            else
            {
                // Общие пункты для всех авторизованных
                NavigationMenu.Items.Add(new NavItem { Name = "Каталог товаров", Page = "Catalog" });
                NavigationMenu.Items.Add(new NavItem { Name = "Корзина", Page = "Cart" });
                NavigationMenu.Items.Add(new NavItem { Name = "Мои заказы", Page = "OrderHistory" });
                NavigationMenu.Items.Add(new NavItem { Name = "Личный кабинет", Page = "Profile" });
                
                // Для менеджера и администратора
                if (CurrentUser.IsManager() || CurrentUser.IsAdmin())
                {
                    NavigationMenu.Items.Add(new NavItem { Name = "Управление товарами", Page = "ProductsManagement" });
                    NavigationMenu.Items.Add(new NavItem { Name = "Управление поставщиками", Page = "SuppliersManagement" });
                    NavigationMenu.Items.Add(new NavItem { Name = "Управление заказами", Page = "OrdersManagement" });
                }
                
                // Только для администратора
                if (CurrentUser.IsAdmin())
                {
                    NavigationMenu.Items.Add(new NavItem { Name = "Управление пользователями", Page = "UsersManagement" });
                }
            }
            
            UpdateUserInfo();
        }
        
        private void UpdateUserInfo()
        {
            if (CurrentUser.User != null)
            {
                UserInfoText.Text = $"{CurrentUser.User.FullName} ({CurrentUser.Role?.RoleName})";
                LogoutButton.Visibility = Visibility.Visible;
            }
            else
            {
                UserInfoText.Text = "Гость";
                LogoutButton.Visibility = Visibility.Collapsed;
            }
        }
        
        private void NavigationMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavigationMenu.SelectedItem == null) return;
            
            var selectedItem = NavigationMenu.SelectedItem as NavItem;
            if (selectedItem == null) return;
            string pageName = selectedItem.Page;
            
            Page page = null;
            switch (pageName)
            {
                case "Login":
                    page = new LoginPage();
                    break;
                case "Register":
                    page = new RegisterPage();
                    break;
                case "Catalog":
                    page = new CatalogPage();
                    break;
                case "Cart":
                    page = new CartPage();
                    break;
                case "OrderHistory":
                    page = new OrderHistoryPage();
                    break;
                case "Profile":
                    page = new ProfilePage();
                    break;
                case "ProductsManagement":
                    page = new ProductsManagementPage();
                    break;
                case "SuppliersManagement":
                    page = new SuppliersManagementPage();
                    break;
                case "OrdersManagement":
                    page = new OrdersManagementPage();
                    break;
                case "UsersManagement":
                    page = new UsersManagementPage();
                    break;
            }
            
            if (page != null)
            {
                MainFrame.Navigate(page);
            }
        }
        
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Logout();
            UpdateNavigationMenu();
            NavigateToLogin();
        }
        
        public void RefreshNavigation()
        {
            UpdateNavigationMenu();
        }
    }
    
    public class NavItem
    {
        public string Name { get; set; }
        public string Page { get; set; }
        public override string ToString() => Name;
    }
}
