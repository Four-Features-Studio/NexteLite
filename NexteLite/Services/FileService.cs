using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic.Logging;
using Newtonsoft.Json;
using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Services.Enums;
using NexteLite.Utils;
using Ookii.Dialogs.Wpf;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;

namespace NexteLite.Services
{
    public class FileService : IFileService
    {
        public event OnProgressChangedHandler OnProgressChanged;

        IWebService _WebService;
        ISettingsLauncher _SettingsLauncher;
        IOptions<AppSettings> _Options;
        IPathRepository _Path;

        IMessageService _MessageService;

        ILogger<FileService> _Logger;
        public FileService(IWebService webService, 
            IPathRepository pathRepository, 
            ISettingsLauncher settingsLauncher, 
            IOptions<AppSettings> options,
            IMessageService messages,
            ILogger<FileService> logger)
        {
            _WebService = webService;
            _SettingsLauncher = settingsLauncher;
            _Options = options;
            _Path = pathRepository;
            _MessageService = messages;
            _Logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<(ActionFile action, FileEntity file)>> CheckFilesClient(ServerProfile profile, FilesEntity webFiles)
        {
            if (webFiles is null)
                return new List<(ActionFile, FileEntity)>();
            //1 - Получить все локальные файлы.
            //2 - Отсеить файлы которые не входят в updatelist
            //3 - Получаем хеши локальных файлов
            //4 - Сверяем хеши локальные с веб хешами.
            //  | 1 - Если локального файла нет в веб файлах, то его нужно удалить
            //  | 2 - Если локального файла нет в веб файлах, но есть в игнор листе, то его не трогаем
            //  | 3 - Если локальный файл есть в веб файлах и хеши совпадают, то файл корректный
            //  | 4 - Если локальный файл есть в веб файлах и хеши не совпадают, то его нужно обновить
            //  | 5 - Если в веб файлах есть файл которого нет локально, то его нужно скачать.
            _Logger.LogDebug($"Запущенна проверка файлов клиента {profile.Title}");

            var typeHash = (ChecksumMethod)webFiles.TypeHash;
            var verifyFiles = webFiles.Files;

            var hashesFiles = new List<FileEntity>();
            var fileIncorect = new List<(ActionFile action, FileEntity file)>();

            var pathClient = _Path.GetClientPath(profile);

            if (!Directory.Exists(pathClient))
            {
                _Logger.LogDebug("Указанной папки не существует, клиент будет скачан полностью");
                //0
                foreach (var file in verifyFiles)
                {
                    fileIncorect.Add((ActionFile.Download, file));
                }
                return fileIncorect;
            }

            //1
            _Logger.LogDebug("Запрос всех локальных файлов клиента");
            var localFiles = Directory.GetFiles(pathClient, "*", SearchOption.AllDirectories);

            //2
            _Logger.LogDebug("Фильтрация локальных файлов, которые не входят в список обновления");
            localFiles = localFiles.Where(x => profile.UpadtesList.Any(u => CheckNameInFileOrFolder(pathClient, x, u))).ToArray();

            //3
            _Logger.LogDebug("Расчет хешсум локальных файлов");
            foreach (var item in localFiles)
            {
                var hash = await GetHashsumAsync(typeHash, item);
                hashesFiles.Add(new FileEntity() { Name = Path.GetFileName(item), Path = item, Size = 0, Hash = hash });
            }

            //4 - 1,2
            _Logger.LogDebug("Сбор списка файлов на удаления, которых нет в списках на сервере");
            foreach (var local in hashesFiles)
            {
                //2
                if (profile.IgnoreList.Any(x => CheckNameInFileOrFolder(pathClient, local.Path, x)))
                    continue;

                if(!verifyFiles.Any(x => x.Name == local.Name))
                    fileIncorect.Add((ActionFile.Delete, local));
            }

            //4 - 3,4,5
            _Logger.LogDebug("Основная провека на соответсвие серверных файлов с локальными");
            foreach (var file in verifyFiles)
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

            _Logger.LogDebug("Проверка завершена.");
            return fileIncorect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<(string hash, double size)>> CheckAssets(ServerProfile profile, AssetsIndex assets)
        {
            if (assets is null)
                return new List<(string, double)>();

            _Logger.LogDebug($"Запущенна проверка ассетов {profile.AssetIndex}");
            var objectsPath = _Path.GetAssetsObjectsPath();
            var assetsIncorect = new List<(string hash, double size)>();

            if (!Directory.Exists(objectsPath))
            {
                _Logger.LogDebug("Указанной папки не существует, ассеты будут скаченны полностью");

                foreach (var asset in assets.Objects.Values)
                {
                    var twoSymbol = asset.Hash.Substring(0, 2);
                    var path = Path.Combine(twoSymbol, asset.Hash);

                    assetsIncorect.Add((asset.Hash, asset.Size));
                }

                return assetsIncorect;
            }

            _Logger.LogDebug("Получение всех локальный файлов ассетов");
            var localFiles = Directory.GetFiles(objectsPath, "*", SearchOption.AllDirectories);


            _Logger.LogDebug("Основная проверка на соответсвие/отсутсвие файлов ассетов");
            foreach (var asset in assets.Objects.Values)
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
            _Logger.LogDebug("Проверка завершена");
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

            _Logger.LogDebug("Проверка существования инжектора");

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
                {
                    _Logger.LogDebug("Инжектор прошел проверку успешно");
                    return;
                }
            }

            _Logger.LogDebug("Инжектор не прошел проверку либо не обнаружен. Инжектор будет распакован из лаунчера");
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
            _Logger.LogDebug($"Скачивание клиента {profile.Title}");
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
            _Logger.LogDebug($"Скачивание ассетов версии {version}");

            if (string.IsNullOrEmpty(_Options.Value.WebFiles.AssetsUrl))
            {
                throw new ArgumentNullException("In the launcher settings there is no link to download the assets");
            }

            var pathAssets = _Path.GetAssetsPath();
            var pathIndexes = _Path.GetAssetsIndexesPath();
            var pathObjects = _Path.GetAssetsObjectsPath();

            if (!Directory.Exists(pathAssets))
            {
                Directory.CreateDirectory(pathAssets);
                SetEveryoneAccess(pathAssets);
            }

            var pathIndex = Path.Combine(pathIndexes, version + ".json");

            new FileInfo(pathIndex).Directory?.Create();
            SetEveryoneAccess(pathIndex);

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
                var url = UrlUtil.Combine(_Options.Value.WebFiles.AssetsUrl, twoSymbol, asset.hash);

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
            _Logger.LogDebug("Запущен процесс удаления всех клиентов");

            _Logger.LogDebug("Запрос папки клиентов");
            var clients = _Path.GetClientsPath();
            DirectoryInfo clientsDir = new DirectoryInfo(clients);

            _Logger.LogDebug("Запрос папки ассетов");
            var assets = _Path.GetAssetsPath();
            DirectoryInfo assetsDir = new DirectoryInfo(assets);

            var tasks = new List<Task>();


            tasks.Add(Task.Run(() =>
            {
                foreach (FileInfo file in clientsDir.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in clientsDir.EnumerateDirectories())
                {
                    dir.Attributes &= ~FileAttributes.ReadOnly;
                    dir.Delete(true);
                }
            }));

            tasks.Add(Task.Run(() =>
            {
                foreach (FileInfo file in assetsDir.EnumerateFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in assetsDir.EnumerateDirectories())
                {
                    dir.Attributes &= ~FileAttributes.ReadOnly;
                    dir.Delete(true);
                }
            }));

            _Logger.LogDebug("Задачи на удаления созданны");
            await Task.WhenAll(tasks);

            _Logger.LogDebug("Удаление завершенно");

            _MessageService.SendInfo("Все клиенты удалены");
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
            var url = UrlUtil.Combine(baseUrl, path);
            return url;
        }


        NetworkSpeeder speeder;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="totalSize"></param>
        /// <param name="files"></param>
        /// <returns></returns>
        async Task DownloadFiles(string folder, double totalSize, List<(string url, string path)> files)
        {
            var _DownloadedBytes = 0d;
            var progress = new Progress<DownloadProgressArguments>(ReportProgressLocal);

            #region Тут происходит магия, тут я считаю скорость скачивания, кря

            speeder = new NetworkSpeeder();

            void ReportProgressLocal(DownloadProgressArguments data)
            {
                _DownloadedBytes += data.DownloadBytes;

                var args = new DownloadProgressArguments(_DownloadedBytes, data.TotalBytes, data.NameFile);
                args.NetworkSpeed = speeder.CalculateSpeed(_DownloadedBytes);
                args.MaxNetworkSpeed = speeder.MaxSpeed;

                ReportProgress(args);
            }

            #endregion

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
            OnProgressChanged?.Invoke(data);
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

            SetEveryoneAccess(path);

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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirName"></param>
        /// <returns></returns>
        bool SetEveryoneAccess(string dirName)
        {

            try
            {
                var path = Path.GetDirectoryName(dirName);

                // Make sure directory exists
                if (Directory.Exists(path) == false)
                {
                    _Logger.LogError($"Directory {path} does not exist, so permissions cannot be set.");
                    return false;
                }

                // Get directory access info
                DirectoryInfo dinfo = new DirectoryInfo(path);
                DirectorySecurity dSecurity = dinfo.GetAccessControl();

                // Add the FileSystemAccessRule to the security settings. 
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));

                // Set the access control
                dinfo.SetAccessControl(dSecurity);
                dinfo.Attributes &= ~FileAttributes.ReadOnly;

                _Logger.LogInformation($"Everyone FullControl Permissions were set for directory {path}");
                return true;

            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.Message);
                return false;
            }


        }
    }
}
