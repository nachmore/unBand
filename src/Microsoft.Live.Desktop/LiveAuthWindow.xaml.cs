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
        public LiveAuthWindow(string url, AuthCompletedCallback callback)
        {
            InitializeComponent();
        }
    }
}
