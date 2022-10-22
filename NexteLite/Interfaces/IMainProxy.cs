using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate void SetProfileFunction(Profile profile);
    public delegate void SetServerProfilesFunction(List<ServerProfile> servers);

    public interface IMainProxy
    {
        SetProfileFunction SetProfile { get; set; }
        SetServerProfilesFunction SetServerProfiles { get; set; }
    }
}
