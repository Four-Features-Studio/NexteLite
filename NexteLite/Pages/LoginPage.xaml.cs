using NexteLite.Interfaces;
using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>
    public partial class LoginPage : Page, IPage, ILoginProxy
    {
        public event LoginClickHandler LoginClick;

        public PageType Id => PageType.Login;
        public bool IsOverlay { get; private set; }

        public LoginPage()
        {
            InitializeComponent();
            IsOverlay = false;
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
            var login = LoginInput.Text;
            var password = PasswordInput.Password;
            var save = save_userdata.IsChecked.Value;

            if (Validate())
            {

                if (!LoginClick.Invoke(login, password, save, out string message))
                {
                    ValidateLoginError(message);
                    ValidatePassError(message);
                }
            }
        }

        private bool Proxy_Login(string username, string password, bool save, out string message)
        {
            throw new NotImplementedException();
        }

    }
}
