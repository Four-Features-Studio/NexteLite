using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Models
{
    public class AuthData
    {
        public bool Successful { get; set; }
        public string Message { get; set; }
        public Profile Profile { get; set; }
    }
}
