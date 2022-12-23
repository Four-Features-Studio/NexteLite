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
        Task<(bool result, Profile profile, string message)> Auth(string username, string password);
        void Logout(string accessToken);
        Task<UpdateInfo> CheckUpdates(string hash);

        Task<List<ServerProfile>> GetServerProfiles();
        Task<FilesEntity> GetFiles(string dir, string profileId);
        Task<AssetsIndex> GetAssetsIndex(string version);
        Task<MemoryStream> Download(double totalSize, string downloadUrl, string name, IProgress<DownloadProgressArguments> progress);
    }


}
