using NexteLite.Interfaces;
using NexteLite.Services;
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

namespace NexteLite
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        #region Window Events
        private void Window_Close(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Minimized(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Window_Drag(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void Window_Back(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        private void Viewer_PreviewExecuted(object sender, NavigatingCancelEventArgs e)
        {
            if (e.NavigationMode == NavigationMode.Forward || e.NavigationMode == NavigationMode.Back)
            {
                e.Cancel = true;
            }
        }

        public void ShowPage(Page page)
        {
            Viewer.NavigationService.Navigate(page);
        }

        public void ShowOverlay(Page page)
        {
            
        }
    }
}
