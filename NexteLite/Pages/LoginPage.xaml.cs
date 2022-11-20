using NexteLite.Interfaces;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page, IPage, ILoginProxy, INotifyPropertyChanged
    {
        public event LoginClickHandler LoginClick;

        public PageType Id => PageType.Login;
        public bool IsOverlay { get; private set; }

        string _Username;
        public string Username
        {
            get
            {
                return _Username;
            }
            set
            {
                _Username = value;
                OnPropertyChanged();
            }
        }

        public string Password { get; private set; }

        public LoginPage()
        {
            InitializeComponent();
            IsOverlay = false;
        }

        public void SetParams(IParamsLoginPage data)
        {
            Username = data.Username;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        public void ValidateLoginError(string error)
        {
            ValidationError validationError_login = new ValidationError(new DataErrorValidationRule(), LoginInput);

            validationError_login.ErrorContent = error;

            Validation.MarkInvalid(LoginInput.GetBindingExpression(TextBox.TextProperty), validationError_login);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        public void ValidatePassError(string error)
        {
            ValidationError validationError_password = new ValidationError(new DataErrorValidationRule(), PasswordInput);

            validationError_password.ErrorContent = error;

            Validation.MarkInvalid(PasswordInput.GetBindingExpression(PasswordBox.TagProperty), validationError_password);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool Validate()
        {
            bool error_login = false;
            bool error_password = false;
            ValidationError validationError_login = new ValidationError(new DataErrorValidationRule(), LoginInput);
            ValidationError validationError_password = new ValidationError(new DataErrorValidationRule(), PasswordInput);


            if (string.IsNullOrEmpty(LoginInput.Text))
            {
                error_login = true;
                validationError_login.ErrorContent = "Данное поле не может быть пустым";
            }

            if (string.IsNullOrEmpty(PasswordInput.Password))
            {
                error_password = true;
                validationError_password.ErrorContent = "Данное поле не может быть пустым";
            }

            if (error_login)
            {
                Validation.MarkInvalid(LoginInput.GetBindingExpression(TextBox.TextProperty), validationError_login);
            }
            else
            {
                Validation.ClearInvalid(LoginInput.GetBindingExpression(TextBox.TextProperty));
            }

            if (error_password)
            {
                Validation.MarkInvalid(PasswordInput.GetBindingExpression(PasswordBox.TagProperty), validationError_password);
            }
            else
            {
                Validation.ClearInvalid(PasswordInput.GetBindingExpression(PasswordBox.TagProperty));
            }

            if (!error_login && !error_password)
            {
                return true;
            }

            return false;
        }


        private void Strbrd_Welcome_Completed(object sender, EventArgs e)
        {
            WelcomLabel.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Login_Click(object sender, RoutedEventArgs e)
        {
            if (Validate())
            {
                LoginClick?.Invoke(Username, Password, false);
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            { Password = ((PasswordBox)sender).Password; }
        }

        #region [INotifyPropertyChanged]
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        #endregion

        bool IsLoaded = false;

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                Storyboard sb = MainGrid.FindResource("Welcome") as Storyboard;
                sb.Begin(this);

                IsLoaded = true;
            }
        }

        public void LoginError(string message)
        {
            ValidateLoginError(message);
            ValidatePassError(message);
        }
    }
}
