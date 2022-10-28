using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NexteLite.Models
{
    public class Profile
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("uuid")]
        public string Uuid { get; set; }

        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("serverToken")]
        public string ServerToken { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

    }
}
