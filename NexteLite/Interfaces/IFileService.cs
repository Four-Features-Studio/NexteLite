using Newtonsoft.Json;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate void OnProgressChangedHandler(DownloadProgressArguments arguments);
    public interface IFileService
    {
        event OnProgressChangedHandler OnProgressChanged;

        Task CheckFilesClient();

        Task CheckAssets();
        Task CheckAndCreateInjector();

        Task DownloadClient(FilesEntity files, ServerProfile profile);

        Task DownloadAssets(AssetsIndex assetsIndex, string version);

        Task RemoveAllClients();

        Task RemoveClient(ServerProfile profile);
    }
}
