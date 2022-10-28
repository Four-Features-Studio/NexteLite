using Newtonsoft.Json;
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
        [JsonProperty("ip")]
        public string Ip { get; set; } 

        [JsonProperty("port")]
        public int Port { get; set; }
    }
}
