using NexteLite.Interfaces;
using NexteLite.Pages;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NexteLite.Services
{
    public class PagesRepository : IPagesRepository
    {
        private Dictionary<PageType,Page> Pages = new Dictionary<PageType,Page>();

        public PagesRepository(MainPage mainPage, LoginPage loginPage, ConsolePage consolePage, SettingsPage settingsPage, RunningPage runningPage)
        {
            Pages.Add(((IPage)mainPage).Id, mainPage);
            Pages.Add(((IPage)loginPage).Id, loginPage);
            Pages.Add(((IPage)consolePage).Id, consolePage);
            Pages.Add(((IPage)settingsPage).Id, settingsPage);
            Pages.Add(((IPage)runningPage).Id, runningPage);
        }
        public Page GetPage(PageType type)
        {
            return Pages[type];
        }
    }
}
