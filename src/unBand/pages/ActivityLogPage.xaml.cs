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
using unBand.Cloud;

namespace unBand.pages
{
    /// <summary>
    /// Interaction logic for MyBandPage.xaml
    /// </summary>
    public partial class ActivityLogPage : UserControl
    {

        private BandManager _band;
        private BandCloudClient _cloud;


        public List<BandEventBase> Events { get; set; }

        public ActivityLogPage()
        {
            InitializeComponent();

            _band = BandManager.Instance;

            Init();
        }

        private async void Init()
        {
            // TODO: for now, we're doing everything internally should move to some kind of wrapped view model
            _cloud = new BandCloudClient();
            _cloud.AuthenticationCompleted += cloud_AuthenticationCompleted;
            _cloud.Login();
        }

        internal async void cloud_AuthenticationCompleted()
        {
            Events = await _cloud.GetEvents();

            DataContext = this;
        }

    }
}
