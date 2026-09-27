using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winui_portfolio.Models
{
    public class AssetType
    {
        public string Name { get; set; } = string.Empty;

        public bool IsValOnly { get; set; } = false;

        public bool IsRetirement { get; set; } = false;

        public bool IsParent { get; set; } = false;

        public bool IsDebt { get; set; } = false;

        public bool SysRec { get; set; } = false;

        public bool IsReadOnly => SysRec;

        public AssetType() { }
    }
}
