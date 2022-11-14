using Newtonsoft.Json;
using NexteLite.Models;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate void OnProgressChangedHandler(DownloadProgressArguments arguments);
    public interface IFileService
    {
        event OnProgressChangedHandler OnProgressChanged;

        Task<List<(ActionFile action, FileEntity file)>> CheckFilesClient(ServerProfile profile, FilesEntity files);

        Task<List<(string hash, double size)>> CheckAssets(ServerProfile profile, AssetsIndex assets);

        Task CheckAndCreateInjector();

        Task DownloadClient(List<(ActionFile action, FileEntity file)> files, ServerProfile profile);

        Task DownloadAssets(AssetsIndex assetsIndex, List<(string hash, double size)> assetsDownload, string version);

        Task RemoveAllClients();

        Task RemoveClient(ServerProfile profile);
    }
}
