using NexteLite.Models;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate void OnLoadedHandler();
    public interface IUpdateProxy
    {
        void SetState(UpdateState state);
    }
}
