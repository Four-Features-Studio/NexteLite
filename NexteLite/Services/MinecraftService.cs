using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using NexteLite.Interfaces;
using NexteLite.Models;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace NexteLite.Services
{
    public class MinecraftService : IMinecraftService
    {
        public event OnMinecraftStateChangedHandler OnMinecraftStateChanged;
        public event OnMinecraftLogRecivedHandler OnMinecraftLogRecived;

        IOptions<AppSettings> _Options;
        ISettingsLauncher _SettingsLauncher;
        IPathRepository _Path;
        IAccountService _Account;

        IMessageService _Messages;
        ILogger<MinecraftService> _Logger;

        ProcessJob _KillerProcessJob;
        Process _Process;
        public MinecraftService (ISettingsLauncher settingsLauncher,
            IAccountService accountService, 
            IPathRepository pathRepository, 
            IOptions<AppSettings> options,
            IMessageService messages,
            ILogger<MinecraftService> logger)
        {
            _SettingsLauncher = settingsLauncher;
            _Options = options;
            _Path = pathRepository;
            _Account = accountService;

            _Messages = messages;
            _Logger = logger;
        }

        public async Task Play(ServerProfile profile)
        {
            await StartMinecraft(profile);
        }

        public void Kill()
        {
            if (_Process != null)
            {
                _Process.Kill();
                _Process.Dispose();
                OnMinecraftStateChanged?.Invoke(Enums.MinecraftState.Closed);
                _Process = null;
            }

        }

        private async Task StartMinecraft(ServerProfile profile)
        {      
            var clientDir = _Path.GetClientPath(profile);    

            if (!Directory.Exists(clientDir))
            {
                _Logger.LogError("Папка с клиентом не существует, запуск не возможен");
                _Messages.SendInfo("Папка с клиентом не существует, запуск не возможен");
                OnMinecraftStateChanged?.Invoke(Enums.MinecraftState.Closed);
                return;
            }

            var java = _Path.GetJavaPath();
            var args = GetArgs(profile);

            if (string.IsNullOrEmpty(args) || string.IsNullOrEmpty(clientDir) || string.IsNullOrEmpty(java))
            {
                _Logger.LogError($"Невозможно запустить майнкрафт, один из параметров пустой. Args {args}; ClientDir {clientDir}; Java {java}");
                OnMinecraftStateChanged?.Invoke(Enums.MinecraftState.Closed);
                return;
            }

            _KillerProcessJob = new ProcessJob();

            ProcessStartInfo info = new ProcessStartInfo();

            info.FileName = java;
            info.WorkingDirectory = clientDir;
            info.UserName = null;
            info.UseShellExecute = false;
            info.Arguments = args;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            _Process = new Process();
            _Process.StartInfo = info;
            _Process.EnableRaisingEvents = true;
            _Process.OutputDataReceived += (sender, args) =>
            {
                OnMinecraftLogRecived?.Invoke(args.Data.ToString());
            };
            _Process.ErrorDataReceived += (s, args) =>
            {
                OnMinecraftLogRecived?.Invoke(args.Data.ToString());
            };
            _Process.Exited += (s, arg) =>
            {
                OnMinecraftStateChanged?.Invoke(Enums.MinecraftState.Closed);
                Console.WriteLine($"Exit time: {_Process.ExitTime}\n" + $"Exit code: {_Process.ExitCode}\n");
            };
            _Process.Start();

            _KillerProcessJob.AddProcess(_Process.Handle);

            _Process.BeginOutputReadLine();
            _Process.BeginErrorReadLine();

            OnMinecraftStateChanged?.Invoke(Enums.MinecraftState.Running);
        }

        private string GetArgs(ServerProfile profile)
        {
            var clientDir = _Path.GetClientPath(profile);
            var nativesDir = _Path.GetNativesPath(profile);
            var librariesDir = _Path.GetLibrariesPath(profile);

            if(string.IsNullOrEmpty(clientDir) || string.IsNullOrEmpty(nativesDir) || string.IsNullOrEmpty(librariesDir))
                return string.Empty;

            var AssetsDir = _Path.GetAssetsPath();
            var AssetsIndex = profile.AssetIndex;
            var versionClient = profile.Version;

            var accessToken = _Account.GetAccessToken();
            var uuid = _Account.GetUuid();
            var username = _Account.GetUsername();

            var memory = _SettingsLauncher.UseRam == 0 ? 1024 : _SettingsLauncher.UseRam;

            var userType = "mojang";
            var versionType = "NexteLauncher";

            var injectorPath = _Path.GetInjectorPath();

            var injectorUrl = _Options.Value.InjectorUrl;
            if (string.IsNullOrEmpty(injectorUrl))
                throw new ArgumentNullException("The configuration file does not contain the url to api injector");



            var libraries = Directory.GetFiles(librariesDir, "*.*", SearchOption.AllDirectories);
            var minecraft = _Path.GetMinecraftPath(profile);

            //TODO - не забывать вернуть этот аргумент
            //
            var jvmArgs = new List<string>()
            { 
                $"-javaagent:{injectorPath}={injectorUrl}",
                $"-Xmx{memory}M",
                $"-Xms{memory}M",
                $"-Djava.library.path={nativesDir}"
            };

            var classPath = new List<string>()
            {
                "-cp",
                $"{String.Join(";", libraries.Append(minecraft).ToArray())}"
            };

            var gameArgs = new List<string>()
            {
                "--username",
                $"{username}",
                "--version",
                $"{versionClient}",
                "--gameDir",
                $"{clientDir}",
                "--assetsDir",
                $"{AssetsDir}",
                "--assetIndex",
                $"{AssetsIndex}",
                "--uuid",
                $"{uuid}",
                "--accessToken",
                $"{accessToken}",
                "--userProperties",
                "{}",
                "--userType",
                $"{userType}",
                "--versionType",
                $"{versionType}"
            };

            if (_SettingsLauncher.FullScreen)
            {
                gameArgs.Add("--fullscreen");
            }

            if (_SettingsLauncher.AutoConnect && profile.Server is not null && !string.IsNullOrEmpty(profile.Server.Ip))
            {
                gameArgs.Add("--server");
                gameArgs.Add(profile.Server.Ip);

                if (profile.Server.Port != 0)
                {
                    gameArgs.Add("--port");
                    gameArgs.Add(profile.Server.Port.ToString());
                }
                else
                {
                    gameArgs.Add("--port");
                    gameArgs.Add("25565");
                }
            }

            var jwmString = String.Join(" ", jvmArgs.ToArray());
            var cpString = String.Join(" ", classPath.ToArray());
            var gameString = String.Join(" ", gameArgs.ToArray());

            return $"{jwmString} {cpString} {profile.MainClass} {gameString}";
        }

        private void GetJvm()
        {

        }


    }
}
