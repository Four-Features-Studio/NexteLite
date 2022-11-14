﻿using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
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

        //Ибо хочу чтоб был общий прогресс бар на весь объем, по этому я тут просто агригирую данные из эвента в эту переменную. Каждое новое скачивание, обнуляем ее.
        double _DownloadedBytes;

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
        public async Task<List<(ActionFile action, FileEntity file)>> CheckFilesClient(ServerProfile profile, FilesEntity webFiles)
        {
            //1 - Получить все локальные файлы.
            //2 - Отсеить файлы которые не входят в updatelist
            //3 - Получаем хеши локальных файлов
            //4 - Сверяем хеши локальные с веб хешами.
            //  | 1 - Если локального файла нет в веб файлах, то его нужно удалить
            //  | 2 - Если локального файла нет в веб файлах, но есть в игнор листе, то его не трогаем
            //  | 3 - Если локальный файл есть в веб файлах и хеши совпадают, то файл корректный
            //  | 4 - Если локальный файл есть в веб файлах и хеши не совпадают, то его нужно обновить
            //  | 5 - Если в веб файлах есть файл которого нет локально, то его нужно скачать.


            var typeHash = (ChecksumMethod)webFiles.TypeHash;
            var verifyFiles = webFiles.Files;

            var pathClient = _Path.GetClientPath(profile);
            //1
            var localFiles = Directory.GetFiles(pathClient, "*", SearchOption.AllDirectories);

            var hashesFiles = new List<FileEntity>();
            var fileIncorect = new List<(ActionFile action, FileEntity file)>();

            //2
            localFiles = localFiles.Where(x => profile.UpadtesList.Any(u => CheckNameInFileOrFolder(pathClient, x, u))).ToArray();

            //3
            foreach (var item in localFiles)
            {
                var hash = await GetHashsumAsync(typeHash, item);
                hashesFiles.Add(new FileEntity() { Name = Path.GetFileName(item), Path = item, Size = 0, Hash = hash });
            }

            //4 - 1,2
            foreach(var local in hashesFiles)
            {
                //2
                if (profile.IgnoreList.Any(x => CheckNameInFileOrFolder(pathClient, local.Path, x)))
                    continue;

                if(!verifyFiles.Any(x => x.Name == local.Name))
                    fileIncorect.Add((ActionFile.Delete, local));
            }

            //4 - 3,4,5
            foreach(var file in verifyFiles)
            {
                var local = hashesFiles.FirstOrDefault(x => x.Name == file.Name);

                //4.3
                if (local != null && file.Hash == local.Hash)
                    continue;

                //4.4
                else if (local != null && file.Hash != local.Hash)
                {
                    fileIncorect.Add((ActionFile.Update, file));
                }
                else if (local == null)
                {
                    fileIncorect.Add((ActionFile.Download, file));
                }
            }

            return fileIncorect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<(string hash, double size)>> CheckAssets(ServerProfile profile, AssetsIndex assets)
        {
            var indexesPath = _Path.GetAssetsIndexesPath();
            var objectsPath = _Path.GetAssetsObjectsPath();

            var localFiles = Directory.GetFiles(objectsPath, "*", SearchOption.AllDirectories);

            var assetsIncorect = new List<(string hash, double size)>();

            foreach(var asset in assets.Objects.Values)
            {
                var twoSymbol = asset.Hash.Substring(0, 2);
                var path = Path.Combine(twoSymbol, asset.Hash);

                var local = localFiles.FirstOrDefault(x => x.Contains(path));

                if(local != null)
                {
                    if(!File.Exists(local))
                        assetsIncorect.Add((asset.Hash, asset.Size));

                    var hash = await GetHashsumAsync(ChecksumMethod.SHA1,local);
                    if (asset.Hash == hash)
                        continue;
                    else
                        assetsIncorect.Add((asset.Hash, asset.Size));
                }
                else
                {
                    assetsIncorect.Add((asset.Hash, asset.Size));
                }

            }

            return assetsIncorect;
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
        public async Task DownloadClient(List<(ActionFile action, FileEntity file)> files, ServerProfile profile)
        {
            var RootDir = _SettingsLauncher.RootDir;
            var ClientDir = _Path.GetClientPath(profile);

            var totalSize = 0d;
            var filesToDownload = new List<(string url, string path)>();
            var filesToRemove = new List<string>();


            foreach (var item in files)
            {
                switch (item.action)
                {
                    case ActionFile.Download:
                    case ActionFile.Update:
                        {
                            totalSize += item.file.Size;
                            item.file.Url = CombineUrlClientFile(item.file.Path);

                            var path = item.file.Path;

                            if (path.StartsWith("/") || path.StartsWith("\\"))
                                path = item.file.Path.Substring(1);

                            item.file.Path = Path.Combine(RootDir, path);
                            filesToDownload.Add((item.file.Url, item.file.Path));
                        }
                        break;
                    case ActionFile.Delete:
                        {
                            filesToRemove.Add(item.file.Path);
                        }
                        break;
                }

            }

            if (!string.IsNullOrEmpty(ClientDir))
            {
                await DeleteFiles(filesToRemove);
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
        public async Task DownloadAssets(AssetsIndex assetsIndex, List<(string hash, double size)> assetsDownload, string version)
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

            List<(string url, string path)> files = new List<(string url, string path)>();
            var totalSize = 0d;

            foreach (var asset in assetsDownload)
            {
                var twoSymbol = asset.hash.Substring(0, 2);
                var path = Path.Combine(twoSymbol, asset.hash);
                var url =  $"{_Options.Value.AssetsUrl}{twoSymbol}/{asset.hash}";

                totalSize += asset.size;

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
        /// <param name="clientDir">Параметр указывающий название папки, а не путь</param>
        /// <param name="path"></param>
        /// <param name="pathPart"></param>
        /// <returns></returns>
        bool CheckNameInFileOrFolder(string clientDirPath, string path, string pathPart)
        {
            //1 - Приводим путь проверки к формату пути ОС
            //2 - Получаем название папки игрового клиента из пути.
            //3 - Создаем путь до условной папки/файла и проверяем, существует ли он, если нет то проверку завершаем возвращая false
            //4 - Проверяем условный путь, является ли это файлом или папкой, если папка то добавляем разделитель в конец
            //5 - Создаем паттерн пути для проверки
            //6 - Превращаем путь в относительный путь от папки Updates (прим. D://.nexte//Updates//HiTech//minecraft.jar -> HiTech//minecraft.jar)
            //  | 1 - Разбиваем путь на массив по разделителю
            //  | 2 - Ищем индекс элемента в массиве который является папкой с клиентом
            //  | 3 - Удаляем из пути все лишнее, чтоб получился относительный путь от папки Updates
            //7 - Проверяем, есть ли в пути наш паттерн

            char GetDirectorySeparatorUsedInPath()
            {
                if (path.Contains(Path.AltDirectorySeparatorChar))
                    return Path.AltDirectorySeparatorChar;

                return Path.DirectorySeparatorChar;
            }

            string NormalizePath(string path)
            {
                var pathParts = path.Split(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                return Path.Combine(pathParts);
            }

            //1 
            pathPart = NormalizePath(pathPart);

            //2
            var nameClientDir = Path.GetFileName(clientDirPath);

            //3
            var checkedPath = Path.Combine(clientDirPath, pathPart);
            if(!Directory.Exists(checkedPath))
                return false;

            //4
            FileAttributes attr = File.GetAttributes(checkedPath);
            if (attr.HasFlag(FileAttributes.Directory))
                pathPart = pathPart + GetDirectorySeparatorUsedInPath();

            //5
            var verifyPattern = Path.Combine(nameClientDir, pathPart);

            //6

            //6.1
            var pathParts = path.Split(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            //6.2
            var startIndex = pathParts.ToList().IndexOf(nameClientDir);

            //6.3
            var tempParts = pathParts.ToList();
            tempParts.RemoveRange(0, startIndex);
            pathParts = tempParts.ToArray();
            path = Path.Combine(pathParts);

            //7
            return path.Contains(verifyPattern);
        }

        string CombineUrlClientFile(string path)
        {
            var baseUrl = _Options.Value.WebFiles.FilesUrl;

            if (path.StartsWith("/") || path.StartsWith("\\"))
                path = path.Substring(1);

            var url = Path.Combine(baseUrl, path);

            return url;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="totalSize"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        async Task DownloadFiles(string folder, double totalSize, List<(string url, string path)> files)
        {
            _DownloadedBytes = 0d;
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

        async Task DeleteFiles(List<string> files)
        {
            foreach(var item in files)
            {
                await DeleteFile(item);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        void ReportProgress(DownloadProgressArguments data)
        {  
            _DownloadedBytes += data.DownloadBytes;
            var args = new DownloadProgressArguments(_DownloadedBytes, data.TotalBytes, data.NameFile);

            OnProgressChanged?.Invoke(args);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        async Task SaveFile(MemoryStream data, string path)
        {
            new FileInfo(path).Directory?.Create();

            var buffer = data.ToArray();

            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            await fileStream.WriteAsync(buffer, 0, buffer.Length);
        }

        async Task DeleteFile(string path)
        {


            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None, 1, FileOptions.DeleteOnClose | FileOptions.Asynchronous))
            {
                await stream.FlushAsync();
            }
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
                                return BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
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
                                return BitConverter.ToString(sha1.Hash).Replace("-", "").ToLowerInvariant();
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
