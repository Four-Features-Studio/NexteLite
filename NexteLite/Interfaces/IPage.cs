using NexteLite.Services.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexteLite.Interfaces
{
    public interface IPage
    {
        public PageType Id { get; }
        public bool IsOverlay { get; }
    }
}
