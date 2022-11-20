using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Pages;
using NexteLite.Services.Enums;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Windows.Forms.Design.AxImporter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

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
        IFileService _FileService;
        IAccountService _Account;
        IPathRepository _Path;

        ILoginProxy _LoginProxy;
        IMainProxy _MainProxy;
        ISettingsProxy _SettingsProxy;
        IConsoleProxy _ConsoleProxy;
        IDownloadingProxy _DownloadingProxy;
        IRunningProxy _RunningProxy;
        IUpdateProxy _UpdateProxy;

        ISettingsLauncher _SettingsLauncher;

        IMinecraftService _Minecraft;

        IOptions<AppSettings> _Options;

        ILogger<CoreLauncher> _Logger;

        private List<ServerProfile> ServerProfiles = new List<ServerProfile>();

        Profile _Profile { get; set; }

        public CoreLauncher(IMainWindow mainWindow, 
            IAccountService accountService,
            IFileService fileService,
            IPagesRepository pagesRepository,
            IWebService webService, 
            ISettingsLauncher settingsLauncher, 
            IMinecraftService minecraftService,
            IPathRepository pathRepository,
            IOptions<AppSettings> options,
            ILogger<CoreLauncher> logger)
        {
            _Account = accountService;
            _Options = options;
            _Main = mainWindow;
            _Pages = pagesRepository;
            _Web = webService;
            _SettingsLauncher = settingsLauncher;
            _FileService = fileService;
            _Logger = logger;
            _Path = pathRepository;


            _FileService.CheckAndCreateInjector();
            _FileService.CheckAndCreateUpdateAgent();

            _Minecraft = minecraftService;

            _Logger.LogInformation("Запрос страниц");
            _LoginProxy = (ILoginProxy)_Pages.GetPage(PageType.Login);
            _MainProxy = (IMainProxy)_Pages.GetPage(PageType.Main);
            _SettingsProxy = (ISettingsProxy)_Pages.GetPage(PageType.Settings);
            _ConsoleProxy = (IConsoleProxy)_Pages.GetPage(PageType.Console);
            _UpdateProxy = (IUpdateProxy)_Pages.GetPage(PageType.Update);

            _Minecraft.OnMinecraftStateChanged += Minecraft_OnMinecraftStateChanged;

            _LoginProxy.LoginClick += LoginProxy_LoginClick;
            _LoginProxy.SetParams(_SettingsLauncher.LoadLoginParams());


            _MainProxy.SettingsClick += MainProxy_SettingsClick;
            _MainProxy.SocialClick += MainProxy_SocialClick;
            _MainProxy.PlayClick += MainProxy_PlayClick;
            _MainProxy.LogoutClick += MainProxy_LogoutClick;

            _SettingsProxy.SettingsApplyClick += SettingsProxy_SettingsApplyClick;
            _SettingsProxy.DeleteAllClick += SettingsProxy_DeleteAllClick;
            _SettingsProxy.SetParams(_SettingsLauncher.LoadSettingsParams());

            _Logger.LogInformation("Расчет максимального количество ОЗУ");
            CalculateMaxRam();

            _Logger.LogInformation("Загрузка социального блока");
            CreateSocialBlock();

            StartingUriProccess += ((e) =>
            {
                _Main.Minimized();
            });

            _Main.Show();
           
            _Logger.LogDebug("Ядро инициализированно");

            CheckUpdate();

            _Logger.LogInformation("Проверка обновления");
        }

        private async void CheckUpdate()
        {
            ShowPage(PageType.Update);

            _UpdateProxy.SetState(UpdateState.Check);

            var hash = _FileService.GetHashsumLeuncher();
            var info = await _Web.CheckUpdates(hash);

            if(info is null)
            {
                _UpdateProxy.SetState(UpdateState.Error);
                return;
            }

            if (info.IsOutdated)
            {
                await UpdateLauncher(info.Size);
            }
            else
            {
                ShowPage(PageType.Login);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="id"></param>
        public void ShowPage(PageType id)
        {
            _Logger.LogDebug($"Запрос на отображение страницы {id.ToString()}");

            switch (id)
            {
                case PageType.Running:
                    _Main.ShowPage(CreateRunningPage());
                    return;
                case PageType.Downloading:
                    _Main.ShowPage(CreateDownloadingPage());
                    return;
            }
            _Main.ShowPage(_Pages.GetPage(id));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public void ShowOverlay(PageType id)
        {
            _Logger.LogDebug($"Запрос на отображение оверлея {id.ToString()}");
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

        private Page CreateRunningPage()
        {
            var page = _Pages.GetRunningPage();
            _RunningProxy = (IRunningProxy)page;
            _RunningProxy.OnKillClientClick += _RunningProxy_OnKillClientClick;
            _RunningProxy.SetParams(_SettingsLauncher.Debug);
            _Minecraft.OnMinecraftLogRecived += _RunningProxy.WriteLog;

            return page;
        }

        private Page CreateDownloadingPage()
        {
            var page = _Pages.GetDownloadingPage();
            _DownloadingProxy = (IDownloadingProxy)page;
            return page;
        }

        private async void StartMinecraft(ServerProfile profile)
        {
            _Logger.LogDebug($"Запрос на запуск игрового клиента {profile.Title}");
            ShowPage(PageType.Downloading);

            //Проверка файлов
            //Скачивание файлов
            //запуск майнкрафта
            _FileService.OnProgressChanged += _DownloadingProxy.OnDownloadingProgress;

            _DownloadingProxy.SetState(DownloadingState.HashAssets);
            var assetsIndex = await _Web.GetAssetsIndex(profile.AssetIndex);
            if (assetsIndex is null)
            {
                _Logger.LogError("Не возможно проверить ассеты клиента, запуск не возможен");
                ShowPage(PageType.Main);
                return;
            }

            var localAssets = await _FileService.CheckAssets(profile, assetsIndex);

            

            _DownloadingProxy.SetState(DownloadingState.HashClient);
            var files = await _Web.GetFiles(profile.Dir);
            if (files is null)
            {
                _Logger.LogError("Не возможно проверить файлы клиента, запуск не возможен");
                ShowPage(PageType.Main);
                return;
            }
                

            var localFiles = await _FileService.CheckFilesClient(profile, files);

            _DownloadingProxy.SetState(DownloadingState.DownloadAssets);
            await _FileService.DownloadAssets(assetsIndex, localAssets, profile.AssetIndex);

            _DownloadingProxy.SetState(DownloadingState.DownloadClient);
            await _FileService.DownloadClient(localFiles, profile);

            _Logger.LogDebug($"Запуск игрового клиента");
            await _Minecraft.Play(profile);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="save"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        /// 
        private async void LoginProxy_LoginClick(string username, string password, bool save)
        {
            _Logger.LogDebug($"Запрос авторизации - Login:{username}");

            var data = await _Account.AuthAccount(username, password);
            if (data.result)
            {
                _Logger.LogDebug($"Авторизация успешна");
                _Profile = data.profile;
                _MainProxy.SetProfile(_Profile);

                ShowPage(PageType.Main);
                LoadServerProfiles();
                return;
            }

            _Logger.LogDebug($"Ошибка авторизации:{data.message}");
            _LoginProxy.LoginError(data.message);
        }

        /// <summary>
        /// 
        /// </summary>
        private async void LoadServerProfiles()
        {
            var profiles = await _Web.GetServerProfiles();
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
            _Logger.LogDebug($"Запрос открытия ссылки социальной кнопки");
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
            var profile = ServerProfiles.FirstOrDefault(x => x.NID == id);
            if (profile == null)
                return;

            StartMinecraft(profile);

        }
        private void MainProxy_LogoutClick()
        {
            _Logger.LogDebug($"Попытка разлогиниться");
            _Web.Logout(_Profile.Uuid);

            _MainProxy.SetProfile(null);
            _SettingsLauncher.Username = String.Empty;
            _SettingsLauncher.Password = String.Empty;
            _SettingsLauncher.SaveLoginParams(new ParamsLoginPage(string.Empty, string.Empty, false));
            _LoginProxy.SetParams(_SettingsLauncher.LoadLoginParams());
            _Profile = null;

            ShowPage(PageType.Login);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paramsSetting"></param>
        private void SettingsProxy_SettingsApplyClick(IParamsSettingPage paramsSetting)
        {
            _Logger.LogDebug($"Запрос сохранения настроек");
            if (_SettingsLauncher.SaveSettingsParams(paramsSetting))
            {
                _Logger.LogDebug($"Настройки сохранены");
                _Main.HideOverlay();
            }
        }
        private void SettingsProxy_DeleteAllClick()
        {
            _Logger.LogDebug($"Запрос удаления всех клиентов");
            _FileService.RemoveAllClients();
        }


        /// <summary>
        /// 
        /// </summary>
        private void _RunningProxy_OnKillClientClick()
        {
            _Logger.LogDebug($"Запрос принудительного завершения процесса игрового клиента");
            _Minecraft.Kill();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void Minecraft_OnMinecraftStateChanged(MinecraftState state)
        {
            switch (state)
            {
                case MinecraftState.Running:
                    ShowPage(PageType.Running);
                    break;

                case MinecraftState.Closed:
                    //Возвращаем главную страницу
                    ShowPage(PageType.Main);
                    break;
            }

            _Logger.LogDebug($"Состояние клиента изменено на {state.ToString()}");
        }

        private async Task UpdateLauncher(double size)
        {
            var pathAppData = _Path.GetAppDataPath();
            var pathLauncher = _Path.GetLocalLauncher();
            var urlUpdate = _Options.Value.API.UpdateUrl;
            var agent = _Path.GetUpdateAgentPath();

            var pID = Process.GetCurrentProcess().Id;

            var args = ((App)App.Current).Args;

            ProcessStartInfo info = new ProcessStartInfo();

            info.FileName = agent;
            info.UserName = null;
            info.UseShellExecute = false;
            info.Arguments = $"-pID \"{pID}\" -appdata-path \"{pathAppData}\" -launcher-path \"{pathLauncher}\" -url \"{urlUpdate}\" -size \"{size}\" {args}";
            var process = new Process();
            process.StartInfo = info;
            process.EnableRaisingEvents = true;
            process.Start();

            App.Current.Shutdown();

        }
    }
}
