using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static NexteLite.ServerCarousel;

namespace NexteLite
{
    /// <summary>
    /// Логика взаимодействия для ItemServer.xaml
    /// </summary>
    public partial class ItemServer : UserControl, INotifyPropertyChanged, IMineStatSubscriber
    {
        string _NID;
        public string NID
        {
            get
            {
                return _NID;
            }
            set
            {
                _NID = value;
                OnPropertyChanged();
            }
        }

        string _Title = "TEST";
        public string Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
                OnPropertyChanged();
            }
        }


        bool _IsSelect;
        public bool IsSelect
        {
            get
            {
                return _IsSelect;
            }
            set
            {
                _IsSelect = value;
                OnPropertyChanged();
            }
        }

        bool _IsHide;
        public bool IsHide
        {
            get
            {
                return _IsHide;
            }
            set
            {
                _IsHide = value;
                OnPropertyChanged();
            }
        }

        ImageSource _ServerAvatar;
        public ImageSource ServerAvatar
        {
            get
            {
                return _ServerAvatar;
            }
            set
            {
                _ServerAvatar = value;
                OnPropertyChanged();
            }
        }

        string _Ip;
        public string Ip
        {
            get
            {
                return _Ip;
            }
            set
            {
                _Ip = value;
                OnPropertyChanged();
            }
        }

        int _Port;
        public int Port
        {
            get
            {
                return _Port;
            }
            set
            {
                _Port = value;
                OnPropertyChanged();
            }
        }

        int _PlayerMax;
        public int PlayerMax
        {
            get
            {
                return _PlayerMax;
            }
            set
            {
                _PlayerMax = value;
                OnPropertyChanged();
            }
        }

        int _PlayerCurrent;
        public int PlayerCurrent
        {
            get
            {
                return _PlayerCurrent;
            }
            set
            {
                _PlayerCurrent = value;
                OnPropertyChanged();
            }
        }

        bool _IsOnline;
        public bool IsOnline
        {
            get
            {
                return _IsOnline;
            }
            set
            {
                _IsOnline = value;
                OnPropertyChanged();
            }
        }

        public SolidColorBrush IsOnlineColor { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom("#7bb458"));
        public SolidColorBrush IsOfflineColor { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ad4444"));

        public event OnPlayClickHandler OnPlayClick;

        IMineStat _State;

        public ItemServer(IMineStat state)
        {
            InitializeComponent();
            _State = state;
        }

        public void Initialize(ServerProfile profile)
        {

            NID = profile.NID;
            Title = profile.Title;

            if(profile.Server != null)
            {
                Ip = profile.Server.Ip;
                Port = profile.Server.Port;
                IsOnline = false;
            }


            byte[] binaryData = Convert.FromBase64String(profile.Avatar is null ? String.Empty : profile.Avatar);

            BitmapImage bi = new BitmapImage();
            if (binaryData.Length > 0)
            {
                bi.BeginInit();
                bi.StreamSource = new MemoryStream(binaryData);
                bi.EndInit();
                ServerAvatar = bi;
            }
            else
            {
                bi.BeginInit();
                bi.UriSource = new Uri("pack://application:,,,/NexteLite;component/Resources/placeholder.jpg");
                bi.EndInit();
                ServerAvatar = bi;
            }

            _State.Subscribe(this);
        }

        public void UpdateInfo(ServerState state)
        {
            if (state != null)
            {
                PlayerCurrent = state.PlayerCurrent;
                PlayerMax = state.PlayerMax;
                IsOnline = state.IsOnline;
            }
        }

        public void Select(bool fast = false)
        {
            var name = "Select";
            Storyboard sb = this.FindResource(name) as Storyboard;

            sb.Begin(this, true);
            if (fast)
                sb.SkipToFill();
        }

        public void Unselect(bool fast = false)
        {
            Storyboard sb = this.FindResource("Unselect") as Storyboard;

            sb.Begin(this, true);
            if (fast)
                sb.SkipToFill();
        }

        public void Show(bool fast = false)
        {
            Storyboard sb = this.FindResource("Show") as Storyboard;

            sb.Begin(this, true);
            if (fast)
                sb.SkipToFill();

            this.Visibility = Visibility.Visible;
        }

        public void Hide(bool fast = false)
        {
            Storyboard sb = this.FindResource("Hide") as Storyboard;

            sb.Begin(this, true);
            if (fast)
                sb.SkipToFill();

            sb.Completed += (sender, args) =>
            {
                this.Visibility = Visibility.Collapsed;
            };

        }

        #region [INotifyPropertyChanged]
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            OnPlayClick?.Invoke(NID);
        }
    }
}
