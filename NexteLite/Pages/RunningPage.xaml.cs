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
    /// Логика взаимодействия для RunningPage.xaml
    /// </summary>
    public partial class RunningPage : Page, IPage, IRunningProxy
    {
        public RunningPage()
        {
            InitializeComponent();
        }

        public PageType Id => PageType.Running;

        public bool IsOverlay => false;
    }
}
