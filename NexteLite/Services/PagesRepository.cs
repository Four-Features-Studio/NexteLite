using Microsoft.Extensions.DependencyInjection;
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
        IServiceProvider _ServiceProvider;
        public PagesRepository(IServiceProvider services, MainPage mainPage, LoginPage loginPage, ConsolePage consolePage, SettingsPage settingsPage)
        {
            _ServiceProvider = services;

            Pages.Add(((IPage)mainPage).Id, mainPage);
            Pages.Add(((IPage)loginPage).Id, loginPage);
            Pages.Add(((IPage)consolePage).Id, consolePage);
            Pages.Add(((IPage)settingsPage).Id, settingsPage);
        }
        public Page GetPage(PageType type)
        {
            if (Pages.ContainsKey(type))
                return Pages[type];

            throw new Exception("Page not found");
        }

        public Page GetRunningPage()
        {
            return _ServiceProvider.GetRequiredService<RunningPage>();
        }

        public Page GetDownloadingPage()
        {
            return _ServiceProvider.GetRequiredService<DownloadingPage>();
        }
    }
}
