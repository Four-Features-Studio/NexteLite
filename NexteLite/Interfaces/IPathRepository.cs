﻿using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public interface IPathRepository
    {
        string GetLocalLauncher();
        string GetAppDataPath();
        string GetInjectorPath();
        string GetUpdateAgentPath();
        string GetProjectPath();
        string GetClientsPath();
        string GetClientPath(ServerProfile profile);
        string GetNativesPath(ServerProfile profile);
        string GetLibrariesPath(ServerProfile profile);

        string GetMinecraftPath(ServerProfile profile);
        string GetAssetsPath();
        string GetAssetsIndexesPath();
        string GetAssetsObjectsPath();

        string GetJavaPath();
    }
}
