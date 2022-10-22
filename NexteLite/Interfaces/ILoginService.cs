using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate bool LoginFunction(string username, string password, bool save, out string message);
    public delegate bool LogoutFunction();

    public interface ILoginProxy
    {
        LoginFunction Core_Login { get; set; }
        LogoutFunction Core_Logout { get; set; }
    }

}
