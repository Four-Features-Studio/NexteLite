using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NexteLite.Models
{
    public class ServerProfile
    {
        [JsonProperty("nID")]
        public string NID { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("configVersion")]
        public string ConfigVersion { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("server")]
        public Server Server { get; set; }

        [JsonProperty("sortIndex")]
        public int SortIndex { get; set; }
        
        [JsonProperty("dir")]
        public string Dir { get; set; }

        [JsonProperty("assetDir")]
        public string AssetDir { get; set; }
       
        [JsonProperty("assetIndex")]
        public string AssetIndex { get; set; }
        
        [JsonProperty("updatesDir")]
        public List<string> UpadtesList { get; set; }
        
        [JsonProperty("updateIgnore")]
        public List<string> IgnoreList { get; set; }
        
        [JsonProperty("jvmArgs")]
        public List<string> JvmArgs { get; set; }

        [JsonProperty("clientArgs")] 
        public List<string> ClientArgs { get; set; }

        [JsonProperty("mainClass")]
        public string MainClass { get; set; }

        [JsonProperty("hideProfile")] 
        public bool HideProfile { get; set; }
    }
}
