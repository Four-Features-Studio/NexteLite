using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace NexteLite.Services
{
    public class FileService : IFileService
    {
        public event OnProgressChangedHandler OnProgressChanged;

        IWebService _WebService;
        ISettingsLauncher _SettingsLauncher;
        IOptions<AppSettings> _Options;
        public FileService(IWebService webService, ISettingsLauncher settingsLauncher, IOptions<AppSettings> options)
        {
            _WebService = webService;
            _SettingsLauncher = settingsLauncher;
            _Options = options;
        }

        public async Task CheckFilesClient()
        {

        }
        public async Task CheckAssets()
        {
        }
        public async Task CheckAndCreateInjector()
        {
            var path = GetPath();
            var fullpath = Path.Combine(path, "injector.jar");
            var injector = Properties.Resources.authlib_injector_1_2_1;

            if (File.Exists(fullpath))
            {
                var sumCorrect = GetHashsum(injector);
                var sumChecked = string.Empty;
                using (FileStream fstream = new FileStream(fullpath, FileMode.Open))
                {
                    byte[] buffer = new byte[fstream.Length];
                    await fstream.ReadAsync(buffer, 0, buffer.Length);

                    sumChecked = GetHashsum(buffer);
                }

                if (sumChecked == sumChecked)
                    return;
            }


            using (FileStream fstream = new FileStream(fullpath, FileMode.Create))
            {
                await fstream.WriteAsync(injector, 0, injector.Length);
            }
        }

        public async Task DownloadClient(FilesEntity files, ServerProfile profile)
        {
            //подготовливаем файлы
            if (profile.Dir.StartsWith('/') || profile.Dir.StartsWith('\\'))
                profile.Dir.TrimStart('/', '\\');

            var ClientDir = Path.Combine(_SettingsLauncher.RootDir, profile.Dir);

            var totalSize = 0l;
            var filesToDownload = new List<(string url, string path)>();


            foreach (var item in files.Files)
            {
                totalSize += item.Size;
                filesToDownload.Add((item.Url, item.Path));
            }

            if (string.IsNullOrEmpty(ClientDir))
            {
                await DownloadFiles(ClientDir, totalSize, filesToDownload);
            }
        }

        public async Task DownloadAssets(AssetsIndex assetsIndex, string version)
        {
            if (string.IsNullOrEmpty(_Options.Value.AssetsUrl))
            {
                throw new ArgumentNullException("In the launcher settings there is no link to download the assets");
            }

            var pathAssets = Path.Combine(_SettingsLauncher.RootDir, "Assets");
            var pathIndexes = Path.Combine(pathAssets, "indexes");
            var pathObjects = Path.Combine(pathAssets, "objects");

            if (!Directory.Exists(pathAssets))
            {
                Directory.CreateDirectory(pathAssets);
            }

            var pathIndex = Path.Combine(pathIndexes, version + ".json");

            new FileInfo(pathIndex).Directory?.Create();

            using (FileStream sourceStream = new FileStream(pathIndex,
               FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None,
               bufferSize: 4096))
            {
                using (StreamWriter file = new StreamWriter(sourceStream))
                {
                    await file.FlushAsync();
                    var data = JsonConvert.SerializeObject(assetsIndex, Formatting.Indented);
                    await file.WriteLineAsync(data);
                }
            };

            var assetsHash = assetsIndex.Objects.Values;
            List<(string url, string path)> files = new List<(string url, string path)>();
            var totalSize = 0l;


            foreach (var asset in assetsHash)
            {
                var twoSymbol = asset.Hash.Substring(0, 2);

                var path = Path.Combine(twoSymbol, asset.Hash);

                var url = $"{_Options.Value.AssetsUrl}{twoSymbol}/{asset.Hash}";

                totalSize += asset.Size;

                files.Add((url, path));
            }

            await DownloadFiles(pathObjects, totalSize, files);
        }

        public async Task RemoveAllClients()
        {

        }

        public async Task RemoveClient(ServerProfile profile)
        {

        }

        async Task DownloadFiles(string folder, long totalSize, List<(string url, string path)> files)
        {
            var progress = new Progress<DownloadProgressArguments>(ReportProgress);

            bool isDownloadEnd = false;

            var queue = new Queue<(MemoryStream data, string path, string name)>();

            var save = Task.Run(async () =>
            {
                do
                {
                    if (queue.Count > 0)
                    {
                        var data = queue.Dequeue();
                        await SaveFile(data.data, data.path);
                    }
                    else
                    {
                        await Task.Delay(200);
                    }
                }
                while (!isDownloadEnd || queue.Count != 0);
            });

            foreach (var file in files)
            {
                var path = Path.Combine(folder, file.path);
                var data = await _WebService.Download(totalSize, file.url, file.path, progress);
                queue.Enqueue((data, path, file.path));
            }

            isDownloadEnd = true;

            await save;
        }
        void ReportProgress(DownloadProgressArguments args)
        {
            OnProgressChanged?.Invoke(args);
        }

        static async Task SaveFile(MemoryStream data, string path)
        {
            new FileInfo(path).Directory?.Create();

            var buffer = data.ToArray();

            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            await fileStream.WriteAsync(buffer, 0, buffer.Length);
        }
        private string GetPath()
        {
            var folder = _Options.Value.DirParams.SettingsPath;
            if (string.IsNullOrEmpty(folder))
                throw new ArgumentNullException("The configuration file does not contain the path to the settings folder");

            var appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            if (folder.StartsWith('/'))
                folder = folder.TrimStart();

            return Path.Combine(appdata, folder);
        }

        public string GetHashsum(byte[] file)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                var result = sha1.ComputeHash(file);
                return string.Concat(result.Select(b => b.ToString("x2")));
            }
        }

    }
}
