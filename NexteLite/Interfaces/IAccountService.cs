using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public interface IAccountService
    {
        bool AuthAccount(string username, string password, out Profile profile, ref string message);
        bool Logout();

        string GetUsername();
        string GetServerToken();
        string GetAccessToken();
        string GetUuid();

    }
}
