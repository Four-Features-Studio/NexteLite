using NexteLite.Interfaces;
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
    /// Логика взаимодействия для UpdatePage.xaml
    /// </summary>
    public partial class UpdatePage : Page, IPage, IUpdateProxy, INotifyPropertyChanged
    {
        public UpdatePage()
        {
            InitializeComponent();
        }

        string _TextMessage;
        public string TextMessage
        {
            get 
            { 
                return _TextMessage; 
            }
            set
            {
                _TextMessage = value;
                OnPropertyChanged();
            }
        }

        public PageType Id => PageType.Update;

        public bool IsOverlay => false;

        public void SetState(UpdateState state)
        {
            switch (state)
            {
                case UpdateState.Error:
                    TextMessage = Properties.Resources.lcl_txt_UpdatePage_NeedUpdate;
                    break;
                case UpdateState.Check:
                    TextMessage = Properties.Resources.lcl_txt_UpdatePage_CheckUpdate;
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
