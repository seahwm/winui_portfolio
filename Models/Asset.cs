using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace winui_portfolio.Models
{
    public class Asset
    {
        public String? Name { get; set; }

        public AssetType? AssetType { get; set; }

        public Decimal Cost { get; set; } = Decimal.Zero;

        public Decimal Value { get; set; } = Decimal.Zero;

        public String? Currency { get; set; }

        public string Remark { get; set; } = string.Empty;

        public List<Stock> Stocks { get; set; } = [];

        public bool IsStock => AssetType?.Name == "股票" || (AssetType?.Name != null && AssetType.Name.Contains("股票")) || Stocks.Count > 0;

        public Asset()
        {

        }
    }
}
