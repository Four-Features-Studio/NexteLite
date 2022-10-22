using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace NexteLite
{
    /// <summary>
    /// Логика взаимодействия для ItemServer.xaml
    /// </summary>
    public partial class ItemServer : UserControl, INotifyPropertyChanged
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

        string _Title;
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

        public ItemServer()
        {
            InitializeComponent();
        }

        public void Initialize(ServerProfile profile)
        {

            NID = profile.NID;
            Title = profile.Title;

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
        }

        public void UpdateData()
        {
            //Action dlg = () =>
            //{
            //    (bool, int, int) profile = default;
            //    if (LauncherData.profilesData != null)
            //    {                   
            //        if (LauncherData.profilesData.TryGetValue(nID, out profile))
            //        {
            //            var text_player = $"Игроков на сервере {profile.Item2} из {profile.Item3}";
            //            item_playercount.Text = text_player;
            //            if (profile.Item1)
            //            {
            //                item_play.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#7bb458"));
            //                item_play.Content = "Играть";
            //                item_playercount.Visibility = Visibility.Visible;
            //            }
            //            else
            //            {
            //                item_play.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#ad4444"));
            //                item_play.Content = "В Офлайне";
            //                item_playercount.Visibility = Visibility.Collapsed;
            //            }
            //        }
            //    }
            //};
            //Dispatcher.BeginInvoke(dlg, System.Windows.Threading.DispatcherPriority.Background);
        }

        public void Select()
        {
            Storyboard sb = this.FindResource("Select") as Storyboard;
            sb.Begin();
            item_block.Visibility = Visibility.Collapsed;
        }

        public void Unselect()
        {
            Storyboard sb = this.FindResource("Unselect") as Storyboard;
            sb.Begin();
            item_block.Visibility = Visibility.Visible;
        }

        public void Show()
        {
            Storyboard sb = this.FindResource("Show") as Storyboard;
            sb.Begin();
            item_block.Visibility = Visibility.Collapsed;
            this.Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Storyboard sb = this.FindResource("Hide") as Storyboard;
            sb.Begin();

            sb.Completed += (sender, args) =>
            {
                this.Visibility = Visibility.Collapsed;
            };

            item_block.Visibility = Visibility.Visible;

        }

        private void item_play_Click(object sender, RoutedEventArgs e)
        {

        }

        private void item_settings_Click(object sender, RoutedEventArgs e)
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
