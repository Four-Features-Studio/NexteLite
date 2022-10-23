using Microsoft.VisualBasic.Logging;
using NexteLite.Interfaces;
using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Services
{
    public class LoginProxy : ILoginProxy
    {
        /// <summary>
        /// Функция для авторизации посредством рест запроса
        /// </summary>
        public event LoginClickHandler LoginClick;

        public bool Login(string login, string password, bool save, out string message)
        {
            return LoginClick.Invoke(login, password, save, out message);
        }
    }
}
