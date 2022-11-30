using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
using static NexteLite.Controls.ServerCarousel;

namespace NexteLite.Controls
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

        string _SelectedPreset;
        public string SelectedPreset
        {
            get
            {
                return _SelectedPreset;
            }
            set
            {
                _SelectedPreset = value;
                OnPropertyChanged();
            }
        }

        bool _IsPresetAvailable;
        public bool IsPresetAvailable
        {
            get
            {
                return _IsPresetAvailable;
            }
            set
            {
                _IsPresetAvailable = value;
                OnPropertyChanged();
            }
        }

        public SolidColorBrush IsOnlineColor { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom("#7bb458"));
        public SolidColorBrush IsOfflineColor { get; set; } = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ad4444"));

        public event OnPlayClickHandler OnPlayClick;

        public delegate void OnSelectedPresetHandler(string profileId, string presetId);
        public event OnSelectedPresetHandler OnSelectedPreset;

        List<ServerPreset> _Presets = new List<ServerPreset>();
        public List<ServerPreset> Presets
        {
            get
            {
                return _Presets;
            }
            set
            {
                _Presets = value;
                OnPropertyChanged();
            }
        }


        IMineStat _State;

        public ItemServer(IMineStat state)
        {
            InitializeComponent();
            _State = state;
        }

        public void Initialize(ServerProfile profile, string presetId = "")
        {

            NID = profile.ProfileId;
            Title = profile.Title;

            if(profile.Server != null)
            {
                Ip = profile.Server.Ip;
                Port = profile.Server.Port;
                IsOnline = false;
            }

            if(profile.Presets is not null && profile.Presets.Count > 0)
            {
                IsPresetAvailable = true;
                Presets = profile.Presets.OrderByDescending(x => x.Index).ToList();

                if (string.IsNullOrEmpty(presetId))
                {
                    SelectedPreset = profile.Presets.First().Id;
                } 
                else if(Presets.Any(x => x.Id == presetId))
                {
                    SelectedPreset = presetId;
                }
                else
                {
                    SelectedPreset = profile.Presets.First().Id;
                }
            }

            ServerAvatar = ImageUtils.GetImageFromBase64(profile.Avatar, "pack://application:,,,/NexteLite;component/Resources/placeholder.jpg");

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
            if (fast)
            {
                Height = 394;
                Border.Height = 279;
                Border.Width = 234;

                ServerName.Opacity = 1;
                PlayButton.Opacity = 1;
                PlayerOnServer.Opacity = 1;
                BackgroundShadow.Opacity = 0;

                return;
            }

            var name = "Select";
            Storyboard sb = this.FindResource(name) as Storyboard;

            sb.Begin(this);
        }

        public void Unselect(bool fast = false)
        {
            if (fast)
            {
                Height = 332;
                Border.Height = 223;
                Border.Width = 188;

                ServerName.Opacity = 0;
                PlayButton.Opacity = 0;
                PlayerOnServer.Opacity = 0;
                BackgroundShadow.Opacity = 0.3;

                return;
            }

            Storyboard sb = this.FindResource("Unselect") as Storyboard;
            sb.Begin(this);
        }

        public void Show()
        {
            Storyboard sb = this.FindResource("Show") as Storyboard;

            sb.Begin(this);

            this.Visibility = Visibility.Visible;
        }

        public void Hide(bool fast = false)
        {
            if (fast)
            {
                this.Visibility = Visibility.Collapsed;
                return;
            }

            Storyboard sb = this.FindResource("Hide") as Storyboard;

            sb.Completed += (sender, args) =>
            {
                this.Visibility = Visibility.Collapsed;
            };

            sb.Begin(this);
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
            OnPlayClick?.Invoke(NID, SelectedPreset);
        }

        private void Selector_Click(object sender, RoutedEventArgs e)
        {
            if(sender is MenuItem item && item.Tag is string id)
            {
                SelectedPreset = id;
            }
        }
    }
}
