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
    /// Логика взаимодействия для UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage : Page, IPage, IUpdateProxy
    {
        public UpdatePage()
        {
            InitializeComponent();
            Loaded += UpdatePage_Loaded;
        }

        private void UpdatePage_Loaded(object sender, RoutedEventArgs e)
        {
            OnLoaded?.Invoke();
        }

        public void SetState(UpdateState state)
        {
            
        }

        public PageType Id => PageType.Update;

        public bool IsOverlay => false;

        public event OnLoadedHandler OnLoaded;
    }
}
