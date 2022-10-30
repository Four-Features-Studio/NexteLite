﻿using Newtonsoft.Json;
using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NexteLite.Services
{
    public class WebService : IWebService
    {
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

        public void GetFiles()
        {
            throw new NotImplementedException();
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

            TestProfile.UpdatesDir = new List<string>()
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

            TestProfile.UpdateIgnore = new List<string>();

            return new List<ServerProfile>
            {
                TestProfile,
                new ServerProfile
                {
                    NID = Guid.NewGuid().ToString(),
                    Title = "test2"
                },                                
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
        public async Task<MemoryStream> Download(long totalDownloadSize, string downloadUrl, string name, IProgress<DownloadProgressArguments> progress)
        {
            using var httpClient = new HttpClient { Timeout = TimeSpan.FromDays(1) };
            using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            using var contentStream = await response.Content.ReadAsStreamAsync();
            var totalBytesRead = 0L;
            var readCount = 0L;
            var buffer = new byte[8192];
            var isMoreToRead = true;

            using var dataStream = new MemoryStream(8192);

            do
            {
                var bytesRead = await contentStream.ReadAsync(buffer);
                if (bytesRead == 0)
                {
                    isMoreToRead = false;
                    progress.Report(new DownloadProgressArguments(totalDownloadSize, totalBytesRead, name));
                    continue;
                }

                await dataStream.WriteAsync(buffer.AsMemory(0, bytesRead));

                totalBytesRead += bytesRead;
                readCount++;

                if (readCount % 100 == 0)
                    progress.Report(new DownloadProgressArguments(totalDownloadSize, totalBytesRead, name));

            }
            while (isMoreToRead);

            return dataStream;
        }
    }
}
