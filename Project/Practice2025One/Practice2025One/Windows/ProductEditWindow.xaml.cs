using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Practice2025One.AppData;
using Practice2025One.Class;

namespace Practice2025One.Windows
{
    public partial class ProductEditWindow : Window
    {
        private int? productId;
        private string selectedImagePath;
        
        public ProductEditWindow(int? productId = null)
        {
            InitializeComponent();
            this.productId = productId;
            LoadSuppliers();
            
            if (productId.HasValue)
            {
                LoadProduct();
                Title = "Редактирование товара";
            }
            else
            {
                Title = "Добавление товара";
                IsActiveCheckBox.IsChecked = true;
            }
        }
        
        private void LoadSuppliers()
        {
            try
            {
                var suppliers = AppConnect.Model1.Suppliers.ToList();
                SupplierComboBox.ItemsSource = suppliers;
                if (suppliers.Count > 0)
                {
                    SupplierComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadProduct()
        {
            try
            {
                var product = AppConnect.Model1.Products.FirstOrDefault(p => p.ProductID == productId.Value);
                if (product == null)
                {
                    MessageBox.Show("Товар не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }
                
                NameTextBox.Text = product.Name;
                DescriptionTextBox.Text = product.Description ?? "";
                PriceTextBox.Text = product.Price.ToString();
                OldPriceTextBox.Text = product.OldPrice?.ToString() ?? "";
                DiscountPercentTextBox.Text = product.DiscountPercent?.ToString() ?? "";
                CharacteristicsTextBox.Text = product.Characteristics ?? "";
                IsActiveCheckBox.IsChecked = product.IsActive == 1;
                
                Suppliers supplier = product.Suppliers;
                if (supplier == null && product.SuppliersReference.EntityKey != null)
                {
                    int supplierId = (int)product.SuppliersReference.EntityKey.EntityKeyValues[0].Value;
                    supplier = AppConnect.Model1.Suppliers.FirstOrDefault(s => s.SupplierID == supplierId);
                }
                if (supplier != null)
                {
                    SupplierComboBox.SelectedItem = supplier;
                }
                
                if (!string.IsNullOrWhiteSpace(product.ImagePath))
                {
                    ImagePathTextBox.Text = product.ImagePath;
                    LoadImage(product.ImagePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товара: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp|Все файлы (*.*)|*.*",
                Title = "Выберите изображение товара"
            };
            
            if (dialog.ShowDialog() == true)
            {
                try
                {
                    string sourcePath = dialog.FileName;
                    string fileName = Path.GetFileName(sourcePath);
                    string productImagesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProductImages");
                    
                    // Создаем папку, если её нет
                    if (!Directory.Exists(productImagesFolder))
                    {
                        Directory.CreateDirectory(productImagesFolder);
                    }
                    
                    string destinationPath = Path.Combine(productImagesFolder, fileName);
                    
                    // Если файл уже существует, добавляем уникальный суффикс
                    int counter = 1;
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                    string extension = Path.GetExtension(fileName);
                    while (File.Exists(destinationPath))
                    {
                        fileName = $"{nameWithoutExt}_{counter}{extension}";
                        destinationPath = Path.Combine(productImagesFolder, fileName);
                        counter++;
                    }
                    
                    File.Copy(sourcePath, destinationPath, true);
                    
                    // Сохраняем относительный путь
                    selectedImagePath = Path.Combine("ProductImages", fileName);
                    ImagePathTextBox.Text = selectedImagePath;
                    LoadImage(selectedImagePath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", 
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        private void LoadImage(string imagePath)
        {
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
                    ProductImage.Source = bitmap;
                }
            }
            catch { }
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация
                if (!Validators.ValidateRequired(NameTextBox.Text))
                {
                    ShowError("Введите название товара");
                    return;
                }
                
                if (!int.TryParse(PriceTextBox.Text, out int price) || price <= 0)
                {
                    ShowError("Введите корректную цену (положительное число)");
                    return;
                }
                
                int? oldPrice = null;
                if (!string.IsNullOrWhiteSpace(OldPriceTextBox.Text))
                {
                    if (!int.TryParse(OldPriceTextBox.Text, out int oldPriceValue) || oldPriceValue <= 0)
                    {
                        ShowError("Старая цена должна быть положительным числом");
                        return;
                    }
                    oldPrice = oldPriceValue;
                }
                
                int? discountPercent = null;
                if (!string.IsNullOrWhiteSpace(DiscountPercentTextBox.Text))
                {
                    if (!int.TryParse(DiscountPercentTextBox.Text, out int discountValue) || discountValue < 0 || discountValue > 100)
                    {
                        ShowError("Процент скидки должен быть от 0 до 100");
                        return;
                    }
                    discountPercent = discountValue;
                }
                
                if (SupplierComboBox.SelectedItem == null)
                {
                    ShowError("Выберите поставщика");
                    return;
                }
                
                Suppliers selectedSupplier = SupplierComboBox.SelectedItem as Suppliers;
                
                Products product;
                if (productId.HasValue)
                {
                    product = AppConnect.Model1.Products.FirstOrDefault(p => p.ProductID == productId.Value);
                    if (product == null)
                    {
                        ShowError("Товар не найден");
                        return;
                    }
                }
                else
                {
                    product = new Products();
                    AppConnect.Model1.AddToProducts(product);
                }
                
                product.Name = NameTextBox.Text.Trim();
                product.Description = DescriptionTextBox.Text.Trim();
                product.Price = price;
                product.OldPrice = oldPrice;
                product.DiscountPercent = discountPercent;
                product.Characteristics = CharacteristicsTextBox.Text.Trim();
                product.SuppliersReference.EntityKey = new System.Data.EntityKey("Entities1.Suppliers", "SupplierID", selectedSupplier.SupplierID);
                product.IsActive = IsActiveCheckBox.IsChecked == true ? 1 : 0;
                
                if (!string.IsNullOrWhiteSpace(selectedImagePath))
                {
                    product.ImagePath = selectedImagePath;
                }
                else if (!productId.HasValue)
                {
                    product.ImagePath = null;
                }
                
                AppConnect.Model1.SaveChanges();
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void ShowError(string message)
        {
            ErrorTextBlock.Text = message;
            ErrorTextBlock.Visibility = Visibility.Visible;
        }
    }
}

