using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace NexteLite.Services
{
    public class WebService : IWebService
    {
        IOptions<AppSettings> _Options;
        HttpClient _HttpClient;
        IMessageService _Messages;

        ILogger<WebService> _Logger;

        string _BaseApiUrl;

        public WebService(HttpClient client, IOptions<AppSettings> options, IMessageService messages, ILogger<WebService> logger)
        {
            _HttpClient = client;

            _Options = options;

            _Messages = messages;

            _Logger = logger;

            _BaseApiUrl = _Options.Value.API.BaseApiUrl;
            
            //TODO проверку вынести в отдельный метод

            if (string.IsNullOrEmpty(_BaseApiUrl))
                throw new ArgumentNullException("В настройках лаунчера не указаны ссылки на api");
        }

        public async Task<(bool result, Profile profile, string message)> Auth(string username, string password)
        {

            try
            {
                var url = UrlUtil.Combine(_BaseApiUrl, _Options.Value.API.AuthUrl);

                var requestModel = new AuthRequest
                {
                    Username = username,
                    Password = password
                };

                var model = JsonConvert.SerializeObject(requestModel);
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(model, Encoding.UTF8);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _HttpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<AuthData>(content);
                    return (true, data.Profile, data.Message);
                }

                return (false, null, string.Empty);
            }
            catch (Exception ex)
            {
                _Messages.SendInfo("Произошла ошибка при авторизации, возможно удаленный сервер не доступен");
                _Logger.LogError(ex.ToString());
                return (false, null, string.Empty);
            }

            ////message = "Неверный логин или пароль";
            ////profile = null;

            ////return false;

            //message = string.Empty;

            //profile = new Profile
            //{
            //    Username = username,
            //    Uuid = "9b15dea6606e47a4a241420251703c59",
            //    AccessToken = "test",
            //    ServerToken = "test",
            //    Avatar = @"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"
            //};
            //return true;
        }
        public async Task<UpdateInfo> CheckUpdates(string hashsum)
        {
            try
            {
                var url = UrlUtil.Combine(_BaseApiUrl, _Options.Value.API.CheckUpdateUrl);

                var requestModel = new UpdateRequest
                {
                    Checksum = hashsum
                };

                var model = JsonConvert.SerializeObject(requestModel);
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(model, Encoding.UTF8);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _HttpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<UpdateInfo>(content);
                    return data;
                }

                return null;
            }
            catch (Exception ex)
            {
                _Messages.SendInfo("Произошла ошибка при запросе наличия обновлений, возможно удаленный сервер не доступен");
                _Logger.LogError(ex.ToString());
                return null;
            }
        }

        public async Task<FilesEntity> GetFiles(string dir)
        {
            try
            {
                var url = UrlUtil.Combine(_BaseApiUrl, _Options.Value.API.FilesClientUrl);

                var requestModel = new FilesRequest
                {
                    Directory = dir
                };

                var model = JsonConvert.SerializeObject(requestModel);
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(model, Encoding.UTF8);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _HttpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var files = JsonConvert.DeserializeObject<FilesEntity>(content);
                    Console.WriteLine("TODO - Errors");
                    return files;
                }

                return null;
            }
            catch (Exception ex)
            {
                _Messages.SendInfo("Произошла ошибка при запросе файлов клиента, возможно удаленный сервер не доступен");
                _Logger.LogError(ex.ToString());
                return null;
            }
        }
        public async Task<AssetsIndex> GetAssetsIndex(string version)
        {

            try
            {
                var url = UrlUtil.Combine(_BaseApiUrl, _Options.Value.API.AssetsIndexUrl);

                var requestModel = new AssetsIndexRequest
                {
                    Version = version
                };

                var model = JsonConvert.SerializeObject(requestModel);
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(model, Encoding.UTF8);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _HttpClient.SendAsync(request);

                var assetsIndex = new AssetsIndex();

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    assetsIndex = JsonConvert.DeserializeObject<AssetsIndex>(content);
                    Console.WriteLine("TODO - Errors");
                    return assetsIndex;
                }

                return assetsIndex;
            }
            catch (Exception ex)
            {
                _Messages.SendInfo("Произошла ошибка при запросе ассетов клиента, возможно удаленный сервер не доступен");
                _Logger.LogError(ex.ToString());
                return null;
            }
           
        }

        public async Task<List<ServerProfile>> GetServerProfiles()
        {

            try
            {
                var url = UrlUtil.Combine(_BaseApiUrl, _Options.Value.API.ProfilesUrl);

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _HttpClient.SendAsync(request);

                var profiles = new List<ServerProfile>();

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    profiles = JsonConvert.DeserializeObject<List<ServerProfile>>(content);
                    Console.WriteLine("TODO - Errors");
                    return profiles;
                }

                return profiles;
            }
            catch (Exception ex)
            {
                _Messages.SendInfo("Произошла ошибка при запросе профилей, возможно удаленный сервер не доступен");
                _Logger.LogError(ex.ToString());
                return null;
            }

            //var TestProfile = new ServerProfile()
            //{
            //    Title = "TestClient",
            //    ConfigVersion = "1",
            //    Version = "1.12.2",
            //    SortIndex = 0,
            //    Dir = "TestClient",
            //    AssetDir = "Assets",
            //    AssetIndex = "1.12",
            //    MainClass = "net.minecraft.client.main.Main",
            //    HideProfile = false
            //};

            //TestProfile.Server = new Server() { Ip = "188.225.47.71", Port = 25565 };

            //TestProfile.UpadtesList = new List<string>()
            //{
            //    "libraries",
            //    "natives",
            //    "mods",
            //    "configs",
            //    "resourcespacks",
            //    "minecraft.jar",
            //    "forge.jar",
            //    "liteloader.jar"
            //};

            //TestProfile.IgnoreList = new List<string>()
            //{
            //    "mods/1.12"
            //};

            //return new List<ServerProfile>
            //{
            //    new ServerProfile
            //    {
            //        NID = Guid.NewGuid().ToString(),
            //        Title = "test2"
            //    },
            //    TestProfile,
            //    new ServerProfile
            //    {
            //        NID = Guid.NewGuid().ToString(),
            //        Title = "test3"
            //    }
            //};
        }

        public async void Logout(string uuid)
        {
            try
            {
                var url = UrlUtil.Combine(_BaseApiUrl, _Options.Value.API.LogoutUrl);

                var requestModel = new LogoutRequest
                {
                    Uuid = uuid,
                };

                var model = JsonConvert.SerializeObject(requestModel);
                var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                request.Content = new StringContent(model, Encoding.UTF8);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                var response = await _HttpClient.SendAsync(request);

            }
            catch (Exception ex)
            {
                _Messages.SendInfo("Произошла ошибка при запросе логаута, возможно удаленный сервер не доступен");
                _Logger.LogError(ex.ToString());
            }
        }

        public async Task<MemoryStream> Download(double totalSize, string downloadUrl, string name, IProgress<DownloadProgressArguments> progress)
        {
            try
            {
                var response = await _HttpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);

                using var dataStream = new MemoryStream();

                if (!response.IsSuccessStatusCode)
                    return dataStream;

                using var contentStream = await response.Content.ReadAsStreamAsync();
                var downloadedSize = 0L;
                var readCount = 0L;
                var buffer = new byte[1024];
                var isMoreToRead = true;

                var lastElapsed = 0l;

                var networkSpeed = 0;

                Stopwatch st = new Stopwatch();
                st.Start();

                do
                {
                    var bytesRead = await contentStream.ReadAsync(buffer);
                    if (bytesRead == 0)
                    {
                        isMoreToRead = false;
                        await Task.Run(() =>
                        {
                            progress.Report(new DownloadProgressArguments(downloadedSize, totalSize, name));
                        });
                        continue;
                    }

                    await dataStream.WriteAsync(buffer.AsMemory(0, bytesRead));

                    downloadedSize += bytesRead;
                }
                while (isMoreToRead);

                return dataStream;
            }
            catch(Exception ex)
            {
                _Logger.LogError(ex.ToString());
                return null;
            }
            
        }
        public void Dispose()
        {
            _HttpClient.Dispose();
        }
    }
}
