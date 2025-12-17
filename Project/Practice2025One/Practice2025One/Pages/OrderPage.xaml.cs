using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Practice2025One.AppData;
using Practice2025One.Class;

namespace Practice2025One.Pages
{
    public partial class OrderPage : Page
    {
        public OrderPage()
        {
            InitializeComponent();
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsGuest())
            {
                MessageBox.Show("Для оформления заказа необходимо войти в систему.", 
                              "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                AppFrame.MainFrame.Navigate(new LoginPage());
                return;
            }
            
            LoadCartItems();
        }
        
        private void LoadCartItems()
        {
            try
            {
                var cart = AppConnect.Model1.Carts
                    .FirstOrDefault(c => c.Users != null && c.Users.UserID == CurrentUser.User.UserID);
                
                if (cart == null)
                {
                    MessageBox.Show("Корзина пуста.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    AppFrame.MainFrame.Navigate(new CartPage());
                    return;
                }
                
                var cartItems = AppConnect.Model1.CartItems
                    .Where(ci => ci.Carts != null && ci.Carts.CartID == cart.CartID)
                    .ToList()
                    .Select(ci => new OrderItemViewModel
                    {
                        ProductID = ci.Products != null ? ci.Products.ProductID :
                                   (ci.ProductsReference.EntityKey != null ?
                                    (int)ci.ProductsReference.EntityKey.EntityKeyValues[0].Value : 0),
                        ProductName = ci.Products?.Name ?? "Неизвестный товар",
                        Quantity = ci.Quantity,
                        PriceAtMoment = ci.PriceAtMoment,
                        TotalPrice = ci.Quantity * ci.PriceAtMoment
                    }).ToList();
                
                OrderItemsListView.ItemsSource = cartItems;
                
                int total = cartItems.Sum(ci => ci.TotalPrice);
                TotalPriceTextBlock.Text = $"Итого к оплате: {total} руб.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.MainFrame.Navigate(new CartPage());
        }
        
        private void ConfirmOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string deliveryAddress = DeliveryAddressTextBox.Text.Trim();
                
                if (string.IsNullOrWhiteSpace(deliveryAddress))
                {
                    MessageBox.Show("Введите адрес доставки.", 
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var cart = AppConnect.Model1.Carts
                    .FirstOrDefault(c => c.Users != null && c.Users.UserID == CurrentUser.User.UserID);
                
                if (cart == null)
                {
                    MessageBox.Show("Корзина пуста.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var cartItems = AppConnect.Model1.CartItems
                    .Where(ci => ci.Carts != null && ci.Carts.CartID == cart.CartID)
                    .ToList();
                
                if (cartItems.Count == 0)
                {
                    MessageBox.Show("Корзина пуста.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                // Генерация номера заказа
                string orderNumber = GenerateOrderNumber();
                
                // Создание заказа
                Orders newOrder = Orders.CreateOrders(0, orderNumber, deliveryAddress, 
                    cartItems.Sum(ci => ci.Quantity * ci.PriceAtMoment), 
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), "Новый");
                EFHelper.SetOrderUser(newOrder, CurrentUser.User);
                
                AppConnect.Model1.AddToOrders(newOrder);
                AppConnect.Model1.SaveChanges();
                
                // Создание элементов заказа
                foreach (var cartItem in cartItems)
                {
                    OrderItems orderItem = OrderItems.CreateOrderItems(0, cartItem.Quantity, cartItem.PriceAtMoment);
                    EFHelper.SetOrderItemOrder(orderItem, newOrder);
                    if (cartItem.ProductsReference.EntityKey != null)
                    {
                        orderItem.ProductsReference.EntityKey = cartItem.ProductsReference.EntityKey;
                    }
                    else
                    {
                        int? productId = EFHelper.GetCartItemProductID(cartItem);
                        Products product = null;
                        if (productId.HasValue)
                        {
                            product = AppConnect.Model1.Products.FirstOrDefault(p => p.ProductID == productId.Value);
                        }
                        else if (cartItem.Products != null)
                        {
                            product = cartItem.Products;
                        }
                        if (product != null)
                        {
                            EFHelper.SetOrderItemProduct(orderItem, product);
                        }
                    }
                    AppConnect.Model1.AddToOrderItems(orderItem);
                }
                
                // Очистка корзины
                foreach (var cartItem in cartItems)
                {
                    AppConnect.Model1.DeleteObject(cartItem);
                }
                
                AppConnect.Model1.SaveChanges();
                
                MessageBox.Show($"Заказ успешно оформлен!\nНомер заказа: {orderNumber}", 
                              "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                
                AppFrame.MainFrame.Navigate(new OrderHistoryPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка оформления заказа: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private string GenerateOrderNumber()
        {
            string prefix = "ORD";
            string date = DateTime.Now.ToString("yyyyMMdd");
            int random = new Random().Next(1000, 9999);
            return $"{prefix}-{date}-{random}";
        }
    }
    
    public class OrderItemViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int PriceAtMoment { get; set; }
        public int TotalPrice { get; set; }
    }
}

