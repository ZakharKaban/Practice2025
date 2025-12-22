using DemoEx.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;




namespace DemoEx.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageAut.xaml
    /// </summary>
    public partial class PageAut : Page
    {
        public PageAut()
        {
            InitializeComponent();
            AppConect.Model1 = new Entities3();
            
        }



        private void buttonAuth_Click(object sender, RoutedEventArgs e)
        {

          var Penis = AppConect.Model1.Users.FirstOrDefault(x=> x.Email==textBoxLogin.Text);
            if (Penis == null) { MessageBox.Show("Неверная почта"); }
            else 
            {
                PageCatalog papa = new PageCatalog();
                NavigationService?.Navigate(papa);
            }

        }


    }
}
