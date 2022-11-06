using NexteLite.Models;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate void OnMinecraftStateChangedHandler(MinecraftState state);
    public delegate void OnMinecraftLogRecivedHandler(string log);
    public interface IMinecraftService
    {
        event OnMinecraftStateChangedHandler OnMinecraftStateChanged;

        event OnMinecraftLogRecivedHandler OnMinecraftLogRecived;
        Task Play(ServerProfile profile);

        void Kill();
    }
}
