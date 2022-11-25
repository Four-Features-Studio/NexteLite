﻿using Newtonsoft.Json;
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
        [JsonProperty("profileId")]
        public string ProfileId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

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
       
        [JsonProperty("assetIndex")]
        public string AssetIndex { get; set; }
        
        [JsonProperty("updatesList")]
        public List<string> UpdatesList { get; set; }
        
        [JsonProperty("ignoreList")]
        public List<string> IgnoreList { get; set; }
        
        [JsonProperty("jvmArgs")]
        public List<string> JvmArgs { get; set; }

        [JsonProperty("mainClass")]
        public string MainClass { get; set; }

        [JsonProperty("hideProfile")] 
        public bool HideProfile { get; set; }
    }
}
