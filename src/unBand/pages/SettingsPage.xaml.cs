using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
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
using unBand.Cloud;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System.Reflection;

namespace unBand.pages
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class SettingsPage : UserControl, INotifyPropertyChanged
    {
        private string _heading;
        public string Heading
        {
            get { return _heading; }
            set
            {
                if (_heading != value)
                {
                    _heading = value;

                    NotifyPropertyChanged();
                }
            }
        }

        public SettingsPage(bool updated = false)
        {
            InitializeComponent();

            this.DataContext = About.Current;

            Heading = updated ? "unBand has been updated! Here's what you got:" : "Changelog";
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }));
            }
        }

        #endregion

        private async void ButtonClearLoginInfo_Click(object sender, RoutedEventArgs e)
        {
            new BandCloudClient().Logout();

            await ((MetroWindow)(Window.GetWindow(this))).ShowMessageAsync("Restart Required", "We'll now restart unBand to flush your login session...");

            Relaunch();
        }

        // TODO: This should probably live in a utils lib
        private void Relaunch()
        {
            Process rv = null;
            ProcessStartInfo processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

            // relaunch with runas to get elevated
            processInfo.UseShellExecute = true;

            try
            {
                rv = Process.Start(processInfo);

                Application.Current.Shutdown();
            }
            catch (Exception)
            {
            }
        }



    }
}
