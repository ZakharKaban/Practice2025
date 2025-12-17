using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Practice2025One.AppData;
using Practice2025One.Class;

namespace Practice2025One.Pages
{
    public partial class CatalogPage : Page
    {
        private List<ProductViewModel> allProducts;
        
        public CatalogPage()
        {
            InitializeComponent();
        }
        
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
            LoadSuppliers();
        }
        
        private void LoadProducts()
        {
            try
            {
                var products = AppConnect.Model1.Products
                    .Where(p => p.IsActive == 1)
                    .ToList();
                
                allProducts = products.Select(p => new ProductViewModel
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
                    IsExpensive = p.Price > 1000,
                    HasDiscount = p.OldPrice.HasValue && p.OldPrice.Value > 0 && p.DiscountPercent.HasValue,
                    FormattedPrice = FormatPrice(CalculateFinalPrice(p.Price, p.OldPrice, p.DiscountPercent)),
                    FormattedOldPrice = p.OldPrice.HasValue ? FormatPrice(p.OldPrice.Value) : "",
                    DiscountText = p.DiscountPercent.HasValue ? $"-{p.DiscountPercent.Value}%" : "",
                    ImageSource = LoadImage(p.ImagePath)
                }).ToList();
                
                ProductsListView.ItemsSource = allProducts;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadSuppliers()
        {
            try
            {
                var suppliers = AppConnect.Model1.Suppliers.ToList();
                SupplierFilterComboBox.Items.Clear();
                SupplierFilterComboBox.Items.Add(new { SupplierID = 0, Name = "Все поставщики" });
                foreach (var supplier in suppliers)
                {
                    SupplierFilterComboBox.Items.Add(supplier);
                }
                SupplierFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private int CalculateFinalPrice(int price, int? oldPrice, int? discountPercent)
        {
            if (oldPrice.HasValue && discountPercent.HasValue && oldPrice.Value > 0)
            {
                return (int)(oldPrice.Value * (1 - discountPercent.Value / 100.0));
            }
            return price;
        }
        
        private string FormatPrice(int price)
        {
            return $"{price} руб.";
        }
        
        private BitmapImage LoadImage(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return null;
            }
            
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, imagePath);
                if (File.Exists(fullPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch { }
            
            return null;
        }
        
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }
        
        private void FilterChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }
        
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }
        
        private void ApplyFilters()
        {
            if (allProducts == null) return;
            
            var filtered = allProducts.AsEnumerable();
            
            // Поиск
            string searchText = SearchTextBox.Text?.ToLower() ?? "";
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filtered = filtered.Where(p => 
                    p.Name.ToLower().Contains(searchText) ||
                    p.Description.ToLower().Contains(searchText) ||
                    p.Characteristics.ToLower().Contains(searchText));
            }
            
            // Фильтр по поставщику
            if (SupplierFilterComboBox.SelectedItem != null)
            {
                dynamic selected = SupplierFilterComboBox.SelectedItem;
                if (selected.SupplierID != null && selected.SupplierID != 0)
                {
                    filtered = filtered.Where(p => p.SupplierID == selected.SupplierID);
                }
            }
            
            // Сортировка
            if (SortComboBox.SelectedItem != null)
            {
                ComboBoxItem selected = SortComboBox.SelectedItem as ComboBoxItem;
                string tag = selected?.Tag?.ToString();
                
                switch (tag)
                {
                    case "Name":
                        filtered = filtered.OrderBy(p => p.Name);
                        break;
                    case "PriceAsc":
                        filtered = filtered.OrderBy(p => p.Price);
                        break;
                    case "PriceDesc":
                        filtered = filtered.OrderByDescending(p => p.Price);
                        break;
                    case "Supplier":
                        filtered = filtered.OrderBy(p => p.SupplierName);
                        break;
                }
            }
            
            ProductsListView.ItemsSource = filtered.ToList();
        }
        
        private void ResetFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = "";
            SupplierFilterComboBox.SelectedIndex = 0;
            SortComboBox.SelectedIndex = 0;
            ApplyFilters();
        }
        
        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CurrentUser.IsGuest())
                {
                    MessageBox.Show("Для добавления товаров в корзину необходимо войти в систему.", 
                                  "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                    AppFrame.MainFrame.Navigate(new LoginPage());
                    return;
                }
                
                Button button = sender as Button;
                int productId = (int)button.Tag;
                
                var product = AppConnect.Model1.Products.FirstOrDefault(p => p.ProductID == productId);
                if (product == null)
                {
                    MessageBox.Show("Товар не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Получаем или создаем корзину пользователя
                var cart = AppConnect.Model1.Carts
                    .FirstOrDefault(c => c.Users != null && c.Users.UserID == CurrentUser.User.UserID);
                
                if (cart == null)
                {
                    cart = Carts.CreateCarts(0, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    EFHelper.SetCartUser(cart, CurrentUser.User);
                    AppConnect.Model1.AddToCarts(cart);
                    AppConnect.Model1.SaveChanges();
                }
                
                // Проверяем, есть ли уже этот товар в корзине
                var cartItem = AppConnect.Model1.CartItems
                    .Where(ci => ci.Carts != null && ci.Carts.CartID == cart.CartID)
                    .ToList()
                    .FirstOrDefault(ci => 
                        (ci.Products != null && ci.Products.ProductID == productId));
                
                int finalPrice = CalculateFinalPrice(product.Price, product.OldPrice, product.DiscountPercent);
                
                if (cartItem != null)
                {
                    cartItem.Quantity += 1;
                }
                else
                {
                    cartItem = CartItems.CreateCartItems(0, 1, finalPrice);
                    EFHelper.SetCartItemCart(cartItem, cart);
                    if (product != null)
                    {
                        EFHelper.SetCartItemProduct(cartItem, product);
                    }
                    AppConnect.Model1.AddToCartItems(cartItem);
                }
                
                AppConnect.Model1.SaveChanges();
                
                MessageBox.Show("Товар добавлен в корзину!", "Успех", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления в корзину: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    public class ProductViewModel
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
        public bool IsExpensive { get; set; }
        public bool HasDiscount { get; set; }
        public string FormattedPrice { get; set; }
        public string FormattedOldPrice { get; set; }
        public string DiscountText { get; set; }
        public BitmapImage ImageSource { get; set; }
    }
}

