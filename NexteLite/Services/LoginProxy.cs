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
        public LoginFunction Core_Login { get; set; }

        /// <summary>
        /// Функция позволяющая разлогиниться в лаунчере
        /// </summary>
        public LogoutFunction Core_Logout { get; set; }
    }
}
