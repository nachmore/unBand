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
        private const string LOGGED_IN_URL = "https://login.live.com/oauth20_desktop.srf";
        private const string LOGGED_OUT_URL = "https://login.live.com/oauth20_logout.srf";

        private string _url;
        private AuthCompletedCallback _callback;

        public LiveAuthWindow(string url, AuthCompletedCallback callback, string title = null)
        {
            InitializeComponent();

            if (title != null)
                Title = title;

            _url = url;
            _callback = callback;
        }

        private void browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.ToString().StartsWith(LOGGED_IN_URL) || e.Uri.ToString().StartsWith(LOGGED_OUT_URL))
            {
                // close the window since auth is complete
                this.Close();

                if (_callback != null)
                {
                    _callback(new AuthResult(e.Uri));
                }
            }
            else
            {
                // show the window since we now need user input
                this.Show();
            }
        }

        public void Login()
        {
            browser.Navigate(_url);
        }

        public void LogOut(string url)
        {
            browser.Navigate(url);
        }
    }
}
