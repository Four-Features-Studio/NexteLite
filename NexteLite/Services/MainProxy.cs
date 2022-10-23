using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Services
{
    public class MainProxy : IMainProxy
    {
        public SetProfileFunction SetProfile { get; set; }
        public SetServerProfilesFunction SetServerProfiles { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public event SettingsClickHandler SettingsClick;

        /// <summary>
        /// Функция позволяющая разлогиниться в лаунчере
        /// </summary>
        public event LogoutClickHandler LogoutClick;
        public event SocialClickHandler SocialClick;
        public event PlayClickHandler PlayClick;

        public void OpenSettings()
        {
            SettingsClick?.Invoke();
        }
    }
}
