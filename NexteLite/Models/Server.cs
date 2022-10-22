using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NexteLite.Models
{
    public class Server
    {
        [JsonPropertyName("ip")]
        public string Ip { get; set; } 

        [JsonPropertyName("port")]
        public int Port { get; set; }
    }
}
