using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace unBand.pages
{
    /// <summary>
    /// Interaction logic for ThemePage.xaml
    /// </summary>
    public partial class ThemePage : UserControl
    {

        private BandManager _band;

        public ThemePage()
        {
            InitializeComponent();

            _band = BandManager.Instance;

            DataContext = _band;
        }

        private void Background_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Images|*.jpg;*.jpeg;*.png";

            if (dialog.ShowDialog() == true)
            {
                _band.Theme.SetBackground(dialog.FileName);
            }
        }
    }
}
