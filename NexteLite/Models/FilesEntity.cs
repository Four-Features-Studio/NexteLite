using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Models
{
    public class FilesEntity
    {
        [JsonProperty("typeHash")]
        public int TypeHash { get; set; }

        [JsonProperty("files")]
        public List<FileEntity> Files { get; set; }
    }
}
