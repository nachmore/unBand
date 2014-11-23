using Microsoft.Cargo.Client;
using Microsoft.Live;
using Microsoft.Live.Desktop;
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
    public partial class ActivityLogPage : UserControl
    {

        private BandManager _band;

        public ActivityLogPage()
        {
            InitializeComponent();

            _band = BandManager.Instance;

            DataContext = _band;

            var liveAuthClient = new LiveAuthClient("000000004811DB42");

            string startUrl = liveAuthClient.GetLoginUrl(new List<string>() {"service::prodkds.dns-cargo.com::MBI_SSL"});
            
            var authForm = new LiveAuthWindow(
                startUrl,
                this.OnAuthCompleted);

            authForm.Show();
        }

        private void OnAuthCompleted(AuthResult result)
        {
            System.Diagnostics.Debug.WriteLine(result);
        }

    }
}
