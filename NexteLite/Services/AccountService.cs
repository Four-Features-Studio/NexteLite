﻿using NexteLite.Interfaces;
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

        public async Task<(bool result, Profile profile, string message)> AuthAccount(string username, string password)
        {
            if (_SettingsLauncher.SaveLoginParams(new ParamsLoginPage(username, String.Empty, false)))
            {
                var data = await _Web.Auth(username, password);

                if (data.result)
                {
                    Profile = data.profile;
                    return new (true, Profile, string.Empty);
                }

                return new(true, null, data.message);
            }
            else
            {
                return new(false, null, string.Empty);
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
