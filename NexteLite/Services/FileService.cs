using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Services.Enums;
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
using System.Windows.Forms;

namespace NexteLite.Services
{
    public class FileService : IFileService
    {
        public event OnProgressChangedHandler OnProgressChanged;

        IWebService _WebService;
        ISettingsLauncher _SettingsLauncher;
        IOptions<AppSettings> _Options;
        IPathRepository _Path;

        public FileService(IWebService webService, IPathRepository pathRepository, ISettingsLauncher settingsLauncher, IOptions<AppSettings> options)
        {
            _WebService = webService;
            _SettingsLauncher = settingsLauncher;
            _Options = options;
            _Path = pathRepository;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> CheckFilesClient(ServerProfile profile, FilesEntity webFiles)
        {
            var pathClient = _Path.GetClientPath(profile);

            var updates = String.Join(";", profile.UpdatesDir);

            var filesRaw = Directory.GetFiles(pathClient, "*", SearchOption.AllDirectories);

            var files = new List<string>();

            foreach(var item in profile.UpdatesDir)
            {
                files.AddRange(filesRaw.Where(x => x.Contains(item)));
            }

            files = files.Distinct().ToList();

            var typeHash = (ChecksumMethod)webFiles.TypeHash;

            var hashes = new Dictionary<string,string>();

            foreach(var item in files)
            {
                var hash = await GetHashsumAsync(typeHash, item);
                hashes.Add(hash,item);
            }

            await Task.Delay(10000);

            return new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> CheckAssets(ServerProfile profile)
        {
            return new List<string>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task CheckAndCreateInjector()
        {
            var path = _Path.GetInjectorPath();
            var injector = Properties.Resources.authlib_injector_1_2_1;

            if (File.Exists(path))
            {
                var sumCorrect = GetHashsum(ChecksumMethod.SHA1,injector);
                var sumChecked = string.Empty;
                using (FileStream fstream = new FileStream(path, FileMode.Open))
                {
                    byte[] buffer = new byte[fstream.Length];
                    await fstream.ReadAsync(buffer, 0, buffer.Length);

                    sumChecked = GetHashsum(ChecksumMethod.SHA1, buffer);
                }

                if (sumChecked == sumChecked)
                    return;
            }


            using (FileStream fstream = new FileStream(path, FileMode.Create))
            {
                await fstream.WriteAsync(injector, 0, injector.Length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="profile"></param>
        /// <returns></returns>
        public async Task DownloadClient(FilesEntity files, ServerProfile profile)
        {

            var ClientDir = _Path.GetClientPath(profile);

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetsIndex"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task DownloadAssets(AssetsIndex assetsIndex, string version)
        {
            if (string.IsNullOrEmpty(_Options.Value.AssetsUrl))
            {
                throw new ArgumentNullException("In the launcher settings there is no link to download the assets");
            }

            var pathAssets = _Path.GetAssetsPath();
            var pathIndexes = _Path.GetAssetsIndexesPath();
            var pathObjects = _Path.GetAssetsObjectsPath();

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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task RemoveAllClients()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public async Task RemoveClient(ServerProfile profile)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="totalSize"></param>
        /// <param name="files"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        void ReportProgress(DownloadProgressArguments args)
        {
            OnProgressChanged?.Invoke(args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        static async Task SaveFile(MemoryStream data, string path)
        {
            new FileInfo(path).Directory?.Create();

            var buffer = data.ToArray();

            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            await fileStream.WriteAsync(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string GetHashsum(ChecksumMethod method, byte[] file)
        {
            switch (method)
            {
                case ChecksumMethod.MD5:
                    {
                        using (MD5 md5 = MD5.Create())
                        {
                            var result = md5.ComputeHash(file);
                            return string.Concat(result.Select(b => b.ToString("x2")));
                        }
                    }
                    break;
                case ChecksumMethod.SHA1:
                    {
                        using (SHA1 sha1 = SHA1.Create())
                        {
                            var result = sha1.ComputeHash(file);
                            return string.Concat(result.Select(b => b.ToString("x2")));
                        }
                    }
                    break;
            }

            throw new ArgumentException("Указанный метод хеширования указан не верно.");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<string> GetHashsumAsync(ChecksumMethod method, string file)
        {
            switch (method)
            {
                case ChecksumMethod.MD5:
                    {
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead;
                                do 
                                {
                                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                                    if(bytesRead > 0)
                                    {
                                        md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                                    }
                                } while(bytesRead > 0);

                                md5.TransformFinalBlock(buffer, 0, 0);
                                return BitConverter.ToString(md5.Hash).Replace("-", "").ToUpperInvariant();
                            }
                            //var result = md5.ComputeHash(file);
                            //return string.Concat(result.Select(b => b.ToString("x2")));
                        }
                    }
                    break;
                case ChecksumMethod.SHA1:
                    {
                        using (SHA1 sha1 = SHA1.Create())
                        {
                            using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true))
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead;
                                do
                                {
                                    bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                                    if (bytesRead > 0)
                                    {
                                        sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                                    }
                                } while (bytesRead > 0);

                                sha1.TransformFinalBlock(buffer, 0, 0);
                                return BitConverter.ToString(sha1.Hash).Replace("-", "").ToUpperInvariant();
                            }
                            //var result = sha1.ComputeHash(file);
                            //return string.Concat(result.Select(b => b.ToString("x2")));
                        }
                    }
                    break;
            }

            throw new ArgumentException("Указанный метод хеширования указан не верно.");

        }
    }
}
