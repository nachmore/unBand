using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace unBand.MapHelpers
{
    public static class MapShapes
    {
        public static UIElement GetDataPointShape()
        {
            return new Ellipse()
            {
                Height = 5,
                Width = 5,
                Stroke = Brushes.Red
            };
        }

        public static UIElement GetStartShape(string text)
        {
            return new Button()
            {
                Content = text,
                Padding = new Thickness(3),
                MinHeight = 12
            };
        }

        public static UIElement GetEndShape(string text)
        {
            return new Button()
            {
                Content = text,
                Padding = new Thickness(3),
                MinHeight = 12
            };
        }
    }
}
