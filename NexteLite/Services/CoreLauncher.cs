using Microsoft.Extensions.Options;
using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Pages;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static System.Windows.Forms.Design.AxImporter;

namespace NexteLite.Services
{
    public class CoreLauncher : ICoreLaucnher
    {
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        Action<string> StartingUriProccess;

        IMainWindow _Main;
        IPagesRepository _Pages;
        IWebService _Web;

        ILoginProxy _LoginProxy;
        IMainProxy _MainProxy;
        ISettingsProxy _SettingsProxy;
        IConsoleProxy _ConsoleProxy;
        ISettingsLauncher _SettingsLauncher;

        IOptions<AppSettings> _Options;
        public CoreLauncher(IMainWindow mainWindow, IPagesRepository pagesRepository, IWebService webService, ISettingsLauncher settingsLauncher, IOptions<AppSettings> options)
        {
            _Options = options;
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
            _MainProxy.SocialClick += MainProxy_SocialClick;
            _MainProxy.PlayClick += MainProxy_PlayClick;

            _SettingsProxy.SettingsApplyClick += SettingsProxy_SettingsApplyClick;
            _SettingsProxy.SetParams(_SettingsLauncher.LoadSettingsParams());

            CalculateMaxRam();

            CreateSocialBlock();

            StartingUriProccess += ((e) =>
            {
                _Main.Minimized();
            });

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
        /// <param name="url"></param>
        public void HyperLinkOpen(string url)
        {
            var dlg = new Action(() =>
            {
                StartingUriProccess?.Invoke(url);
                System.Diagnostics.Process.Start("explorer.exe", url);
            });
            try
            { dlg.Invoke(); }
            catch (Exception ex)
            { }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateSocialBlock()
        {
            var socials = _Options.Value.Social;
            if (socials == null)
                return;

            foreach(var social in socials)
            {
                _MainProxy.AddSocial(social);
            }
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

        /// <summary>
        /// 
        /// </summary>
        private void MainProxy_SettingsClick()
        {
            //Нужно сбросить до тех параметров которые у нас сохранены.
            _SettingsProxy.SetParams(new ParamsSettingPage(_SettingsLauncher.UseRam, _SettingsLauncher.AutoConnect, _SettingsLauncher.FullScreen, _SettingsLauncher.Debug, _SettingsLauncher.RootDir));
            ShowOverlay(PageType.Settings);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        private void MainProxy_SocialClick(string url)
        {
            HyperLinkOpen(url);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MainProxy_PlayClick(string id)
        {
            Console.WriteLine(id);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramsSetting"></param>
        private void SettingsProxy_SettingsApplyClick(IParamsSettingPage paramsSetting)
        {
            if (_SettingsLauncher.SaveSettingsParams(paramsSetting))
            {
                _Main.HideOverlay();
            }
        }
    }
}
