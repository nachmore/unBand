using MahApps.Metro.Controls;
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
using unBand.pages;

namespace unBand
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ButtonMyBand_Click(null, null);
        }

        private void ButtonMyBand_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new MyBandPage();
        }
        
        private void ButtonTheme_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new ThemePage();
        }
        
        private void ButtonSensors_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new SensorsPage();
        }

        public void Navigate(UserControl content)
        {
            PageContent.Content = content;
        }
    }
}
