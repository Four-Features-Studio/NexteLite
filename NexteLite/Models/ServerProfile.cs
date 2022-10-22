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
        [JsonPropertyName("nID")]
        public string NID { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("configVersion")]
        public string ConfigVersion { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

        [JsonPropertyName("servers")]
        public Server Servers { get; set; }

        [JsonPropertyName("sortIndex")]
        public int SortIndex { get; set; }
        
        [JsonPropertyName("dir")]
        public string Dir { get; set; }

        [JsonPropertyName("assetDir")]
        public string AssetDir { get; set; }
       
        [JsonPropertyName("assetIndex")]
        public string AssetIndex { get; set; }
        
        [JsonPropertyName("updatesDir")]
        public List<string> UpdatesDir { get; set; }
        
        [JsonPropertyName("updateIgnore")]
        public List<string> UpdateIgnore { get; set; }
        
        [JsonPropertyName("updateOptional")]
        public List<string> UpdateOptional { get; set; }
        
        [JsonPropertyName("jvmArgs")]
        public List<string> JvmArgs { get; set; }

        [JsonPropertyName("clientArgs")] 
        public List<string> ClientArgs { get; set; }

        [JsonPropertyName("mainClass")]
        public string MainClass { get; set; }

        [JsonPropertyName("whiteRole")] 
        public List<string> WhiteRole { get; set; }

        [JsonPropertyName("hideProfile")] 
        public bool HideProfile { get; set; }
    }
}
