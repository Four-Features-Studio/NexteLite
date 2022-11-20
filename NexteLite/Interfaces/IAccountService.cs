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
        Task<(bool result, Profile profile, string message)> AuthAccount(string username, string password);
        bool Logout();

        string GetUsername();
        string GetServerToken();
        string GetAccessToken();
        string GetUuid();

    }
}
