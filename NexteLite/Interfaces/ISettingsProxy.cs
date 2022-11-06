using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate void SettingsApplyClickHangler(IParamsSettingPage paramsSetting);
    public delegate void DeleteAllClickHangler();

    public interface ISettingsProxy
    {
        void SetMaxRam(long ram);
        void SetParams(IParamsSettingPage data);

        event SettingsApplyClickHangler SettingsApplyClick;
        event DeleteAllClickHangler DeleteAllClick;
    }
}
