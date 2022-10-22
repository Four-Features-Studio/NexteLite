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

        public PagesRepository(MainPage main, LoginPage login, ConsolePage console, SettingsPage settings)
        {
            Pages.Add(((IPage)main).Id,main);
            Pages.Add(((IPage)login).Id,login);
            Pages.Add(((IPage)console).Id,console);
            Pages.Add(((IPage)settings).Id,settings);
        }
        public Page GetPage(PageType type)
        {
            return Pages[type];
        }
    }
}
