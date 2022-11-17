using NexteLite.Services;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NexteLite.Interfaces
{
    public interface IMainWindow
    {
        void SendMessage(string message);
        void ShowPage(Page page);
        void ShowOverlay(Page page);
        void HideOverlay();
        void Show();
        void Minimized();
    }
}
