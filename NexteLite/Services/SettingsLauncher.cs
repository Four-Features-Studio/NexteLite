using NexteLite.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Services
{
    public class SettingsLauncher : ISettingsLauncher
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Save { get; set; }
        public string RootDir { get; set; }
        public int UseRam { get; set; }
        public bool AutoConnect { get; set; }
        public bool Debug { get; set; }
        public bool FullScreen { get; set; }

        public void LoadSettings()
        {

        }
        public void SaveSettings()
        {

        }
    }
}
