using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace NexteLite.Converters
{
    public class BoolToBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //[0] Bool
            //[1] First brush
            //[2] Second brush
            if(values.Length == 3 && values[0] is bool isOnline && values[1] is SolidColorBrush firstColor && values[2] is SolidColorBrush secondColor)
            {
                if (isOnline)
                {
                    return firstColor;
                }

                return secondColor;
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
