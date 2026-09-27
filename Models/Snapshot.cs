using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winui_portfolio.Models
{
    public class Snapshot
    {
        public DateOnly Date { get; set; }

        public Decimal UsdRate { get; set; } = Decimal.Zero;
        public ObservableCollection<Asset> Assets { get; set; } = [];

        public Snapshot()
        {

        }
    }
}
