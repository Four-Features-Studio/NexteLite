using Newtonsoft.Json.Linq;
using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Services;
using NexteLite.Services.Enums;
using NexteLite.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
using static System.Net.WebRequestMethods;

namespace NexteLite.Pages
{
    /// <summary>
    /// Логика взаимодействия для MainPage.xaml
    /// </summary>
    public partial class MainPage : Page, IPage, IMainProxy, INotifyPropertyChanged
    {
        public event SettingsClickHandler SettingsClick;
        public event LogoutClickHandler LogoutClick;
        public event SocialClickHandler SocialClick;
        public event PlayClickHandler PlayClick;

        ObservableCollection<Button> _SocialButtons = new ObservableCollection<Button>();
        public ObservableCollection<Button> SocialButtons => _SocialButtons;

        public ObservableCollection<ServerProfile> ServerProfiles { get; set; }
        public PageType Id => PageType.Main;
        public bool IsOverlay { get; private set; }

        private Profile profile;
        public Profile Profile
        {
            get
            {
                return profile;
            }
            set
            {
                profile = value;
                OnPropertyChanged();
            }
        }

        ImageSource _Avatar;
        public ImageSource Avatar
        {
            get
            {
                return _Avatar;
            }
            set
            {
                _Avatar = value;
                OnPropertyChanged();
            }
        }



        public MainPage()
        {
            InitializeComponent();
            DataContext = this;
            IsOverlay = false;
        }

        public void SetProfile(Profile profile)
        {
            Profile = profile;
            Avatar = ImageUtils.GetAvatar(profile is null ? string.Empty : profile.Avatar, "pack://application:,,,/NexteLite;component/Resources/avatar.png");
        }

        public void SetServerProfiles(List<ServerProfile> profiles)

        {
            var sorted = profiles.OrderByDescending(x => x.SortIndex);
            ServerProfiles = new ObservableCollection<ServerProfile>(sorted);
            OnPropertyChanged(nameof(ServerProfiles));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="social"></param>
        public void AddSocial(SocialItem social)
        {
            ResourceDictionary resource = new ResourceDictionary
            {
                Source = new Uri(social.ImageResource)
            };

            var icon_res = resource[social.ImageCode] as DrawingImage;

            if (icon_res != null)
            {
                var btn = new Button();
                btn.Height = 26;
                btn.Width = 26;
                btn.Margin = new Thickness(0, 10, 0, 0);
                btn.Cursor = Cursors.Hand;
                btn.Tag = social.Url;

                var img = new Image();
                img.Source = icon_res;
                btn.Content = img;
                btn.Click += Social_OnClick;

                SocialButtons.Add(btn);
            }
        }

        private void Social_OnClick(object sender, RoutedEventArgs e)
        {
            var tag = ((Button)sender).Tag as string;
            SocialClick?.Invoke(tag);
        }

        private void Buttons_OnClick(object sender, RoutedEventArgs e)
        {
            var tag = ((Button)sender).Tag as string;
            switch (tag)
            {
                case "settings":
                    SettingsClick?.Invoke();
                    break;
                case "logout":
                    LogoutClick?.Invoke();
                    break;
            }
        }

        #region [INotifyPropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nID"></param>
        private void ServerCarousel_PlayClick(string nID, string presetId)
        {
            PlayClick?.Invoke(nID, presetId);
        }
    }
}
