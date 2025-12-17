using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Practice2025One.AppData;
using Practice2025One.Class;
using Practice2025One.Windows;

namespace Practice2025One.Pages
{
    public partial class SuppliersManagementPage : Page
    {
        public SuppliersManagementPage()
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
                DeleteSupplierButton.Visibility = Visibility.Visible;
            }
            
            LoadSuppliers();
        }
        
        private void LoadSuppliers()
        {
            try
            {
                var suppliers = AppConnect.Model1.Suppliers.ToList();
                SuppliersListView.ItemsSource = suppliers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void AddSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            SupplierEditWindow window = new SupplierEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadSuppliers();
            }
        }
        
        private void EditSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            if (SuppliersListView.SelectedItem == null)
            {
                MessageBox.Show("Выберите поставщика для редактирования.", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            Suppliers selected = SuppliersListView.SelectedItem as Suppliers;
            if (selected != null)
            {
                SupplierEditWindow window = new SupplierEditWindow(selected.SupplierID);
                if (window.ShowDialog() == true)
                {
                    LoadSuppliers();
                }
            }
        }
        
        private void DeleteSupplierButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (SuppliersListView.SelectedItem == null)
                {
                    MessageBox.Show("Выберите поставщика для удаления.", 
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                Suppliers selected = SuppliersListView.SelectedItem as Suppliers;
                if (selected == null) return;
                
                // Проверка наличия товаров у поставщика
                bool hasProducts = AppConnect.Model1.Products
                    .Any(p => (p.SuppliersReference.EntityKey != null &&
                               (int)p.SuppliersReference.EntityKey.EntityKeyValues[0].Value == selected.SupplierID) ||
                              (p.Suppliers != null && p.Suppliers.SupplierID == selected.SupplierID));
                
                if (hasProducts)
                {
                    MessageBox.Show("Невозможно удалить поставщика, так как у него есть товары.", 
                                  "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                var result = MessageBox.Show("Вы уверены, что хотите удалить этого поставщика?", 
                                           "Подтверждение", 
                                           MessageBoxButton.YesNo, 
                                           MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    AppConnect.Model1.DeleteObject(selected);
                    AppConnect.Model1.SaveChanges();
                    LoadSuppliers();
                    MessageBox.Show("Поставщик успешно удален.", 
                                  "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления поставщика: {ex.Message}", 
                              "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

