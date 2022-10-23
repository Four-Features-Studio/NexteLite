using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public interface IParamsLoginPage
    {
        string Username { get; }
        string Password { get; }
        bool SavePassword { get; }
    }
}
