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

        public string GetHashsumLeuncher()
        {
            var path = _Path.GetLocalLauncher();
            var hash = string.Empty;

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var bs = new BufferedStream(fs);

            hash = GetHashsum(ChecksumMethod.SHA1, bs);

            return hash;
        }

        public async Task UpdateLauncher()
        {
            var pathUpdate =_Path.GetAppDataPath();
            var url = _Options.Value.API.UpdateUrl;
            await DownloadFiles(pathUpdate, 1, new List<(string url, string path)>() { (url,"new.update") });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<(ActionFile action, FileEntity file)>> CheckFilesClient(ServerProfile profile, FilesEntity webFiles, string presetId = "")
        {
            void RemoveAllPresets()
            {
                var presets = profile.Presets.Select(x => x.Dir).ToList();
                if (presets.Count > 0)
                {
                    foreach (var preset in presets)
                    {
                        var toDelete = webFiles.Files.Where(x => CheckPreset(preset, x.Path, out _)).ToList();

                        webFiles.Files.RemoveAll(x => toDelete.Select(h => h.Hash).Contains(x.Hash) && toDelete.Select(h => h.Path).Contains(x.Path));
                    }
                }
            }


            if (webFiles is null)
                return new List<(ActionFile, FileEntity)>();

            if(profile.UpdatesList is null)
            {
                _Logger.LogCritical($"ВНИМАНИЕ! Битый профиль {profile.Title}");
                return new List<(ActionFile, FileEntity)>();
            }
                

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
            var verifyFiles = new List<FileEntity>();
            var hashesFiles = new List<FileEntity>();
            var fileIncorect = new List<(ActionFile action, FileEntity file)>();

            var pathClient = _Path.GetClientPath(profile);

            if (string.IsNullOrEmpty(pathClient))
            {
                _Logger.LogError($"Неверно указан путь к файлам игры");
                return new List<(ActionFile, FileEntity)>();
            }

            if (string.IsNullOrEmpty(presetId))
            {
                RemoveAllPresets();
            }
            else
            {
                var presets = profile.Presets.Where(x => x.Id != presetId).Select(x => x.Dir).ToList();
                if (presets.Count > 0)
                {
                    foreach (var preset in presets)
                    {
                        var toDelete = webFiles.Files.Where(x => CheckPreset(preset, x.Path, out _)).ToList();

                        webFiles.Files.RemoveAll(x => toDelete.Select(h => h.Hash).Contains(x.Hash) && toDelete.Select(h => h.Path).Contains(x.Path));
                    }
                }
 
            }

            verifyFiles = webFiles.Files;

            if (!string.IsNullOrEmpty(presetId) && profile.Presets.Any(x => x.Id == presetId))
            {
                var preset = profile.Presets.FirstOrDefault();
                foreach (var file in verifyFiles)
                {
                    if (CheckPreset(preset.Dir, file.Path, out var dir))
                    {
                        file.Path = file.Path.Replace(dir, GetDirectorySeparatorUsedInPath(dir).ToString());
                    }
                }
            }
            else
            {
                RemoveAllPresets();
            }


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
            localFiles = localFiles.Where(x => profile.UpdatesList.Any(u => CheckNameInFileOrFolder(pathClient,u,x))).ToArray();

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
                if (profile.IgnoreList is not null && profile.IgnoreList.Any(x => CheckNameInFileOrFolder(pathClient, x, local.Path)))
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

            new FileInfo(path).Directory?.Create();

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

                if (sumChecked == sumCorrect)
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
        /// <returns></returns>
        public async Task CheckAndCreateUpdateAgent()
        {
            var path = _Path.GetUpdateAgentPath();
            var updateAgent = Properties.Resources.NexteAgent;

            new FileInfo(path).Directory?.Create();

            _Logger.LogDebug("Проверка существования агента");

            if (File.Exists(path))
            {
                var sumCorrect = GetHashsum(ChecksumMethod.SHA1, updateAgent);

                var sumChecked = string.Empty;
                using (FileStream fstream = new FileStream(path, FileMode.Open))
                {
                    byte[] buffer = new byte[fstream.Length];
                    await fstream.ReadAsync(buffer, 0, buffer.Length);

                    sumChecked = GetHashsum(ChecksumMethod.SHA1, buffer);
                }

                if (sumChecked == sumCorrect)
                {
                    _Logger.LogDebug("Агент обновления прошел проверку успешно");
                    return;
                }
            }

            _Logger.LogDebug("Агент обновления не прошел проверку либо не обнаружен. Агент будет распакован из лаунчера");
            using (FileStream fstream = new FileStream(path, FileMode.Create))
            {
                await fstream.WriteAsync(updateAgent, 0, updateAgent.Length);
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
            //var RootDir = _SettingsLauncher.RootDir;
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
                            var url = CombineUrlClientFile(item.file.Url);

                            var path = item.file.Path;

                            if (path.StartsWith("/") || path.StartsWith("\\"))
                                path = item.file.Path.Substring(1);

                            item.file.Path = Path.Combine(ClientDir, path);
                            filesToDownload.Add((url, item.file.Path));
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
        /// <param name="clientDirPath">Пусть к папке клиента</param>
        /// <param name="path">Полный путь, в котором будет искаться соответсвие</param>
        /// <param name="pathName">Часть пути, которую нужно найти в полном формате пути</param>
        /// <returns></returns>
        bool CheckNameInFileOrFolder(string clientDirPath, string pathName,  string path)
        {
            //1 - Приводим путь проверки к формату пути ОС
            //2 - Получаем название папки игрового клиента из пути.
            //3 - Создаем путь до условной папки/файла и проверяем, существует ли в пути расширение файла
            //4 - Проверяем условный путь, является ли это файлом или папкой, если папка то добавляем разделитель в конец
            //5 - Создаем паттерн пути для проверки
            //6 - Превращаем путь в относительный путь от папки Updates (прим. D://.nexte//Updates//HiTech//minecraft.jar -> HiTech//minecraft.jar)
            //  | 1 - Разбиваем путь на массив по разделителю
            //  | 2 - Ищем индекс элемента в массиве который является папкой с клиентом
            //  | 3 - Удаляем из пути все лишнее, чтоб получился относительный путь от папки Updates
            //7 - Проверяем, есть ли в пути наш паттерн


            string NormalizePath(string path)
            {
                var pathParts = path.Split(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                return Path.Combine(pathParts);
            }

            //1 
            pathName = NormalizePath(pathName);

            //2
            var nameClientDir = Path.GetFileName(clientDirPath);

            //3
            var checkedPath = Path.Combine(clientDirPath, pathName);

            //4
            if (string.IsNullOrEmpty(Path.GetExtension(checkedPath)))
                pathName = pathName + GetDirectorySeparatorUsedInPath(path);

            //5
            var verifyPattern = Path.Combine(nameClientDir, pathName);

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

        bool CheckPreset(string presetsDir, string path, out string dir)
        {

            string NormalizePath(string path)
            {
                var pathParts = path.Split(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
                return GetDirectorySeparatorUsedInPath(path) + Path.Combine(pathParts) + GetDirectorySeparatorUsedInPath(path);
            }

            dir = string.Empty;

            //1 
            presetsDir = NormalizePath(presetsDir);

            var resutl = path.Contains(presetsDir);

            dir = presetsDir;

            return resutl;
        }

        string CombineUrlClientFile(string file_url)
        {
            var baseUrl = _Options.Value.WebFiles.FilesUrl;
            var url = UrlUtil.Combine(baseUrl, file_url);
            return url;
        }
        char GetDirectorySeparatorUsedInPath(string path)
        {
            if (path.Contains(Path.AltDirectorySeparatorChar))
                return Path.AltDirectorySeparatorChar;

            return Path.DirectorySeparatorChar;
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
                        await Task.Delay(100);
                    }
                }
                while (!isDownloadEnd || queue.Count != 0);
            });

            foreach (var file in files)
            {
                var path = Path.Combine(folder, file.path);
                var data = await _WebService.Download(totalSize, file.url, file.path, progress);
                if (data != null)
                    queue.Enqueue((data, path, file.path));
                else
                    _Logger.LogError($"Не удалось скачать файл - {file.path}");
            }

            isDownloadEnd = true;

            await save;
        }

        async Task DeleteFiles(List<string> files)
        {
            try
            {
                foreach (var item in files)
                {
                    await DeleteFile(item);
                }
            }
            catch(Exception ex)
            {
                _Logger.LogError(ex.ToString());
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
            try
            {
                new FileInfo(path).Directory?.Create();

                SetEveryoneAccess(path);

                var buffer = data.ToArray();

                using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
                await fileStream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex.ToString());
            }   
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
                        return Hasher.ComputeSHA1(file);
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
        public string GetHashsum(ChecksumMethod method, BufferedStream file)
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
                                    else
                                    {
                                        break;
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
