using Microsoft.Extensions.Options;
using Microsoft.Win32;
using NexteLite.Interfaces;
using NexteLite.Models;
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

        IOptions<AppSettings> _Options;
        ISettingsLauncher _SettingsLauncher;
        IPathRepository _Path;
        IAccountService _Account;

        public MinecraftService (ISettingsLauncher settingsLauncher, IAccountService accountService, IPathRepository pathRepository, IOptions<AppSettings> options)
        {
            _SettingsLauncher = settingsLauncher;
            _Options = options;
            _Path = pathRepository;
            _Account = accountService;
        }

        public async Task Play(ServerProfile profile)
        {
            await StartMinecraft(profile);
        }

        private async Task StartMinecraft(ServerProfile profile)
        {
            var args = GetArgs(profile);
            var clientDir = _Path.GetClientPath(profile);

            var java = _Path.GetJavaPath();
            var killerProcessJob = new ProcessJob();

            ProcessStartInfo info = new ProcessStartInfo();
            info.FileName = java;
            info.WorkingDirectory = clientDir;
            info.UserName = null;
            info.UseShellExecute = false;
            info.Arguments = args;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;

            var process = new Process();
            process.StartInfo = info;
            process.EnableRaisingEvents = true;
            process.OutputDataReceived += (sender, args) =>
            {

            };
            process.ErrorDataReceived += (s, args) =>
            {

            };
            process.Exited += (s, arg) =>
            {
                Console.WriteLine($"Exit time: {process.ExitTime}\n" + $"Exit code: {process.ExitCode}\n");
            };
            process.Start();

            killerProcessJob.AddProcess(process.Handle);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync().ConfigureAwait(true);
        }

        private string GetArgs(ServerProfile profile)
        {
            var clientDir = _Path.GetClientPath(profile);
            var nativesDir = _Path.GetNativesPath(profile);
            var librariesDir = _Path.GetLibrariesPath(profile);

            var AssetsDir = _Path.GetAssetsPath();
            var AssetsIndex = profile.AssetIndex;
            var versionClient = profile.Version;

            var accessToken = _Account.GetAccessToken();
            var uuid = _Account.GetUuid();
            var username = _Account.GetUsername();

            var userType = "mojang";
            var versionType = "NexteLauncher";

            var injectorPath = _Path.GetInjectorPath();

            var injectorUrl = _Options.Value.InjectorUrl;
            if (string.IsNullOrEmpty(injectorUrl))
                throw new ArgumentNullException("The configuration file does not contain the url to api injector");

            var libraries = Directory.GetFiles(librariesDir, "*.*", SearchOption.AllDirectories);
            var minecraft = _Path.GetMinecraftPath(profile);

            //TODO - не забывать вернуть этот аргумент
            //$"-javaagent:{injectorPath}={injectorUrl}",
            var jvmArgs = new List<string>()
            {
                $"-Djava.library.path={nativesDir}",
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
