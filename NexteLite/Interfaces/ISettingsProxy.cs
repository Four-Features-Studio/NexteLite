using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate void SettingsApplyClickHanglder(IParamsSettingPage paramsSetting);
    public delegate void DeleteAllClickHanglder();

    public interface ISettingsProxy
    {
        void SetMaxRam(long ram);
        void SetParams(IParamsSettingPage data);

        event SettingsApplyClickHanglder SettingsApplyClick;
        event DeleteAllClickHanglder DeleteAllClick;
    }
}
