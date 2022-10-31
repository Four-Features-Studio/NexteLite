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
    public interface IMinecraftService
    {
        event OnMinecraftStateChangedHandler OnMinecraftStateChanged;
        Task Play(ServerProfile profile);
    }
}
