using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NexteLite.Converters
{
    public class StatusAndPercantageCombineConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //[0] IsDownloading bool
            //[1] Status string
            //[2] Percentage double
            if(values.Length == 3 && values[0] is bool downloading && values[1] is string status && values[2] is double percentage)
            {
                var format = "{0}";
                if (downloading)
                    format = "{0} {1:n0}%";

                var result = string.Format(format, status.ToUpper(), percentage);
                return result;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
