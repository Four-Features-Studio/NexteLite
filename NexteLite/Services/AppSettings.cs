using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Services
{
    public class AppSettings
    {
        public SettingsFolders SettingSection { get; set; }

        public string DefaultPath { get; set; }
        public int DefaultRam { get; set; }
    }

    public class SettingsFolders
    {
        public string SettingsPath { get; set; }
        public string SettingsLoginName { get; set; }
        public string SettingsName { get; set; }
        public string SettingsExtension { get; set; }
    }
}
