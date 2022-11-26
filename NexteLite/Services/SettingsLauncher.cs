using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;

namespace NexteLite.Services
{
    public class SettingsLauncher : ISettingsLauncher
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool Save { get; set; }
        public string RootDir { get; set; }
        public int UseRam { get; set; }
        public bool AutoConnect { get; set; }
        public bool Debug { get; set; }
        public bool FullScreen { get; set; }

        public Dictionary<string,string> SelectedPresets { get; set; }

        AppSettings _AppSettings;

        public SettingsLauncher(IOptions<AppSettings> options)
        {
            _AppSettings = options.Value;

            UseRam = _AppSettings.DefaultRam;
            RootDir = GetPathRoot();
        }

        public IParamsLoginPage LoadLoginParams()
        {
            var path = GetPath();
            var fullpath = Path.Combine(path, _AppSettings.DirParams.SettingsLoginName + _AppSettings.DirParams.SettingsExtension);

            ParamsLoginPage result = new ParamsLoginPage(String.Empty, String.Empty, false);

            if (!File.Exists(fullpath))
                return result;

            using(BinaryReader reader = new BinaryReader(File.Open(fullpath, FileMode.Open)))
            {
                var login = reader.ReadString();
                var save = reader.ReadBoolean();

                result = new ParamsLoginPage(login, "", save);
            }

            Username = result.Username;
            Save = result.SavePassword;

            return result;
        }

        public IParamsSettingPage LoadSettingsParams()
        {
            var path = GetPath();
            var fullpath = Path.Combine(path, _AppSettings.DirParams.SettingsName + _AppSettings.DirParams.SettingsExtension);

            ParamsSettingPage result = new ParamsSettingPage(_AppSettings.DefaultRam, false, false, false, GetPathRoot());

            if (!File.Exists(fullpath))
                return result;

            using (BinaryReader reader = new BinaryReader(File.Open(fullpath, FileMode.Open)))
            {
                var ram = reader.ReadInt32();
                var autoConnect = reader.ReadBoolean();
                var fullScreen = reader.ReadBoolean();
                var debug = reader.ReadBoolean();
                var root = reader.ReadString();

                result = new ParamsSettingPage(ram, autoConnect, fullScreen, debug, root);
            }

            UseRam = result.CurrentRam;
            RootDir = result.Path;

            AutoConnect = result.AutoConnectMode;
            FullScreen = result.FullScreenMode;
            Debug = result.DebugMode;

            return result;
        }

        public bool SaveLoginParams(IParamsLoginPage data)
        {
            try
            {
                var path = GetPath();
                var fullpath = Path.Combine(path, _AppSettings.DirParams.SettingsLoginName + _AppSettings.DirParams.SettingsExtension);

                new FileInfo(fullpath).Directory?.Create();

                using (BinaryWriter writer = new BinaryWriter(File.Open(fullpath, FileMode.Create)))
                {
                    writer.Flush();
                    writer.Write(data.Username);
                    writer.Write(data.SavePassword);
                }

                Username = data.Username;
                Save = data.SavePassword;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        public bool SaveSettingsParams(IParamsSettingPage data)
        {
            try
            {
               var PathRoot = string.IsNullOrEmpty(data.Path) ? GetPathRoot() : data.Path;

                var path = GetPath();
                var fullpath = Path.Combine(path, _AppSettings.DirParams.SettingsName + _AppSettings.DirParams.SettingsExtension);

                new FileInfo(fullpath).Directory?.Create();

                using (BinaryWriter writer = new BinaryWriter(File.Open(fullpath, FileMode.Create)))
                {
                    writer.Flush();
                    writer.Write(data.CurrentRam);
                    writer.Write(data.AutoConnectMode);
                    writer.Write(data.FullScreenMode);
                    writer.Write(data.DebugMode);
                    writer.Write(PathRoot);
                }

                UseRam = data.CurrentRam;
                RootDir = PathRoot;

                AutoConnect = data.AutoConnectMode;
                FullScreen = data.FullScreenMode;
                Debug = data.DebugMode;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool SaveLastSelectedPreset(string profileId, string presetId)
        {
            try
            {
                if (SelectedPresets.ContainsKey(profileId))
                {
                    SelectedPresets[profileId] = presetId;
                }
                else
                {
                    SelectedPresets.Add(profileId, presetId);
                }

                var path = GetPath();
                var fullpath = Path.Combine(path, "presets" + _AppSettings.DirParams.SettingsExtension);

                new FileInfo(fullpath).Directory?.Create();

                using (BinaryWriter writer = new BinaryWriter(File.Open(fullpath, FileMode.Create)))
                {
                    writer.Flush();
                    var data = JsonConvert.SerializeObject(path, Formatting.Indented);
                    writer.Write(data);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public void LoadLastSelectedPreset()
        {
            SelectedPresets = new Dictionary<string, string>();

            var path = GetPath();
            var fullpath = Path.Combine(path, "presets" + _AppSettings.DirParams.SettingsExtension);

            if (!File.Exists(fullpath))
                return;

            using (BinaryReader reader = new BinaryReader(File.Open(fullpath, FileMode.Open)))
            {
                var data = reader.ReadString();
                SelectedPresets = JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
            }

            return;
        }

        private string GetPath()
        {
            var folder = _AppSettings.DirParams.SettingsPath;
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentNullException("The configuration file does not contain the path to the settings folder");

            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (folder.StartsWith('/'))
                folder = folder.TrimStart();

            return Path.Combine(appdata, folder);
        }

        private string GetPathRoot()
        {
            var root = _AppSettings.DefaultPath;
            if (string.IsNullOrEmpty(root))
                throw new ArgumentNullException("In the configuration there is no default path to the working folder of the Launcher.");

            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (root.StartsWith('/'))
                root = root.TrimStart();

            return Path.Combine(appdata, root);
        }
    }
}
