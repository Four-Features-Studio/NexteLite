using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NexteLite.Services
{
    public class PathRepository : IPathRepository
    {
        private const string NameDirectoryNatives = "natives";
        private const string NameDirectoryLibraries = "libraries";
        private const string NameDirectoryClients = "Updates";
        private const string NameDirectoryAssets = "Assets";
        private const string NameDirectoryAssetsObjects = "objects";
        private const string NameDirectoryAssetsIndexes = "indexes";

        IOptions<AppSettings> _Options;
        ISettingsLauncher _SettingsLauncher;
        ILogger<PathRepository> _Logger;
        public PathRepository(IOptions<AppSettings> options, ISettingsLauncher settingsLauncher, ILogger<PathRepository> logger)
        {
            _Options = options;
            _SettingsLauncher = settingsLauncher;
            _Logger = logger;
        }

        public string GetLocalLauncher()
        {
            return System.Environment.ProcessPath;
        }

        public string GetAppDataPath()
        {
            var folder = _Options.Value.DirParams.SettingsPath;
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentNullException("The configuration file does not contain the path to the settings folder");

            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (folder.StartsWith('/'))
                folder = folder.TrimStart();

            return Path.Combine(appdata, folder);
        }

        public string GetProjectPath()
        {        
            var root = _SettingsLauncher.RootDir;
            return root;
        }


        public string GetAssetsIndexesPath()
        {
            var path = Path.Combine(GetAssetsPath(), NameDirectoryAssetsIndexes);
            return path;
        }

        public string GetAssetsObjectsPath()
        {
            var path = Path.Combine(GetAssetsPath(), NameDirectoryAssetsObjects);
            return path;
        }

        public string GetAssetsPath()
        {
            var path = Path.Combine(_SettingsLauncher.RootDir, NameDirectoryAssets);
            return path;
        }
        public string GetClientsPath()
        {
            var path = Path.Combine(_SettingsLauncher.RootDir, NameDirectoryClients);
            return path;
        }

        public string GetClientPath(ServerProfile profile)
        {
            var folder = NameDirectoryClients;

            var clientDir = profile.Dir;

            if (string.IsNullOrEmpty(clientDir))
            {
                _Logger.LogError("В данный профиль не полный, не возможно получить путь");
                return String.Empty;
            }

            var root = _SettingsLauncher.RootDir;

            if (folder.StartsWith('/'))
                folder = folder.TrimStart();

            return Path.Combine(root, folder, clientDir);
        }

        public string GetInjectorPath()
        {
            var path = GetAppDataPath();
            var fullpath = Path.Combine(path, "injector.jar");
            return fullpath;
        }

        public string GetUpdateAgentPath()
        {
            var path = GetAppDataPath();
            var fullpath = Path.Combine(path, "agent.exe");
            return fullpath;
        }

        public string GetMinecraftPath(ServerProfile profile)
        {
            var path = GetClientPath(profile);
            var fullpath = Path.Combine(path, "minecraft.jar");
            return fullpath;
        }

        public string GetLibrariesPath(ServerProfile profile)
        {
            var clientPath = GetClientPath(profile);
            var path = Path.Combine(clientPath, NameDirectoryLibraries);
            return path;
        }

        public string GetNativesPath(ServerProfile profile)
        {
            var clientPath = GetClientPath(profile);
            var path = Path.Combine(clientPath, NameDirectoryNatives);
            return path;
        }

        public string GetJavaPath()
        {
            var installPath = GetJavaInstallationPath();
            string path = Path.Combine(installPath, "bin\\javaw.exe");
            return path;
        }

        private static string GetJavaInstallationPath()
        {
            try
            {
                string javaKey = "SOFTWARE\\JavaSoft\\Java Runtime Environment";
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(javaKey))
                {
                    string currentVersion = baseKey.GetValue("CurrentVersion").ToString();
                    using (var homeKey = baseKey.OpenSubKey(currentVersion))
                        return homeKey.GetValue("JavaHome").ToString();
                }
            }
            catch (Exception ex)
            {
                string javaKey = "SOFTWARE\\WOW6432Node\\JavaSoft\\Java Runtime Environment";
                using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(javaKey))
                {
                    string currentVersion = baseKey.GetValue("CurrentVersion").ToString();
                    using (var homeKey = baseKey.OpenSubKey(currentVersion))
                        return homeKey.GetValue("JavaHome").ToString();
                }
            }
        }
    }
}
