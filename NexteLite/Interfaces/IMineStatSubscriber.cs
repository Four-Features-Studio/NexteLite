using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public interface IMineStatSubscriber
    {
        public string Ip { get; set; }
        public int Port { get; set; }

        void UpdateInfo(ServerState state);
    }
}
