using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    /// Логика взаимодействия для DownloadingPage.xaml
    /// </summary>
    public partial class DownloadingPage : Page, IPage, IDownloadingProxy, INotifyPropertyChanged
    {
        public DownloadingPage()
        {
            InitializeComponent();
        }

        public PageType Id => PageType.Downloading;

        public bool IsOverlay => false;

        string _FileName;
        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                _FileName = value;
                OnPropertyChanged();
            }
        }

        long _TotalSize;
        public long TotalSize
        {
            get
            {
                return _TotalSize;
            }
            set
            {
                _TotalSize = value;
                OnPropertyChanged();
            }
        }

        long _CurrentSize;
        public long CurrentSize
        {
            get
            {
                return _CurrentSize;
            }
            set
            {
                _CurrentSize = value;
                OnPropertyChanged();
            }
        }

        public void OnDownloadingProgress(DownloadProgressArguments args)
        {
            
        }

        public void SetState(DownloadingState state)
        {
            
        }

        #region [INotifyPropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}
