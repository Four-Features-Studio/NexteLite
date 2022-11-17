using NexteLite.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NexteLite.Converters
{
    public class DoubleToSpeedNetworkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is double bytes && double.IsNormal(bytes))
            {
                var result = DoubleUtil.SizeSuffix((long)bytes, 2);
                return result;  
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

    }
}
