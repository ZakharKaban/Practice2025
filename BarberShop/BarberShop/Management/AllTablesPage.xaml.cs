using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BarberShop.Management
{
    public partial class AllTablesPage : Page
    {
        private BarberShopDBEntities1 _db;

        public AllTablesPage()
        {
            InitializeComponent();
            Loaded += AllTablesPage_Loaded;
        }

        private void AllTablesPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!CurrentSession.IsInRole("Менеджер") && !CurrentSession.IsInRole("Manager") &&
                !CurrentSession.IsInRole("Администратор") && !CurrentSession.IsInRole("Administrator") &&
                !CurrentSession.IsInRole("Admin"))
            {
                MessageBox.Show("Нет прав для управления таблицами.", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _db = new BarberShopDBEntities1();

            dgServices.ItemsSource = _db.Services.ToList();
            dgServiceTypes.ItemsSource = _db.ServiceTypes.ToList();
            dgEmployees.ItemsSource = _db.Employees.ToList();
            dgPromotions.ItemsSource = _db.Promotions.ToList();

            var isAdmin = CurrentSession.IsInRole("Администратор") ||
                          CurrentSession.IsInRole("Administrator") ||
                          CurrentSession.IsInRole("Admin");

            btnDelete.IsEnabled = isAdmin;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
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
            var isAdmin = CurrentSession.IsInRole("Администратор") ||
                          CurrentSession.IsInRole("Administrator") ||
                          CurrentSession.IsInRole("Admin");
            if (!isAdmin)
            {
                MessageBox.Show("Удаление записей доступно только администратору.", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedTab = tabControl.SelectedItem as TabItem;
            if (selectedTab == null)
            {
                return;
            }

            DataGrid grid = null;
            if (selectedTab.Content == dgServices)
            {
                grid = dgServices;
            }
            else if (selectedTab.Content == dgServiceTypes)
            {
                grid = dgServiceTypes;
            }
            else if (selectedTab.Content == dgEmployees)
            {
                grid = dgEmployees;
            }
            else if (selectedTab.Content == dgPromotions)
            {
                grid = dgPromotions;
            }

            if (grid == null || grid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (grid == dgServices)
            {
                var service = grid.SelectedItem as Services;
                if (service != null)
                {
                    var hasLinks = _db.Cart.Any(c => c.ServiceID == service.ServiceID) ||
                                   _db.PurchaseHistory.Any(p => p.ServiceID == service.ServiceID);

                    if (hasLinks)
                    {
                        MessageBox.Show("Невозможно удалить услугу, так как она присутствует в одном или нескольких заказах.",
                            "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _db.Services.Remove(service);
                }
            }
            else
            {
                _db.Entry(grid.SelectedItem).State = System.Data.Entity.EntityState.Deleted;
            }

            try
            {
                _db.SaveChanges();
                grid.ItemsSource = grid.ItemsSource;
                AllTablesPage_Loaded(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


