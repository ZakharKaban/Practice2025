using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BarberShop
{
    public partial class OrdersHistoryPage : Page
    {
        public OrdersHistoryPage()
        {
            InitializeComponent();
            Loaded += OrdersHistoryPage_Loaded;
        }

        private void OrdersHistoryPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
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
                    var list = db.PurchaseHistory
                        .Where(p => p.UserID == CurrentSession.CurrentUser.UserID)
                        .OrderByDescending(p => p.PurchaseDate)
                        .Select(p => new OrderHistoryViewModel
                        {
                            ServiceName = p.Services.ServiceName,
                            PurchaseDate = p.PurchaseDate,
                            AppointmentDateTime = p.AppointmentDateTime,
                            ActualPrice = p.ActualPrice,
                            PaymentMethod = p.PaymentMethod,
                            PaymentStatus = p.PaymentStatus
                        })
                        .ToList();

                    dgOrders.ItemsSource = list;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки истории заказов: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    public class OrderHistoryViewModel
    {
        public string ServiceName { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public DateTime? AppointmentDateTime { get; set; }
        public decimal ActualPrice { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
    }
}


