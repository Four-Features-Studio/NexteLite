using NexteLite.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace NexteLite.Models
{
    public class ParamsLoginPage : IParamsLoginPage
    {
        public ParamsLoginPage(string username, string password, bool savePassword)
        {
            Username = username;
            Password = password;
            SavePassword = savePassword;
        }

        public string Username { get; private set; }
        public string Password { get; private set; }
        public bool SavePassword { get; private set; }
    }
}
