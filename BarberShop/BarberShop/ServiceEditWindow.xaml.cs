using System;
using System.Linq;
using System.Windows;

namespace BarberShop
{
    public partial class ServiceEditWindow : Window
    {
        private readonly int? _serviceId;

        public ServiceEditWindow(int? serviceId)
        {
            InitializeComponent();
            _serviceId = serviceId;
            Loaded += ServiceEditWindow_Loaded;
        }

        private void ServiceEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    cbType.ItemsSource = db.ServiceTypes.OrderBy(t => t.TypeName).ToList();

                    if (_serviceId.HasValue)
                    {
                        var service = db.Services.FirstOrDefault(s => s.ServiceID == _serviceId.Value);
                        if (service == null)
                        {
                            return;
                        }

                        txtName.Text = service.ServiceName;
                        txtDescription.Text = service.Description;
                        txtDuration.Text = service.Duration.ToString();
                        txtPrice.Text = service.Price.ToString("0.00");
                        chkIsActive.IsChecked = service.IsActive ?? true;

                        var type = cbType.Items.Cast<ServiceTypes>()
                            .FirstOrDefault(t => t.ServiceTypeID == service.ServiceTypeID);
                        cbType.SelectedItem = type;
                    }
                    else
                    {
                        chkIsActive.IsChecked = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных услуги: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            txtError.Text = string.Empty;

            var name = txtName.Text.Trim();
            var description = txtDescription.Text.Trim();
            var type = cbType.SelectedItem as ServiceTypes;
            int duration;
            decimal price;

            if (string.IsNullOrWhiteSpace(name))
            {
                txtError.Text = "Введите название услуги.";
                return;
            }

            if (type == null)
            {
                txtError.Text = "Выберите тип услуги.";
                return;
            }

            if (!int.TryParse(txtDuration.Text.Trim(), out duration) || duration <= 0)
            {
                txtError.Text = "Введите корректную длительность (целое число > 0).";
                return;
            }

            if (!decimal.TryParse(txtPrice.Text.Trim(), out price) || price <= 0)
            {
                txtError.Text = "Введите корректную цену (число > 0).";
                return;
            }

            try
            {
                using (var db = new BarberShopDBEntities1())
                {
                    Services service;

                    if (_serviceId.HasValue)
                    {
                        service = db.Services.FirstOrDefault(s => s.ServiceID == _serviceId.Value);
                        if (service == null)
                        {
                            txtError.Text = "Услуга не найдена.";
                            return;
                        }
                    }
                    else
                    {
                        service = new Services();
                        db.Services.Add(service);
                    }

                    service.ServiceName = name;
                    service.Description = description;
                    service.ServiceTypeID = type.ServiceTypeID;
                    service.Duration = duration;
                    service.Price = price;
                    service.IsActive = chkIsActive.IsChecked ?? true;

                    db.SaveChanges();
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения услуги: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}


