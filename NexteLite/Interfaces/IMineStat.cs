using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public interface IMineStat
    {
        void Subscribe(IMineStatSubscriber subscriber);

        Task<ServerState> CheckOnline(string ip, int port);
    }
}
