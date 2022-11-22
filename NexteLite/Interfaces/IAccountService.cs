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
        void Logout();

        Profile GetProfile();
        string GetUsername();
        string GetAccessToken();
        string GetUuid();

    }
}
