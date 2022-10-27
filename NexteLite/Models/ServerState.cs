using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Models
{
    public class ServerState
    {
        public int PlayerMax { get; }
        public int PlayerCurrent { get; }
        public bool IsOnline { get; }

        public ServerState(int playerMax, int playerCurrent, bool isOnline)
        {
            PlayerMax = playerMax;
            PlayerCurrent = playerCurrent;
            IsOnline = isOnline;
        }
    }
}
