using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NexteLite.Converters
{
    internal class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool isOnline)
            {
                if (isOnline)
                    return Properties.Resources.lcl_txt_PlayButton_Online;
                return Properties.Resources.lcl_txt_PlayButton_Offline;
            }

            return Properties.Resources.lcl_txt_PlayButton_Offline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
