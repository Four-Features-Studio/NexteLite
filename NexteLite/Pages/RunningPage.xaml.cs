﻿using NexteLite.Interfaces;
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
    /// Логика взаимодействия для RunningPage.xaml
    /// </summary>
    public partial class RunningPage : Page, IPage, IRunningProxy, INotifyPropertyChanged
    {
        public RunningPage()
        {
            InitializeComponent();
        }
        public event OnKillClientClickHandler OnKillClientClick;

        bool _IsDebugMode = false;
        public bool IsDebugMode
        {
            get
            {
                return _IsDebugMode;
            }
            set
            {
                _IsDebugMode = value;
                OnPropertyChanged();
            }
        }
        public void WriteLog(string log)
        {
            Action dlg = () =>
            {
                PATH_Log.Text += log + Environment.NewLine;
                PATH_Scroll.ScrollToBottom();
            };
            Dispatcher.BeginInvoke(dlg, System.Windows.Threading.DispatcherPriority.Render);
        }
        public void SetParams(bool isDebug)
        {
            IsDebugMode = isDebug;
        }
        public PageType Id => PageType.Running;

        public bool IsOverlay => false;

        #region [INotifyPropertyChanged]

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion

        private void KillClient_Click(object sender, RoutedEventArgs e)
        {
            OnKillClientClick?.Invoke();
        }
    }
}
