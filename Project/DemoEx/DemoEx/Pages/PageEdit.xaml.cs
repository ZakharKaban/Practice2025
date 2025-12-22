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

using System.Data.Entity;


namespace DemoEx.Pages
{
    /// <summary>
    /// Логика взаимодействия для PageEdit.xaml
    /// </summary>
    public partial class PageEdit : Page
    {
        public int Id;
        public PageEdit(int Idshnk, string NameSup)
        {
            InitializeComponent();
            TextBlokSupName.Text = NameSup;
            Id = Idshnk;
        }

        private void buttonIban_Click(object sender, RoutedEventArgs e)
        {
            var baba = AppConect.Model1.Products.FirstOrDefault(x => x.ProductID == Id);
            baba.SupplierID = 1;

            AppConect.Model1.Products.Add(2,"Niga","","",300, 0,);
            AppConect.Model1.SaveChanges();

        }
    }
}
