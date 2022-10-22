
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NexteLite.Interfaces
{
    public interface ICoreLaucnher
    {
        void ShowPage(PageType id);
        void ShowOverlay(PageType id);
    }
}
