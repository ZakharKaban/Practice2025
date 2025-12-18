using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Practice2025One.AppData;
using Practice2025One.Pages;

namespace Practice2025One.Pages
{
    public partial class OrderDetailsPage : Page
    {
        private int orderId;
        
        public OrderDetailsPage(int orderId)
        {
            InitializeComponent();
            this.orderId = orderId;
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrderDetails();
        }
        
        private void LoadOrderDetails()
        {
            try
            {
                var order = AppConnect.Model1.Orders.FirstOrDefault(o => o.OrderID == orderId);
                
                if (order == null)
                {
                    MessageBox.Show("Заказ не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    AppFrame.MainFrame.GoBack();
                    return;
                }
                
                OrderNumberTextBlock.Text = order.OrderNumber;
                OrderDateTextBlock.Text = order.OrderDate;
                DeliveryAddressTextBlock.Text = order.DeliveryAddress;
                StatusTextBlock.Text = order.Status;
                TotalPriceTextBlock.Text = $"{order.TotalPrice} руб.";
                
              var orderItems = AppConnect.Model1.OrderItems
                    .Where(oi => oi.OrdersReference.EntityKey != null && 
                                (int)oi.OrdersReference.EntityKey.EntityKeyValues[0].Value == orderId)
                    .ToList()
                    .Select(oi => new OrderItemViewModel
                    {
                        ProductID = oi.Products != null ? oi.Products.ProductID : 
                                   (oi.ProductsReference.EntityKey != null ? 
                                    (int)oi.ProductsReference.EntityKey.EntityKeyValues[0].Value : 0),
                        ProductName = oi.Products?.Name ?? "Неизвестный товар",
                        Quantity = oi.Quantity,
                        PriceAtMoment = oi.PriceAtMoment,
                        TotalPrice = oi.Quantity * oi.PriceAtMoment
                    }).ToList();
                
                OrderItemsListView.ItemsSource = orderItems;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки деталей заказа: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppFrame.MainFrame.CanGoBack)
            {
                AppFrame.MainFrame.GoBack();
            }
            else
            {
                AppFrame.MainFrame.Navigate(new OrderHistoryPage());
            }
        }
    }
}

