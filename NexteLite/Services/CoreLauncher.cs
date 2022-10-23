using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Pages;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NexteLite.Services
{
    public class CoreLauncher : ICoreLaucnher
    {
        IMainWindow Main;
        IPagesRepository Pages;
        IWebService Web;

        ILoginProxy LoginProxy;
        IMainProxy MainProxy;
        ISettingsLauncher SettingsLauncher;

        public CoreLauncher(IMainWindow mainWindow, IPagesRepository pages, IWebService web, ISettingsLauncher settings, ILoginProxy login, IMainProxy main)
        {
            Main = mainWindow;
            Pages = pages;
            Web = web;
            SettingsLauncher = settings;

            LoginProxy = login;
            MainProxy = main;

            LoginProxy.LoginClick += LoginProxy_LoginClick;

            MainProxy.SettingsClick += MainProxy_SettingsClick;

            Main.Show();
            ShowPage(PageType.Login);
        }

        private List<ServerProfile> ServerProfiles = new List<ServerProfile>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="save"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        private bool LoginProxy_LoginClick(string username, string password, bool save, out string message)
        {
            message = string.Empty;
            if (Web.Auth(username, password, out var profile, ref message))
            {
                MainProxy.SetProfile(profile);
                ShowPage(PageType.Main);
                LoadProfiles();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadProfiles()
        {
            var profiles = Web.GetServerProfiles();
            ServerProfiles = profiles;
            MainProxy.SetServerProfiles(ServerProfiles);
        }

        private void MainProxy_SettingsClick()
        {
            ShowOverlay(PageType.Settings);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        public void ShowPage(PageType id)
        {
            Main.ShowPage(Pages.GetPage(id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void ShowOverlay(PageType id)
        {
            Main.ShowOverlay(Pages.GetPage(id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="save"></param>
        public void LoginPage_Login(string username, string password, bool save)
        {
            //заглушка
            ShowPage(PageType.Main);
        }
    }
}
