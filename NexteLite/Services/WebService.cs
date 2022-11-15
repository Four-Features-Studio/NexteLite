using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NexteLite.Services
{
    public class WebService : IWebService
    {
        IOptions<AppSettings> _Options;
        HttpClient _HttpClient;

        string _BaseApiUrl;

        public WebService(HttpClient client, IOptions<AppSettings> options)
        {
            _HttpClient = client;

            _Options = options;

            _BaseApiUrl = _Options.Value.API.BaseApiUrl;

            //TODO проверку вынести в отдельный метод

            if (string.IsNullOrEmpty(_BaseApiUrl))
                throw new ArgumentNullException("В настройках лаунчера не указаны ссылки на api");
        }

        public bool Auth(string username, string password, out Profile profile, ref string message)
        {
            //message = "Неверный логин или пароль";
            //profile = null;

            //return false;

            message = string.Empty;

            profile = new Profile
            {
                Username = username,
                Uuid = "9b15dea6606e47a4a241420251703c59",
                AccessToken = "test",
                ServerToken = "test",
                Avatar = "/avatar/placeholder.png"
            };
            return true;
        }

        public async Task<FilesEntity> GetFiles(string dir)
        {
            var url = Url.Combine(_BaseApiUrl, _Options.Value.API.FilesClientUrl);

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
        public async Task<AssetsIndex> GetAssetsIndex(string version)
        {
            var url = Url.Combine(_BaseApiUrl, _Options.Value.API.AssetsIndexUrl);

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

        public List<ServerProfile> GetServerProfiles()
        {
            var TestProfile = new ServerProfile()
            {
                ConfigVersion = "1",
                Version = "1.12.2",
                SortIndex = 0,
                Dir = "TestClient",
                AssetDir = "Assets",
                AssetIndex = "1.12",
                MainClass = "net.minecraft.client.main.Main",
                HideProfile = false
            };

            TestProfile.Server = new Server() { Ip = "188.225.47.71", Port = 25565 };

            TestProfile.UpadtesList = new List<string>()
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
                    NID = Guid.NewGuid().ToString(),
                    Title = "test2"
                },
                TestProfile,
                new ServerProfile
                {
                    NID = Guid.NewGuid().ToString(),
                    Title = "test3"
                }
            };
        }

        public void Logout()
        {
            throw new NotImplementedException();
        }

        public async Task<MemoryStream> Download(double totalSize, string downloadUrl, string name, IProgress<DownloadProgressArguments> progress)
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
        public void Dispose()
        {
            _HttpClient.Dispose();
        }
    }
}
