using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winui_portfolio.Views
{
    internal interface IDirtyCheckable
    {
        Task<bool> IsDirtyAsync();
    }
}
