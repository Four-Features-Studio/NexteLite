﻿using Newtonsoft.Json.Linq;
using NexteLite.Interfaces;
using NexteLite.Models;
using NexteLite.Services;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public MainPage()
        {
            InitializeComponent();
            DataContext = this;
            IsOverlay = false;
        }

        public void SetProfile(Profile profile)
        {
            Profile = profile;
        }

        public void SetServerProfiles(List<ServerProfile> profiles)
        {
            ServerProfiles = new ObservableCollection<ServerProfile>(profiles);
            OnPropertyChanged(nameof(ServerProfiles));
        }

        public void ShowProgressBar()
        {
            Action dlg = () =>
            {
                download_control.Visibility = Visibility.Visible;
                Storyboard sb = this.FindResource("Show") as Storyboard;
                sb.Completed += (s, args) => { main_control.Visibility = Visibility.Collapsed; };
                sb.Begin();
            };
            Dispatcher.BeginInvoke(dlg, System.Windows.Threading.DispatcherPriority.Render);
        }

        public void HideProgressBar()
        {
            Action dlg = () =>
            {
                main_control.Visibility = Visibility.Visible;
                Storyboard sb = this.FindResource("Hide") as Storyboard;
                sb.Begin();
            };
            Dispatcher.BeginInvoke(dlg, System.Windows.Threading.DispatcherPriority.Render);
        }

        public void ShowPlug()
        {
            Action dlg = () =>
            {
                plug_control.Visibility = Visibility.Visible;
                main_control.Visibility = Visibility.Collapsed;
            };
            Dispatcher.BeginInvoke(dlg, System.Windows.Threading.DispatcherPriority.Render);
        }

        public void HidePlug()
        {
            Action dlg = () =>
            {
                plug_control.Visibility = Visibility.Collapsed;
                main_control.Visibility = Visibility.Visible;
            };
            Dispatcher.BeginInvoke(dlg, System.Windows.Threading.DispatcherPriority.Render);
        }

        public void UpdateInfoFile(string data, string total)
        {
            Action dlg = () =>
            {
                info_totalsize.Text = total;
                info_currentfile.Text = data;
            };
            Dispatcher.BeginInvoke(dlg, System.Windows.Threading.DispatcherPriority.Normal);
        }

        public void ChangeProgress(string value, double percent)
        {
            Action dlg = () =>
            {
                info_currentsize.Text = value;
                item_progress.Value = percent;
            };
            Dispatcher.BeginInvoke(dlg, System.Windows.Threading.DispatcherPriority.Render);
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
        private void ServerCarousel_PlayClick(string nID)
        {
            PlayClick?.Invoke(nID);
        }
    }
}
