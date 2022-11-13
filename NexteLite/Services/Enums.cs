using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Services.Enums
{
    public enum PageType
    {
        Login = 0,
        Main,
        Console,
        Settings,
        Downloading,
        Running
    }

    public enum MinecraftState
    {
        Running,
        Closed
    }

    public enum ChecksumMethod
    {
        MD5,
        SHA1
    }

    public enum DownloadingState
    {
        DownloadAssets,
        DownloadClient,
        HashAssets,
        HashClient
    }

    public enum ActionFile
    {
        Delete,
        Download,
        Update
    }
}
