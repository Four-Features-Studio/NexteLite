using NexteLite.Models;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public interface IDownloadingProxy
    {
        void SetState(DownloadingState state);
        void OnDownloadingProgress(DownloadProgressArguments args);
    }
}
