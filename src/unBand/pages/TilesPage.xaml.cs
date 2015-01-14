using Microsoft.Cargo.Client;
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
    public partial class TilesPage : UserControl
    {

        private BandManager _band;

        public TilesPage()
        {
            InitializeComponent();

            _band = BandManager.Instance;

            DataContext = _band;
        }

        // Drag & Drop care of http://stackoverflow.com/a/4004590
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            ListBox parent = sender as ListBox;
            CargoStrapp data = e.Data.GetData(typeof(CargoStrapp)) as CargoStrapp;
            CargoStrapp objectToPlaceBefore = GetObjectDataFromPoint(parent, e.GetPosition(parent)) as CargoStrapp;

            if (data != null && objectToPlaceBefore != null)
            {
                int index = _band.Tiles.Strip.IndexOf(objectToPlaceBefore);

                _band.Tiles.Strip.Remove(data);
                _band.Tiles.Strip.Insert(index, data);
                parent.SelectedItem = data;
            }
        }

        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox parent = sender as ListBox;

            CargoStrapp data = GetObjectDataFromPoint(parent, e.GetPosition(parent)) as CargoStrapp;

            if (data != null)
            {
                DragDrop.DoDragDrop(parent, data, DragDropEffects.Move);
            }
        }

        private static object GetObjectDataFromPoint(ListBox source, Point point)
        {
            UIElement element = source.InputHitTest(point) as UIElement;
            if (element != null)
            {
                object data = DependencyProperty.UnsetValue;

                while (data == DependencyProperty.UnsetValue)
                {
                    data = source.ItemContainerGenerator.ItemFromContainer(element);

                    if (data == DependencyProperty.UnsetValue)
                        element = VisualTreeHelper.GetParent(element) as UIElement;

                    if (element == source)
                        return null;
                }

                if (data != DependencyProperty.UnsetValue)
                    return data;
            }

            return null;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            _band.Tiles.Save();
        }

        private void btnClearCounts_Click(object sender, RoutedEventArgs e)
        {
            _band.Tiles.ClearAllCounts();
        }
    }
}
