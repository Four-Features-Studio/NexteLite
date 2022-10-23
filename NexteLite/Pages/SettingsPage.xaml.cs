﻿using NexteLite.Interfaces;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
    /// Логика взаимодействия для SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page, IPage, ISettingsProxy, INotifyPropertyChanged
    {
        public PageType Id => PageType.Settings;
        public bool IsOverlay { get; private set; }

        double _MaximumRam;
        public double MaximumRam
        {
            get
            {
                return _MaximumRam;
            }
            set
            {
                _MaximumRam = value;
                OnPropertyChanged();
            }
        }

        int _CurrentRam;
        public int CurrentRam
        {
            get
            {
                return _CurrentRam;
            }
            set
            {
                _CurrentRam = value;
                Debug.WriteLine($"RAM - {value}");
                OnPropertyChanged();
            }
        }

        bool _AutoConnectMode;
        public bool AutoConnectMode
        {
            get
            {
                return _AutoConnectMode;
            }
            set
            {
                _AutoConnectMode = value;
                OnPropertyChanged();
            }
        }

        bool _FullScreenMode;
        public bool FullScreenMode
        {
            get
            {
                return _FullScreenMode;
            }
            set
            {
                _FullScreenMode = value;
                OnPropertyChanged();
            }
        }

        bool _DebugMode;
        public bool DebugMode
        {
            get
            {
                return _DebugMode;
            }
            set
            {
                _DebugMode = value;
                OnPropertyChanged();
            }
        }

        string _Path;
        public string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
                OnPropertyChanged();
            }
        }

        public SettingsPage()
        {
            InitializeComponent();
            IsOverlay = true;
        }

        public void SetMaxRam(long memory)
        {
            Action dlg = () =>
            {
                var ram = (memory / 1024);

                var ram_gb = Math.Ceiling(ram / 1024d);

                if (ram_gb > 16)
                {
                    ram_gb = 16;
                }

                var ram_mb = ram_gb * 1024;

                MaximumRam = ram_mb;
            };
            Dispatcher.BeginInvoke(dlg, System.Windows.Threading.DispatcherPriority.Render);
        }

        public void SetSettings(params object[] arg)
        {
            Action dlg = () =>
            {
                ram_slider.Value = (int)arg[0];
                autoconnect_control.IsChecked = (bool)arg[1];
                fullscreen_control.IsChecked = (bool)arg[2];
                debugging_control.IsChecked = (bool)arg[3];
                usemetrica_control.IsChecked = (bool)arg[4];
                path_control.Text = (string)arg[5];
            };
            Dispatcher.BeginInvoke(dlg, System.Windows.Threading.DispatcherPriority.Render);
        }

        private void Buttons_Click(object sender, RoutedEventArgs e)
        {
            //var tag = ((Button)sender).Tag as string;
            //switch (tag)
            //{
            //    case "apply":
            //        LauncherData.API_OnApplySettigns((int)ram_slider.Value,
            //            autoconnect_control.IsChecked.Value,
            //            fullscreen_control.IsChecked.Value,
            //            debugging_control.IsChecked.Value,
            //            usemetrica_control.IsChecked.Value,
            //            path_control.Text);
            //        break;
            //    case "delete":
            //        LauncherData.API_OnDelete();
            //        break;
            //}
        }

        private void TextBlock_Click(object sender, MouseButtonEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            dialog.SelectedPath = ((TextBlock)sender).Text;

            if (dialog.ShowDialog().GetValueOrDefault())
            {
                ((TextBlock)sender).Text = dialog.SelectedPath;
            }
        }

        #region [INotifyPropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
        private void RmSldr_KeyUp(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            switch (e.Key)
            {
                case Key.Left:
                    {
                        var current = CurrentRam;
                        var isFind = current % 1024 != 0;
                        if (isFind)
                        {
                            int tmp, number = current;
                            if ((tmp = number % 1024) != 0)
                                number += number > -1 ? (1024 - tmp) : -tmp;

                            CurrentRam = number - 1024;
                        }
                        else
                        {
                            CurrentRam -= 1024;
                        }
                    }
                    break;
                case Key.Right:
                    {
                        var current = CurrentRam;
                        var isFind = current % 1024 != 0;
                        if (isFind)
                        {
                            int tmp, number = current;
                            if ((tmp = number % 1024) != 0)
                                number += number > -1 ? (1024 - tmp) : -tmp;

                            CurrentRam = number + 1024;
                        }
                        else
                        {
                            CurrentRam += 1024;
                        }
                    }
                    break;
            }
        }
    }
}
