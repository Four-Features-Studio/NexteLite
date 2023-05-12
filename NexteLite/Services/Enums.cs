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
        Running,
        Update
    }

    public enum MinecraftState
    {
        Running,
        Closed
    }

    public enum ChecksumMethod
    {
        SHA1 = 0,
        MD5
    }

    public enum DownloadingState
    {
        DownloadAssets,
        DownloadClient,
        HashAssets,
        HashClient
    }

    public enum UpdateState
    {
        Check,
        Update,
        Error
    }

    public enum ActionFile
    {
        Delete,
        Download,
        Update
    }
}
