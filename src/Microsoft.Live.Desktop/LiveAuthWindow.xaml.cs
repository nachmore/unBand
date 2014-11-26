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
using System.Windows.Shapes;

namespace Microsoft.Live.Desktop
{
    public delegate void AuthCompletedCallback(AuthResult result);
    
    /// <summary>
    /// Interaction logic for LiveAuthWindow.xaml
    /// </summary>
    public partial class LiveAuthWindow : Window
    {
        private const string END_URL = "https://login.live.com/oauth20_desktop.srf";

        private string _url;
        private AuthCompletedCallback _callback;

        public LiveAuthWindow(string url, AuthCompletedCallback callback)
        {
            InitializeComponent();

            _url = url;
            _callback = callback;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            browser.Navigate(_url);
        }

        private void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(END_URL))
            {
                if (_callback != null)
                {
                    _callback(new AuthResult(e.Uri));
                }
            }

            // close the window when auth is completed
            this.Close();
        }
        
    }
}
