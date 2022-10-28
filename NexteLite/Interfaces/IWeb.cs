using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public interface IWebService
    {
        bool Auth(string username, string password, out Profile profile, ref string message);
        void Logout();
        List<ServerProfile> GetServerProfiles();
        void GetFiles();
        Task<MemoryStream> Download(long totalDownloadSize, string downloadUrl, string name, IProgress<DownloadProgressArguments> progress);
    }


}
