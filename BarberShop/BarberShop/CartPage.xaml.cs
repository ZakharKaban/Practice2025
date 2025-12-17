using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BarberShop
{
    public partial class CartPage : Page
    {
        private List<CartViewModel> _items;

        public CartPage()
        {
            InitializeComponent();
            Loaded += CartPage_Loaded;
        }

        private void CartPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCart();
        }

        private void LoadCart()
        {
            if (!CurrentSession.IsAuthenticated)
            {
                MessageBox.Show("Необходимо войти в систему.", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    _items = db.Cart
                        .Where(c => c.UserID == CurrentSession.CurrentUser.UserID)
                        .Select(c => new CartViewModel
                        {
                            CartID = c.CartID,
                            ServiceID = c.ServiceID,
                            ServiceName = c.Services.ServiceName,
                            Price = c.Services.Price,
                            Quantity = c.Quantity ?? 1
                        })
                        .ToList();
                }

                dgCart.ItemsSource = _items;
                UpdateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки корзины: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotal()
        {
            if (_items == null)
            {
                txtTotal.Text = "0";
                return;
            }

            var sum = _items.Sum(i => i.Total);
            txtTotal.Text = sum.ToString("0.00");
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button != null ? button.DataContext as CartViewModel : null;
            if (item == null)
            {
                return;
            }

            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    var entity = db.Cart.FirstOrDefault(c => c.CartID == item.CartID);
                    if (entity != null)
                    {
                        db.Cart.Remove(entity);
                        db.SaveChanges();
                    }
                }

                LoadCart();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении из корзины: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Checkout_Click(object sender, RoutedEventArgs e)
        {
            if (_items == null || !_items.Any())
            {
                MessageBox.Show("Корзина пуста.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    foreach (var item in _items)
                    {
                        var purchase = new PurchaseHistory
                        {
                            UserID = CurrentSession.CurrentUser.UserID,
                            ServiceID = item.ServiceID,
                            AppointmentDateTime = DateTime.Now,
                            ActualPrice = item.Price,
                            PurchaseDate = DateTime.Now,
                            PaymentMethod = "Не указан",
                            PaymentStatus = "Создан"
                        };

                        db.PurchaseHistory.Add(purchase);

                        var cartEntity = db.Cart.FirstOrDefault(c => c.CartID == item.CartID);
                        if (cartEntity != null)
                        {
                            db.Cart.Remove(cartEntity);
                        }
                    }

                    db.SaveChanges();
                }

                MessageBox.Show("Заказ успешно оформлен.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LoadCart();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при оформлении заказа: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class CartViewModel
    {
        public int CartID { get; set; }
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total
        {
            get { return Price * Quantity; }
        }
    }
}


