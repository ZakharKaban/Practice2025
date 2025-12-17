using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Practice2025One.AppData;
using Practice2025One.Class;
using Practice2025One.Windows;

namespace Practice2025One.Pages
{
    public partial class ProductsManagementPage : Page
    {
        public ProductsManagementPage()
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
            
            if (CurrentUser.IsAdmin())
            {
                DeleteProductButton.Visibility = Visibility.Visible;
            }
            
            LoadProducts();
        }
        
        private void LoadProducts()
        {
            try
            {
                var products = AppConnect.Model1.Products.ToList()
                    .Select(p => new ProductManagementViewModel
                    {
                        ProductID = p.ProductID,
                        Name = p.Name,
                        Description = p.Description ?? "",
                        Price = p.Price,
                        OldPrice = p.OldPrice,
                        DiscountPercent = p.DiscountPercent,
                        Characteristics = p.Characteristics ?? "",
                        SupplierName = p.Suppliers?.Name ?? "",
                        SupplierID = p.Suppliers != null ? p.Suppliers.SupplierID :
                                    (p.SuppliersReference.EntityKey != null ?
                                     (int)p.SuppliersReference.EntityKey.EntityKeyValues[0].Value : 0),
                        IsActive = p.IsActive == 1,
                        ImagePath = p.ImagePath
                    }).ToList();
                
                ProductsListView.ItemsSource = products;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Можно добавить фильтрацию по поиску
            LoadProducts();
        }
        
        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            ProductEditWindow window = new ProductEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadProducts();
            }
        }
        
        private void EditProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите товар для редактирования.", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            ProductManagementViewModel selected = ProductsListView.SelectedItem as ProductManagementViewModel;
            if (selected != null)
            {
                ProductEditWindow window = new ProductEditWindow(selected.ProductID);
                if (window.ShowDialog() == true)
                {
                    LoadProducts();
                }
            }
        }
        
        private void DeleteProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductsListView.SelectedItem == null)
                {
                    MessageBox.Show("Выберите товар для удаления.", 
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                ProductManagementViewModel selected = ProductsListView.SelectedItem as ProductManagementViewModel;
                if (selected == null) return;
                
                // Проверка наличия товара в заказах
                bool hasOrders = AppConnect.Model1.OrderItems
                    .Any(oi => (oi.ProductsReference.EntityKey != null && 
                               (int)oi.ProductsReference.EntityKey.EntityKeyValues[0].Value == selected.ProductID) ||
                              (oi.Products != null && oi.Products.ProductID == selected.ProductID));
                
                if (hasOrders)
                {
                    MessageBox.Show("Невозможно удалить товар, так как он присутствует в одном или нескольких заказах.", 
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?", 
                                           "Подтверждение", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    var product = AppConnect.Model1.Products
                        .FirstOrDefault(p => p.ProductID == selected.ProductID);
                    
                    if (product != null)
                    {
                        AppConnect.Model1.DeleteObject(product);
                        AppConnect.Model1.SaveChanges();
                        LoadProducts();
                        MessageBox.Show("Товар успешно удален.", 
                                      "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления товара: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    public class ProductManagementViewModel
    {
        public int ProductID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Price { get; set; }
        public int? OldPrice { get; set; }
        public int? DiscountPercent { get; set; }
        public string Characteristics { get; set; }
        public string SupplierName { get; set; }
        public int SupplierID { get; set; }
        public bool IsActive { get; set; }
        public string ImagePath { get; set; }
    }
}

