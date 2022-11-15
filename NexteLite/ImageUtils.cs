using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NexteLite
{
    public class ImageUtils
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="base64">Закодированная картинка в BASE64</param>
        /// <param name="placeholder"></param>
        /// <returns></returns>
        public static ImageSource GetImageFromBase64(string base64, string placeholder)
        {
            ImageSource ReturnDefault()
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(placeholder);
                bi.EndInit();
                return bi;
            }

            if(string.IsNullOrEmpty(base64))
            {
                return ReturnDefault();
            }

            var date = Convert.FromBase64String(base64 is null ? String.Empty : base64);

            //Выглядит как говно код, да и на вкус точно говно код))
            if(date != null && date.Length > 0)
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = new MemoryStream(date);
                bi.EndInit();
                return bi;
            }
            else
            {
                return ReturnDefault();
            }
        }
        static byte[] DecodeBase64(string input)
        {
            var bytes = new Span<byte>(new byte[256]); // 256 is arbitrary

            if (!Convert.TryFromBase64String(input, bytes, out var bytesWritten))
            {
                throw new InvalidOperationException("The input is not a valid base64 string");
            }

            return bytes.Slice(0, bytesWritten).ToArray();
        }
    }
}
