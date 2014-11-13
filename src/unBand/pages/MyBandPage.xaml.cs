using Microsoft.Cargo.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
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
using unBand.BandHelpers;

namespace unBand.pages
{
    /// <summary>
    /// Interaction logic for MyBandPage.xaml
    /// </summary>
    public partial class MyBandPage : UserControl
    {

        private BandManager _band;

        public MyBandPage()
        {
            InitializeComponent();

            _band = BandManager.Instance;

            DataContext = _band;
        }

        private void BandMeTile_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // TODO: this whole paradigm is horrible
            Window parentWindow = Window.GetWindow(this);
            ((MainWindow)(parentWindow)).Navigate(new ThemePage());
        }
    }
}
