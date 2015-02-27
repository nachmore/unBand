using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace unBand
{
    class PropValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // covers the null case
            if (value == null) 
            {
                return (parameter == null ? Visibility.Visible : Visibility.Collapsed);
            } 
            else if (parameter == null)
            {
                return Visibility.Collapsed;
            }

            var strParameter = parameter as string;
            var strValue = value.ToString();

            if (strParameter.Contains("|"))
            {
                var parameters = strParameter.Split(new char[] {'|'});

                foreach (var singleParam in parameters)
                {
                    if (strValue == singleParam)
                        return Visibility.Visible;
                }
            }

            return (strValue == strParameter ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
