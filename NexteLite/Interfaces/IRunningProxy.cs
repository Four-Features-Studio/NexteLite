using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate void OnKillClientClickHandler();

    public interface IRunningProxy
    {
        void SetParams(bool isDebug);

        void WriteLog(string log);

        event OnKillClientClickHandler OnKillClientClick;
    }
}
