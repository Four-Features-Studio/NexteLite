using NexteLite.Interfaces;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NexteLite.Pages
{
    /// <summary>
    /// Логика взаимодействия для ConsolePage.xaml
    /// </summary>
    public partial class ConsolePage : Page, IPage, IConsoleProxy
    {
        public PageType Id => PageType.Console;
        public bool IsOverlay { get; private set; }

        public ConsolePage()
        {
            InitializeComponent();
            IsOverlay = true;
        }

    }
}
