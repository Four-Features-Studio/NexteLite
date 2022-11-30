using NexteLite.Models;
using NexteLite.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate void LogoutClickHandler();
    public delegate void SocialClickHandler(string url);
    public delegate void SettingsClickHandler();

    public delegate void PlayClickHandler(string id, string presetId);

    public interface IMainProxy
    {
        event SettingsClickHandler SettingsClick;
        event LogoutClickHandler LogoutClick;
        event SocialClickHandler SocialClick;
        event PlayClickHandler PlayClick;

        void AddSocial(SocialItem social);
        void SetProfile(Profile profile);
        void SetServerProfiles(List<ServerProfile> servers);
    }
}
