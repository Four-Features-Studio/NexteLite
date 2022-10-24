﻿using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Pages;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NexteLite.Services
{
    public class CoreLauncher : ICoreLaucnher
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        IMainWindow _Main;
        IPagesRepository _Pages;
        IWebService _Web;

        ILoginProxy _LoginProxy;
        IMainProxy _MainProxy;
        ISettingsProxy _SettingsProxy;
        IConsoleProxy _ConsoleProxy;
        ISettingsLauncher _SettingsLauncher;

        public CoreLauncher(IMainWindow mainWindow, IPagesRepository pagesRepository, IWebService webService, ISettingsLauncher settingsLauncher)
        {
            _Main = mainWindow;
            _Pages = pagesRepository;
            _Web = webService;
            _SettingsLauncher = settingsLauncher;

            _LoginProxy = (ILoginProxy)_Pages.GetPage(PageType.Login);
            _MainProxy = (IMainProxy)_Pages.GetPage(PageType.Main);
            _SettingsProxy = (ISettingsProxy)_Pages.GetPage(PageType.Settings);
            _ConsoleProxy = (IConsoleProxy)_Pages.GetPage(PageType.Console);


            _LoginProxy.LoginClick += LoginProxy_LoginClick;
            _LoginProxy.SetParams(_SettingsLauncher.LoadLoginParams());


            _MainProxy.SettingsClick += MainProxy_SettingsClick;

            _SettingsProxy.SettingsApplyClick += SettingsProxy_SettingsApplyClick;
            _SettingsProxy.SetParams(_SettingsLauncher.LoadSettingsParams());

            CalculateMaxRam();

            _Main.Show();
            ShowPage(PageType.Login);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        public void ShowPage(PageType id)
        {
            _Main.ShowPage(_Pages.GetPage(id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void ShowOverlay(PageType id)
        {
            _Main.ShowOverlay(_Pages.GetPage(id));
        }


        /// <summary>
        /// 
        /// </summary>
        private void CalculateMaxRam()
        {
            var total = 0l;
            GetPhysicallyInstalledSystemMemory(out total);
            _SettingsProxy.SetMaxRam(total);
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

            if (_SettingsLauncher.SaveLoginParams(new ParamsLoginPage(username, String.Empty, false)))
            {
                if (_Web.Auth(username, password, out var profile, ref message))
                {
                    _MainProxy.SetProfile(profile);
                    ShowPage(PageType.Main);
                    LoadProfiles();
                    return true;
                }
                return false;
            }
            else
            {
                return false;
            }      
        }

        /// <summary>
        /// 
        /// </summary>
        private void LoadProfiles()
        {
            var profiles = _Web.GetServerProfiles();
            ServerProfiles = profiles;
            _MainProxy.SetServerProfiles(ServerProfiles);
        }

        private void MainProxy_SettingsClick()
        {
            //Нужно сбросить до тех параметров которые у нас сохранены.
            _SettingsProxy.SetParams(new ParamsSettingPage(_SettingsLauncher.UseRam, _SettingsLauncher.AutoConnect, _SettingsLauncher.FullScreen, _SettingsLauncher.Debug, _SettingsLauncher.RootDir));
            ShowOverlay(PageType.Settings);
        }

        private void SettingsProxy_SettingsApplyClick(IParamsSettingPage paramsSetting)
        {
            if (_SettingsLauncher.SaveSettingsParams(paramsSetting))
            {
                _Main.HideOverlay();
            }
        }
    }
}
