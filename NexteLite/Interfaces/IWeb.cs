using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public interface IWebService: IDisposable
    {
        bool Auth(string username, string password, out Profile profile, ref string message);
        void Logout();

        Task<UpdateInfo> CheckUpdates(string hash);

        List<ServerProfile> GetServerProfiles();
        Task<FilesEntity> GetFiles(string dir);
        Task<AssetsIndex> GetAssetsIndex(string version);
        Task<MemoryStream> Download(double totalSize, string downloadUrl, string name, IProgress<DownloadProgressArguments> progress);
    }


}
