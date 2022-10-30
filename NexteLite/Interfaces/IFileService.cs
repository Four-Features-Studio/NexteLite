using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate void OnProgressChangedHandler(DownloadProgressArguments arguments);
    public interface IFileService
    {
        event OnProgressChangedHandler OnProgressChanged;

        Task CheckAndCreateInjector();
    }
}
