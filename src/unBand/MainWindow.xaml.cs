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
using MahApps.Metro.Controls.Dialogs;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using unBand.BandHelpers;
using System.Timers;

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

            Telemetry.Client.TrackEvent(Telemetry.Events.AppLaunch);

            // Create just creates the singleton - call Start() to actually get things rolling
            BandManager.Create();
        }

        private async void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Properties.Settings.Default.AgreedToFirstRunWarning)
            {
                if (!await AgreeToFirstRunWarning())
                {
                    Telemetry.Client.TrackEvent(Telemetry.Events.DeclinedFirstRunWarning);
                    Application.Current.Shutdown();
                }
            }

            if (!Properties.Settings.Default.AgreedToTelemetry)
            {
                if (!await AgreeToTelemetry())
                {
                    Telemetry.Client.TrackEvent(Telemetry.Events.DeclinedTelemetry);
                    Application.Current.Shutdown();
                }
            }

            ButtonMyBand_Click(null, null);
        }

        private async Task<bool> AgreeToFirstRunWarning()
        {
            var dialogSettings = new MetroDialogSettings();
            dialogSettings.AffirmativeButtonText = "Understood";
            dialogSettings.NegativeButtonText = "Exit";

            var dialogResult = await this.ShowMessageAsync("Hello", 
                "WARNING: This software is not supported by Microsoft, we take no responsibility for what may happen to your Band.\n\n... and no, we haven't seen anything bad... yet.", 
                MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                Properties.Settings.Default.AgreedToFirstRunWarning = true;
                return true;
            }

            return false;
        }

        private async Task<bool> AgreeToTelemetry()
        {
            var dialogSettings = new MetroDialogSettings(); 
            dialogSettings.AffirmativeButtonText = "I agree";
            dialogSettings.NegativeButtonText = "Exit";

            var dialogResult = await this.ShowMessageAsync("Heads Up",
                "We track usage of certain portions of this application, such as section usage and failure information. We don't collect anything personal (ever), but you should still know about it.\n\n", 
                MessageDialogStyle.AffirmativeAndNegative, dialogSettings);

            if (dialogResult == MessageDialogResult.Affirmative)
            {
                Properties.Settings.Default.AgreedToTelemetry = true;
                return true;
            }

            return false;
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

        private void MetroWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void ButtonLog_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new LogOutputPage();
        }

        private void ButtonTiles_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new TilesPage();
        }

        private void ButtonActivityLog_Click(object sender, RoutedEventArgs e)
        {
            PageContent.Content = new ActivityLogPage();
        }
        
    }
}
