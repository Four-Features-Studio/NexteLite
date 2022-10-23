using NexteLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public delegate bool LoginClickHandler(string username, string password, bool save, out string message);

    public interface ILoginProxy
    {
        event LoginClickHandler LoginClick;
    }

}
