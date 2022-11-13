using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite
{
    public class UrlUtils
    {
        public static string UrlCombine(string baseUrl, string relative)
        {
            if(relative.StartsWith("/") || relative.StartsWith("\\"))
            {
                relative = relative.Substring(1);
            }

            Uri baseUri = new Uri(baseUrl);
            Uri combined = new Uri(baseUri, relative);
            return baseUri.AbsoluteUri;
        }


    }
}
