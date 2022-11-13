using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Models
{
    public class FileEntity
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("path")]
        public string Path { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("size")]
        public double Size { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }
    }
}
