using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace NexteLite.Converters
{
    public class PresetIdToTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //[0] SelectPresetId
            //[1] Presets
            if (values.Length == 2 && values[0] is string id && values[1] is IEnumerable<ServerPreset> presets)
            {
                if(presets.Any(x => x.Id == id))
                {
                    var selected = presets.FirstOrDefault(x => x.Id == id);
                    return selected.Title;
                }

            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
