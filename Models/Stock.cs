using System;

namespace winui_portfolio.Models
{
    public class Stock
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name { get; set; } = string.Empty;

        public string Symbol { get; set; } = string.Empty;

        public string StockType { get; set; } = string.Empty;

        public decimal Units { get; set; } = Decimal.Zero;

        public decimal AvgPrice { get; set; } = Decimal.Zero;

        public decimal CurrentPrice { get; set; } = Decimal.Zero;

        public decimal DividendYield { get; set; } = Decimal.Zero;

        public decimal AccumulatedProfit { get; set; } = Decimal.Zero;

        // 向后兼容旧名称
        public decimal AccumulatedDividend
        {
            get => AccumulatedProfit;
            set => AccumulatedProfit = value;
        }

        public string Currency { get; set; } = "MYR";

        public string Remark { get; set; } = string.Empty;

        // 便捷计算属性供UI绑定使用
        public decimal TotalCost => Units * AvgPrice;

        public decimal TotalValue => Units * CurrentPrice;

        public decimal ProfitLoss => TotalValue - TotalCost;

        public decimal ProfitLossRate => TotalCost > 0 ? (ProfitLoss / TotalCost) * 100 : Decimal.Zero;

        public Stock()
        {
        }
    }
}
