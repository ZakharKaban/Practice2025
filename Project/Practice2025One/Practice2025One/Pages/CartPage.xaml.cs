using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Practice2025One.AppData;
using Practice2025One.Class;

namespace Practice2025One.Pages
{
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsGuest())
            {
                MessageBox.Show("Для просмотра корзины необходимо войти в систему.", 
                              "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                AppFrame.MainFrame.Navigate(new LoginPage());
                return;
            }
            
            LoadCart();
        }
        
        private void LoadCart()
        {
            try
            {
                var cart = AppConnect.Model1.Carts
                    .FirstOrDefault(c => c.Users != null && c.Users.UserID == CurrentUser.User.UserID);
                
                if (cart == null)
                {
                    CartItemsListView.ItemsSource = null;
                    TotalPriceTextBlock.Text = "Итого: 0 руб.";
                    return;
                }
                
                var cartItems = AppConnect.Model1.CartItems
                    .Where(ci => ci.Carts != null && ci.Carts.CartID == cart.CartID)
                    .ToList()
                    .Select(ci => new CartItemViewModel
                    {
                        CartItemID = ci.CartItemID,
                        ProductID = ci.Products != null ? ci.Products.ProductID : 
                                   (ci.ProductsReference.EntityKey != null ? 
                                    (int)ci.ProductsReference.EntityKey.EntityKeyValues[0].Value : 0),
                        ProductName = ci.Products?.Name ?? "Неизвестный товар",
                        Quantity = ci.Quantity,
                        PriceAtMoment = ci.PriceAtMoment,
                        TotalPrice = ci.Quantity * ci.PriceAtMoment
                    }).ToList();
                
                CartItemsListView.ItemsSource = cartItems;
                UpdateTotalPrice(cartItems);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void UpdateTotalPrice(System.Collections.IEnumerable items)
        {
            int total = 0;
            foreach (CartItemViewModel item in items)
            {
                total += item.TotalPrice;
            }
            TotalPriceTextBlock.Text = $"Итого: {total} руб.";
        }
        
        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                int cartItemId = (int)button.Tag;
                
                var cartItem = AppConnect.Model1.CartItems
                    .FirstOrDefault(ci => ci.CartItemID == cartItemId);
                
                if (cartItem != null)
                {
                    cartItem.Quantity += 1;
                    AppConnect.Model1.SaveChanges();
                    LoadCart();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения количества: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                int cartItemId = (int)button.Tag;
                
                var cartItem = AppConnect.Model1.CartItems
                    .FirstOrDefault(ci => ci.CartItemID == cartItemId);
                
                if (cartItem != null)
                {
                    if (cartItem.Quantity > 1)
                    {
                        cartItem.Quantity -= 1;
                        AppConnect.Model1.SaveChanges();
                        LoadCart();
                    }
                    else
                    {
                        RemoveCartItem(cartItemId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения количества: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                int cartItemId = (int)button.Tag;
                
                var result = MessageBox.Show("Удалить товар из корзины?", 
                                           "Подтверждение", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    RemoveCartItem(cartItemId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления товара: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void RemoveCartItem(int cartItemId)
        {
            var cartItem = AppConnect.Model1.CartItems
                .FirstOrDefault(ci => ci.CartItemID == cartItemId);
            
            if (cartItem != null)
            {
                AppConnect.Model1.DeleteObject(cartItem);
                AppConnect.Model1.SaveChanges();
                LoadCart();
            }
        }
        
        private void ClearCartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show("Очистить всю корзину?", 
                                           "Подтверждение", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    var cart = AppConnect.Model1.Carts
                        .FirstOrDefault(c => c.Users != null && c.Users.UserID == CurrentUser.User.UserID);
                    
                    if (cart != null)
                    {
                        var cartItems = AppConnect.Model1.CartItems
                            .Where(ci => ci.Carts != null && ci.Carts.CartID == cart.CartID)
                            .ToList();
                        
                        foreach (var item in cartItems)
                        {
                            AppConnect.Model1.DeleteObject(item);
                        }
                        
                        AppConnect.Model1.SaveChanges();
                        LoadCart();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка очистки корзины: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void CreateOrderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
                
                AppFrame.MainFrame.Navigate(new OrderPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    public class CartItemViewModel
    {
        public int CartItemID { get; set; }
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public int PriceAtMoment { get; set; }
        public int TotalPrice { get; set; }
    }
}

