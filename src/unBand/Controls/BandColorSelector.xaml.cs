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
using Xceed.Wpf.Toolkit;

namespace unBand.Controls
{
    /// <summary>
    /// Interaction logic for BandColorSelector.xaml
    /// </summary>
    public partial class BandColorSelector : UserControl
    {
        public BandColorSelector()
        {
            InitializeComponent();
        }
        
        private void Color_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var border = sender as Border;

            (border.Child as ColorPicker).IsOpen = true;
        }

        private void Color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Telemetry.Client.TrackEvent(
                Telemetry.Events.ChangeThemeColor, 
                new Dictionary<string, string>() {
                    {"Category", Tag as string},
                    {"Color", e.NewValue.ToString()}
                },
                null);

            var band = DataContext as BandManager;

            var prop = band.Theme.GetType().GetProperty(Tag as string);

            prop.SetValue(band.Theme, new SolidColorBrush(e.NewValue));
        }

        private void ColorPicker_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // swallow this event so that it is not forwarded to our parent (who would reopen the picker if
            // the user selected the currently selected color)
            
            e.Handled = true;
        }
    }
}
