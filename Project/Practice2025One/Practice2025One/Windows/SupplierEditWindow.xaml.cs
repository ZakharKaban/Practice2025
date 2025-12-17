using System;
using System.Linq;
using System.Windows;
using Practice2025One.AppData;
using Practice2025One.Class;

namespace Practice2025One.Windows
{
    public partial class SupplierEditWindow : Window
    {
        private int? supplierId;
        
        public SupplierEditWindow(int? supplierId = null)
        {
            InitializeComponent();
            this.supplierId = supplierId;
            
            if (supplierId.HasValue)
            {
                LoadSupplier();
                Title = "Редактирование поставщика";
            }
            else
            {
                Title = "Добавление поставщика";
            }
        }
        
        private void LoadSupplier()
        {
            try
            {
                var supplier = AppConnect.Model1.Suppliers.FirstOrDefault(s => s.SupplierID == supplierId.Value);
                if (supplier == null)
                {
                    MessageBox.Show("Поставщик не найден.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    DialogResult = false;
                    Close();
                    return;
                }
                
                NameTextBox.Text = supplier.Name;
                ContactInfoTextBox.Text = supplier.ContactInfo ?? "";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщика: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Validators.ValidateRequired(NameTextBox.Text))
                {
                    ShowError("Введите название поставщика");
                    return;
                }
                
                Suppliers supplier;
                if (supplierId.HasValue)
                {
                    supplier = AppConnect.Model1.Suppliers.FirstOrDefault(s => s.SupplierID == supplierId.Value);
                    if (supplier == null)
                    {
                        ShowError("Поставщик не найден");
                        return;
                    }
                }
                else
                {
                    supplier = Suppliers.CreateSuppliers(0, NameTextBox.Text.Trim());
                    AppConnect.Model1.AddToSuppliers(supplier);
                }
                
                supplier.Name = NameTextBox.Text.Trim();
                supplier.ContactInfo = ContactInfoTextBox.Text.Trim();
                
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

