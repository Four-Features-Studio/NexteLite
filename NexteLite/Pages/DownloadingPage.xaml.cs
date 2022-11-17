using NexteLite.Controls;
using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

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

        string _Status;
        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                OnPropertyChanged();
            }
        }

        bool _IsDownloading;
        public bool IsDownloading
        {
            get
            {
                return _IsDownloading;
            }
            set
            {
                _IsDownloading = value;
                OnPropertyChanged();
            }
        }

        double _Percentage;
        public double Percentage
        {
            get
            {
                return _Percentage;
            }
            set
            {
                _Percentage = value;
                OnPropertyChanged();
            }
        }

        double _DownloadedSize;
        public double DownloadedSize
        {
            get
            {
                return _DownloadedSize;
            }
            set
            {
                _DownloadedSize = value;
                OnPropertyChanged();
            }
        }

        double _TotalSize;
        public double TotalSize
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

        double _CurrentSpeed;
        public double CurrentSpeed
        {
            get
            {
                return _CurrentSpeed;
            }
            set
            {
                _CurrentSpeed = value;
                OnPropertyChanged();
            }
        }

        double _MaximumSpeed;
        public double MaximumSpeed
        {
            get
            {
                return _MaximumSpeed;
            }
            set
            {
                _MaximumSpeed = value;
                OnPropertyChanged();
            }
        }

        ObservableCollection<NetworkChartData> _ChartDatas = new ObservableCollection<NetworkChartData>();
        public ObservableCollection<NetworkChartData> ChartDatas => _ChartDatas;

        public void OnDownloadingProgress(DownloadProgressArguments args)
        {
            Percentage = args.DownloadBytes / args.TotalBytes * 100;
            MaximumSpeed = args.MaxNetworkSpeed;
            CurrentSpeed = args.NetworkSpeed;

            TotalSize = args.TotalBytes;
            DownloadedSize = args.DownloadBytes;

            ChartDatas.Add(new NetworkChartData(CurrentSpeed, DateTime.Now));

            Debug.WriteLine($"DOWNLOADED Part {Percentage}");
        }

        public void SetState(DownloadingState state)
        {
            switch (state)
            {
                case DownloadingState.HashAssets:
                    IsDownloading = false;
                    Status = Properties.Resources.lcl_txt_DownloadingPage_AssetsHashStatus;
                    break;

                case DownloadingState.HashClient:
                    IsDownloading = false;
                    Status = Properties.Resources.lcl_txt_DownloadingPage_ClientHashStatus;
                    break;

                case DownloadingState.DownloadAssets:
                    IsDownloading = true;
                    Status = Properties.Resources.lcl_txt_DownloadingPage_AssetsDownloadingStatus;
                    break;

                case DownloadingState.DownloadClient:
                    IsDownloading = true;
                    Status = Properties.Resources.lcl_txt_DownloadingPage_ClientDownloadingStatus;
                    break;
            }
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
