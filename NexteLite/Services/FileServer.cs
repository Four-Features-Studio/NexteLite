using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NexteLite.Services
{
    public class FileServer : IFileService
    {
        IWebService _WebService;
        ISettingsLauncher _SettingsLauncher;

        public FileServer(IWebService webService, ISettingsLauncher settingsLauncher)
        {
            _WebService = webService;
            _SettingsLauncher = settingsLauncher;
        }

        public async Task CheckFilesClient()
        {

        }

        public async Task DownloadClient()
        {

        }

        public async Task DownloadAssets()
        {

        }

        public async Task RemoveAllClients()
        {

        }

        public async Task RemoveClient(string nId)
        {

        }

        async Task DownloadFiles(string folder, List<(string url, string path, long size)> files)
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
                var data = await _WebService.Download(file.size, file.url, file.path, progress);
                queue.Enqueue((data, path, file.path));
            }

            isDownloadEnd = true;

            await save;
        }
        void ReportProgress(DownloadProgressArguments args)
        {

        }

        static async Task SaveFile(MemoryStream data, string path)
        {
            new FileInfo(path).Directory?.Create();

            var buffer = data.ToArray();

            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            await fileStream.WriteAsync(buffer, 0, buffer.Length);
        }
        
    }
}
