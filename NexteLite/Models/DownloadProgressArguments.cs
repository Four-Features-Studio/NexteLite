using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Models
{
    public class DownloadProgressArguments
    {
        public DownloadProgressArguments(long totalDownloadBytes, long totalBytes, string nameFile)
        {
            TotalDownloadBytes = totalDownloadBytes;
            TotalBytes = totalBytes;
            NameFile = nameFile;
        }   

        public long TotalDownloadBytes { get; set; }
        public long TotalBytes { get; set; }
        public string NameFile { get; set; }
    }
}
