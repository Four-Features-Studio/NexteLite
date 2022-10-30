using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NexteLite.Services
{
    public class AccountService : IAccountService
    {
        public Profile Profile { get; set; }

        ISettingsLauncher _SettingsLauncher;
        IWebService _Web;

        public AccountService(ISettingsLauncher settingsLauncher, IWebService webService)
        {
            _SettingsLauncher = settingsLauncher;
            _Web = webService;
        }

        public bool AuthAccount(string username, string password, out Profile profile, ref string message)
        {
            profile = null;

            if (_SettingsLauncher.SaveLoginParams(new ParamsLoginPage(username, String.Empty, false)))
            {
                if (_Web.Auth(username, password, out var prf, ref message))
                {
                    profile = prf;
                    Profile = profile;
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }
        }

        public Profile GetProfile()
        {
            return Profile;
        }
        public string GetAccessToken()
        {
            return Profile.AccessToken;
        }

        public string GetServerToken()
        {
            return Profile.ServerToken;
        }

        public string GetUsername()
        {
            return Profile.Username;
        }

        public string GetUuid()
        {
            return Profile.Uuid;
        }

        public bool Logout()
        {
            throw new NotImplementedException();
        }
    }
}
