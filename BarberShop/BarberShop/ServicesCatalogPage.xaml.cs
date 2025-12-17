using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BarberShop
{
    public partial class ServicesCatalogPage : Page
    {
        private List<ServiceViewModel> _allServices;

        public ServicesCatalogPage()
        {
            InitializeComponent();
            Loaded += ServicesCatalogPage_Loaded;
        }

        private void ServicesCatalogPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadServiceTypes();
            LoadServices();
            UpdateRoleButtonsVisibility();
        }

        private void LoadServiceTypes()
        {
            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    var types = db.ServiceTypes
                        .OrderBy(t => t.TypeName)
                        .ToList();

                    cbServiceType.Items.Clear();
                    cbServiceType.Items.Add("Все");
                    foreach (var type in types)
                    {
                        cbServiceType.Items.Add(type);
                    }
                    cbServiceType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки типов услуг: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadServices()
        {
            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    _allServices = db.Services
                        .Where(s => s.IsActive == null || s.IsActive == true)
                        .Select(s => new ServiceViewModel
                        {
                            ServiceID = s.ServiceID,
                            ServiceName = s.ServiceName,
                            Description = s.Description,
                            Duration = s.Duration,
                            Price = s.Price,
                            IsActive = s.IsActive ?? true,
                            ServiceTypeName = s.ServiceTypes != null ? s.ServiceTypes.TypeName : string.Empty
                        })
                        .ToList();

                    ApplyFilters();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки услуг: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters()
        {
            if (_allServices == null)
            {
                return;
            }

            var filtered = _allServices.AsEnumerable();

            var search = txtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(search))
            {
                filtered = filtered.Where(s =>
                    (!string.IsNullOrEmpty(s.ServiceName) && s.ServiceName.ToLower().Contains(search)) ||
                    (!string.IsNullOrEmpty(s.Description) && s.Description.ToLower().Contains(search)));
            }

            var selectedType = cbServiceType.SelectedItem as ServiceTypes;
            if (selectedType != null)
            {
                filtered = filtered.Where(s => s.ServiceTypeName == selectedType.TypeName);
            }

            var sortItem = cbSort.SelectedItem as ComboBoxItem;
            var sortTag = sortItem != null ? (sortItem.Tag as string) : null;

            switch (sortTag)
            {
                case "NameAsc":
                    filtered = filtered.OrderBy(s => s.ServiceName);
                    break;
                case "NameDesc":
                    filtered = filtered.OrderByDescending(s => s.ServiceName);
                    break;
                case "PriceAsc":
                    filtered = filtered.OrderBy(s => s.Price);
                    break;
                case "PriceDesc":
                    filtered = filtered.OrderByDescending(s => s.Price);
                    break;
            }

            dgServices.ItemsSource = filtered.ToList();
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentSession.IsAuthenticated)
            {
                MessageBox.Show("Только авторизованные пользователи могут добавлять услуги в корзину.", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = sender as Button;
            var service = button != null ? button.DataContext as ServiceViewModel : null;
            if (service == null)
            {
                return;
            }

            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    var existing = db.Cart.FirstOrDefault(c =>
                        c.UserID == CurrentSession.CurrentUser.UserID &&
                        c.ServiceID == service.ServiceID);

                    if (existing != null)
                    {
                        existing.Quantity = (existing.Quantity ?? 1) + 1;
                    }
                    else
                    {
                        var cartItem = new Cart
                        {
                            UserID = CurrentSession.CurrentUser.UserID,
                            ServiceID = service.ServiceID,
                            Quantity = 1,
                            AddedDate = DateTime.Now
                        };
                        db.Cart.Add(cartItem);
                    }

                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при добавлении в корзину: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddService_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentSession.IsInRole("Менеджер") && !CurrentSession.IsInRole("Manager") &&
                !CurrentSession.IsInRole("Администратор") && !CurrentSession.IsInRole("Administrator") &&
                !CurrentSession.IsInRole("Admin"))
            {
                return;
            }

            var window = new ServiceEditWindow(null);
            if (window.ShowDialog() == true)
            {
                LoadServices();
            }
        }

        private void btnEditService_Click(object sender, RoutedEventArgs e)
        {
            if (!CurrentSession.IsInRole("Менеджер") && !CurrentSession.IsInRole("Manager") &&
                !CurrentSession.IsInRole("Администратор") && !CurrentSession.IsInRole("Administrator") &&
                !CurrentSession.IsInRole("Admin"))
            {
                return;
            }

            var selected = dgServices.SelectedItem as ServiceViewModel;
            if (selected == null)
            {
                MessageBox.Show("Выберите услугу для редактирования.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var window = new ServiceEditWindow(selected.ServiceID);
            if (window.ShowDialog() == true)
            {
                LoadServices();
            }
        }

        private void btnDeleteService_Click(object sender, RoutedEventArgs e)
        {
            var selected = dgServices.SelectedItem as ServiceViewModel;
            if (selected == null)
            {
                MessageBox.Show("Выберите услугу для удаления.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (!CurrentSession.IsInRole("Администратор") && !CurrentSession.IsInRole("Administrator") &&
                !CurrentSession.IsInRole("Admin"))
            {
                MessageBox.Show("Удаление услуг доступно только администратору.", "Доступ запрещен",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    var hasLinks = db.Cart.Any(c => c.ServiceID == selected.ServiceID) ||
                                   db.PurchaseHistory.Any(p => p.ServiceID == selected.ServiceID);

                    if (hasLinks)
                    {
                        MessageBox.Show("Невозможно удалить услугу, так как она присутствует в одном или нескольких заказах.",
                            "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var entity = db.Services.FirstOrDefault(s => s.ServiceID == selected.ServiceID);
                    if (entity == null)
                    {
                        return;
                    }

                    db.Services.Remove(entity);
                    db.SaveChanges();
                }

                LoadServices();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении услуги: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateRoleButtonsVisibility()
        {
            var isManager = CurrentSession.IsInRole("Менеджер") || CurrentSession.IsInRole("Manager");
            var isAdmin = CurrentSession.IsInRole("Администратор") || CurrentSession.IsInRole("Administrator") ||
                          CurrentSession.IsInRole("Admin");

            if (isManager || isAdmin)
            {
                btnAddService.Visibility = Visibility.Visible;
                btnEditService.Visibility = Visibility.Visible;
            }

            if (isAdmin)
            {
                btnDeleteService.Visibility = Visibility.Visible;
            }
        }
    }

    public class ServiceViewModel
    {
        public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public int Duration { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string ServiceTypeName { get; set; }
    }
}


