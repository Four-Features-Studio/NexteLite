using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public interface ISettingsLauncher
    {
        string Username { get; set; }
        string Password { get; set; }
        bool Save { get; set; }

        string RootDir { get; set; }
        int UseRam { get; set; }
        bool AutoConnect { get; set; }
        bool Debug { get; set; }
        bool FullScreen { get; set; }

        public Dictionary<string, string> SelectedPresets { get; set; }

        IParamsLoginPage LoadLoginParams();

        bool SaveLoginParams(IParamsLoginPage data);

        IParamsSettingPage LoadSettingsParams();

        bool SaveSettingsParams(IParamsSettingPage data);

        void LoadLastSelectedPreset();
        bool SaveLastSelectedPreset(string profileId, string selectId);
    }
}
