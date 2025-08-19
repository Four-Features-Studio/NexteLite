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

#if DEVMODE
            var message = "Неверный логин или пароль";

            var profile = new Profile
            {
                Username = username,
                Uuid = "9b15dea6606e47a4a241420251703c59",
                AccessToken = "test",
                Avatar = "https://placekitten.com/g/300/300"
            };

            return (true, profile, message);
#else
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

                if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<AuthData>(content);

                    return (data.Successful, data.Profile, data.Message);
                }

                return (false, new Profile(), "Возможно произошла ошибка при авторизации");
            }
            catch (Exception ex)
            {
                _Messages.SendInfo("Произошла ошибка при авторизации, возможно удаленный сервер не доступен");
                _Logger.LogError(ex.ToString());
                return (false, new Profile(), "Произошла ошибка при авторизации");
            }
#endif
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

        public async Task<FilesEntity> GetFiles(string dir, string profileId)
        {
            try
            {
                var url = UrlUtil.Combine(_BaseApiUrl, _Options.Value.API.FilesClientUrl);

                var requestModel = new FilesRequest
                {
                    ProfileId = profileId,
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
#if DEVMODE
            var TestProfile = new ServerProfile()
            {
                ProfileId = Guid.NewGuid().ToString(),
                Title = "TestClient",
                Version = "1.12.2",
                SortIndex = 0,
                Dir = "TestClient",
                AssetIndex = "1.12",
                MainClass = "net.minecraft.client.main.Main",
                HideProfile = false
            };

            TestProfile.Server = new Server() { Ip = "188.225.47.71", Port = 25565 };

            TestProfile.UpdatesList = new List<string>()
            {
                "libraries",
                "natives",
                "mods",
                "configs",
                "resourcespacks",
                "minecraft.jar",
                "forge.jar",
                "liteloader.jar"
            };

            TestProfile.IgnoreList = new List<string>()
            {
                "mods/1.12"
            };

            return new List<ServerProfile>
            {
                new ServerProfile
                {
                    ProfileId = Guid.NewGuid().ToString(),
                    Title = "test2"
                },
                TestProfile,
                new ServerProfile
                {
                    ProfileId = Guid.NewGuid().ToString(),
                    Title = "test3"
                }
            };
#else
            try
            {
                var url = UrlUtil.Combine(_BaseApiUrl, _Options.Value.API.ProfilesUrl);

                var request = new HttpRequestMessage(HttpMethod.Get, url);

                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

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
#endif
        }

        public async void Logout(string accessToken)
        {
            try
            {
                var url = UrlUtil.Combine(_BaseApiUrl, _Options.Value.API.LogoutUrl);

                var requestModel = new LogoutRequest
                {
                    AccessToken = accessToken,
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
