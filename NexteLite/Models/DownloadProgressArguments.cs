using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Models
{
    public class DownloadProgressArguments
    {
        public DownloadProgressArguments(double downloadBytes, double totalBytes, string nameFile)
        {
            DownloadBytes = downloadBytes;
            TotalBytes = totalBytes;
            NameFile = nameFile;
        }   

        public double DownloadBytes { get; set; }
        public double TotalBytes { get; set; }
        public string NameFile { get; set; }
    }
}
