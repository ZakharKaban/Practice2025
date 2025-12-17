using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Practice2025One.AppData;
using Practice2025One.Class;

namespace Practice2025One.Pages
{
    public partial class OrdersManagementPage : Page
    {
        private bool _isLoadingOrders = false;
        private bool _isSettingStatus = false;
        
        public OrdersManagementPage()
        {
            InitializeComponent();
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsGuest() || (!CurrentUser.IsManager() && !CurrentUser.IsAdmin()))
            {
                MessageBox.Show("У вас нет доступа к этой странице.", 
                              "Доступ запрещен", MessageBoxButton.OK, MessageBoxImage.Warning);
                AppFrame.MainFrame.Navigate(new CatalogPage());
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
                _isLoadingOrders = true;
                var orders = AppConnect.Model1.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();
                
                // Фильтрация по статусу
                string selectedStatus = StatusFilterComboBox.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Все статусы")
                {
                    orders = orders.Where(o => o.Status == selectedStatus).ToList();
                }
                
                var orderViewModels = orders.Select(o => new OrderManagementViewModel
                {
                    OrderID = o.OrderID,
                    OrderNumber = o.OrderNumber,
                    UserName = o.Users?.FullName ?? "Неизвестный пользователь",
                    OrderDate = o.OrderDate,
                    DeliveryAddress = o.DeliveryAddress,
                    Status = o.Status,
                    TotalPrice = $"{o.TotalPrice} руб."
                }).ToList();
                
                OrdersListView.ItemsSource = orderViewModels;
                _isLoadingOrders = false;
            }
            catch (Exception ex)
            {
                _isLoadingOrders = false;
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
        
        private void StatusComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox == null || comboBox.Tag == null) return;
            
            int orderId = (int)comboBox.Tag;
            var order = AppConnect.Model1.Orders.FirstOrDefault(o => o.OrderID == orderId);
            if (order != null)
            {
                // Устанавливаем текущий статус
                _isSettingStatus = true;
                foreach (ComboBoxItem item in comboBox.Items)
                {
                    if (item.Content.ToString() == order.Status)
                    {
                        comboBox.SelectedItem = item;
                        break;
                    }
                }
                _isSettingStatus = false;
            }
        }
        
        private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (_isLoadingOrders || _isSettingStatus) return;
                
                ComboBox comboBox = sender as ComboBox;
                if (comboBox == null || comboBox.Tag == null) return;
                
                int orderId = (int)comboBox.Tag;
                ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;
                if (selectedItem == null) return;
                
                string newStatus = selectedItem.Content.ToString();
                
                var order = AppConnect.Model1.Orders.FirstOrDefault(o => o.OrderID == orderId);
                if (order != null)
                {
                    order.Status = newStatus;
                    AppConnect.Model1.SaveChanges();
                    LoadOrders();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения статуса: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                OrderManagementViewModel selected = OrdersListView.SelectedItem as OrderManagementViewModel;
                if (selected != null)
                {
                    AppFrame.MainFrame.Navigate(new OrderDetailsPage(selected.OrderID));
                }
            }
        }
    }
    
    public class OrderManagementViewModel
    {
        public int OrderID { get; set; }
        public string OrderNumber { get; set; }
        public string UserName { get; set; }
        public string OrderDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string Status { get; set; }
        public string TotalPrice { get; set; }
    }
}

