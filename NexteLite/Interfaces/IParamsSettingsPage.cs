using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public interface IParamsSettingPage
    {
        public double MaximumRam { get; }

        public double CurrentRam { get; }

        public bool AutoConnectMode { get; }

        public bool FullScreenMode { get; }

        public bool DebugMode { get; }

        public string Path { get; }
    }
}
