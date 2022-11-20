using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate void LoginClickHandler(string username, string password, bool save);

    public interface ILoginProxy
    {
        event LoginClickHandler LoginClick;
        void SetParams(IParamsLoginPage data);
        void LoginError(string message);
    }

}
