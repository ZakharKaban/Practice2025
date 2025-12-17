using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Practice2025One.AppData;
using Practice2025One.Class;

namespace Practice2025One.Pages
{
    public partial class OrderHistoryPage : Page
    {
        public OrderHistoryPage()
        {
            InitializeComponent();
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsGuest())
            {
                MessageBox.Show("Для просмотра истории заказов необходимо войти в систему.", 
                              "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                AppFrame.MainFrame.Navigate(new LoginPage());
                return;
            }
            
            LoadStatusFilter();
            LoadOrders();
        }
        
        private void LoadStatusFilter()
        {
            StatusFilterComboBox.Items.Clear();
            StatusFilterComboBox.Items.Add("Все статусы");
            StatusFilterComboBox.Items.Add("Новый");
            StatusFilterComboBox.Items.Add("В обработке");
            StatusFilterComboBox.Items.Add("Доставляется");
            StatusFilterComboBox.Items.Add("Выполнен");
            StatusFilterComboBox.Items.Add("Отменен");
            StatusFilterComboBox.SelectedIndex = 0;
        }
        
        private void LoadOrders()
        {
            try
            {
                var orders = AppConnect.Model1.Orders
                    .Where(o => o.Users != null && o.Users.UserID == CurrentUser.User.UserID)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
                
                // Фильтрация по статусу
                string selectedStatus = StatusFilterComboBox.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Все статусы")
                {
                    orders = orders.Where(o => o.Status == selectedStatus).ToList();
                }
                
                var orderViewModels = orders.Select(o => new OrderViewModel
                {
                    OrderID = o.OrderID,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    DeliveryAddress = o.DeliveryAddress,
                    Status = o.Status,
                    TotalPrice = $"{o.TotalPrice} руб."
                }).ToList();
                
                OrdersListView.ItemsSource = orderViewModels;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadOrders();
        }
        
        private void ResetFilterButton_Click(object sender, RoutedEventArgs e)
        {
            StatusFilterComboBox.SelectedIndex = 0;
            LoadOrders();
        }
        
        private void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            int orderId = (int)button.Tag;
            AppFrame.MainFrame.Navigate(new OrderDetailsPage(orderId));
        }
        
        private void OrdersListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (OrdersListView.SelectedItem != null)
            {
                OrderViewModel selected = OrdersListView.SelectedItem as OrderViewModel;
                if (selected != null)
                {
                    AppFrame.MainFrame.Navigate(new OrderDetailsPage(selected.OrderID));
                }
            }
        }
    }
    
    public class OrderViewModel
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public string OrderDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string Status { get; set; }
        public string TotalPrice { get; set; }
    }
}

