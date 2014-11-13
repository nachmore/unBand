using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace unBand
{
    class DesignHelpers
    {
        public static DependencyProperty IsHiddenProperty = DependencyProperty.RegisterAttached(("IsHidden"), typeof(bool), typeof(DesignHelpers),
            new FrameworkPropertyMetadata(false, new PropertyChangedCallback(OnIsHiddenChanged)));

        public static void SetIsHidden(UIElement element, bool value)
        {
            element.SetValue(IsHiddenProperty, value);
        }

        public static bool GetIsHidden(UIElement element)
        {
            return (bool)(element.GetValue(IsHiddenProperty));
        }

        private static void OnIsHiddenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {

            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(d))
            {
                if (true.Equals(e.NewValue))
                {
                    ((FrameworkElement)d).LayoutTransform = new ScaleTransform(0.001, 0.001);
                }
                else
                {
                    ((FrameworkElement)d).LayoutTransform = null;
                }
            }
        }
    }
}
