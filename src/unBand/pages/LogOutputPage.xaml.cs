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
using unBand.BandHelpers;

namespace unBand.pages
{
    /// <summary>
    /// Interaction logic for MyBandPage.xaml
    /// </summary>
    public partial class LogOutputPage : UserControl
    {

        private BandManager _band;

        public LogOutputPage()
        {
            InitializeComponent();

            _band = BandManager.Instance;

            DataContext = _band;
        }
    }
}
