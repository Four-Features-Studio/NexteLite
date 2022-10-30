using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Services
{
    public class AppSettings
    {
        public SettingsFolders DirParams { get; set; }
        public string DefaultPath { get; set; }
        public int DefaultRam { get; set; }

        public List<SocialItem> Social { get; set; }

        public string AssetsUrl { get; set; }
        public string InjectorUrl { get; set; }
    }

    public class SocialItem
    {
        public string ImageResource { get; set; }
        public string ImageCode { get; set; }
        public string Url { get; set; }
    }

    public class SettingsFolders
    {
        public string SettingsPath { get; set; }
        public string SettingsLoginName { get; set; }
        public string SettingsName { get; set; }
        public string SettingsExtension { get; set; }
    }
}
