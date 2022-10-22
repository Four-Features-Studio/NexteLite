using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;

namespace NexteLite.Interfaces
{
    public interface IPagesRepository
    {
        Page GetPage(PageType type);
    }
}
