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
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("uuid")]
        public string Uuid { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }

        [JsonPropertyName("serverToken")]
        public string ServerToken { get; set; }

        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }

    }
}
