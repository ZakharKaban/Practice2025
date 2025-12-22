using DemoEx.AppData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
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

using System.Data.Entity; // для Include()
namespace DemoEx.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageCatalog.xaml
    /// </summary>
    public partial class PageCatalog : Page
    {
        
       

        public PageCatalog()
        {
            InitializeComponent();

            List<Products> producta = AppConect.Model1.Products.Include(x=> x.Suppliers).ToList();
            listViewMain.ItemsSource = producta;

        }

        private void listViewMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
