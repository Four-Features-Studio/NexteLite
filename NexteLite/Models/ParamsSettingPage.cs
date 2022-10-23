using NexteLite.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Models
{
    public class ParamsSettingPage : IParamsSettingPage
    {
        public ParamsSettingPage(double maximumRam, double currentRam, bool autoConnectMode, bool fullScreenMode, bool debugMode, string path)
        {
            MaximumRam = maximumRam;
            CurrentRam = currentRam;
            AutoConnectMode = autoConnectMode;
            FullScreenMode = fullScreenMode;
            DebugMode = debugMode;
            Path = path;
        }

        public double MaximumRam {get; private set;}
        public double CurrentRam { get; private set; }
        public bool AutoConnectMode { get; private set; }
        public bool FullScreenMode { get; private set; }
        public bool DebugMode { get; private set; }
        public string Path { get; private set; }
    }
}
