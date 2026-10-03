using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using winui_portfolio.Models;

namespace winui_portfolio.Services
{
    public static class SnapshotService
    {
        private static decimal ConvertToMyr(decimal amount, string? currency, decimal usdRate)
        {
            if (string.Equals(currency, "USD", StringComparison.OrdinalIgnoreCase))
            {
                return amount * (usdRate > 0 ? usdRate : 1m);
            }
            return amount;
        }

        public static Decimal CalculateStockReturn(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            Decimal totalCost = Decimal.Zero;
            Decimal totalValue = Decimal.Zero;
            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType != null && asset.AssetType.Name.Equals("股票"))
                {
                    totalCost += ConvertToMyr(asset.Cost, asset.Currency, snapshot.UsdRate);
                    totalValue += ConvertToMyr(asset.Value, asset.Currency, snapshot.UsdRate);
                }
            }
            return totalValue - totalCost;
        }

        public static Decimal CalculateTotalWorth(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            Decimal totalWorth = Decimal.Zero;
            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType != null && asset.AssetType.IsDebt)
                {
                    continue;
                }
                totalWorth += ConvertToMyr(asset.Value, asset.Currency, snapshot.UsdRate);
            }
            return totalWorth;
        }

        public static Decimal CalculateTotalNetWorth(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            Decimal totalNetWorth = CalculateTotalWorth(snapshot) - CalculateTotalLiability(snapshot);
            return totalNetWorth;
        }

        public static Decimal CalculateTotalLiability(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            Decimal totalLiability = Decimal.Zero;
            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType != null && asset.AssetType.IsDebt)
                {
                    totalLiability += ConvertToMyr(asset.Value, asset.Currency, snapshot.UsdRate);
                }
            }
            return totalLiability;
        }

        public static Decimal CalculateStockAssets(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            var assets = snapshot.Assets;
            Decimal stockAsset = Decimal.Zero;
            foreach (var asset in assets)
            {
                if (asset.AssetType != null && asset.AssetType.Name.Equals("股票"))
                {
                    stockAsset += ConvertToMyr(asset.Value, asset.Currency, snapshot.UsdRate);
                }
            }
            return stockAsset;
        }

        public static Decimal CalculateStockAssetsProfitRate(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            var assets = snapshot.Assets;

            Decimal stockAsset = Decimal.Zero;
            Decimal stockCost = Decimal.Zero;
            foreach (var asset in assets)
            {
                if (asset.AssetType != null && asset.AssetType.Name.Equals("股票"))
                {
                    stockAsset += ConvertToMyr(asset.Value, asset.Currency, snapshot.UsdRate);
                    stockCost += ConvertToMyr(asset.Cost, asset.Currency, snapshot.UsdRate);
                }
            }
            return stockCost > 0 ? (stockAsset - stockCost) / stockCost * 100 : Decimal.Zero;
        }

        public static Decimal CalculateLiquidityAssetWithoutDebt(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }

            Decimal liquidAsset = Decimal.Zero;

            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType != null && !asset.AssetType.IsRetirement)
                {
                    Decimal converted = ConvertToMyr(asset.Value, asset.Currency, snapshot.UsdRate);
                    Decimal val = asset.AssetType.IsDebt ? -converted : converted;
                    liquidAsset += val;
                }
            }
            return liquidAsset;
        }

        public static Decimal CalculateLiquidityAssetWithoutParentsWithoutDebt(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }

            Decimal rsl = Decimal.Zero;

            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType != null && !asset.AssetType.IsRetirement && !asset.AssetType.IsParent)
                {
                    Decimal converted = ConvertToMyr(asset.Value, asset.Currency, snapshot.UsdRate);
                    Decimal val = asset.AssetType.IsDebt ? -converted : converted;
                    rsl += val;
                }
            }
            return rsl;
        }

        public static Decimal CalculateParentalInvestmentProfitRate(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            Decimal rsl = Decimal.Zero;
            Decimal cost = Decimal.Zero;
            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType != null && !asset.AssetType.IsRetirement && asset.AssetType.IsParent)
                {
                    rsl += ConvertToMyr(asset.Value, asset.Currency, snapshot.UsdRate);
                    cost += ConvertToMyr(asset.Cost, asset.Currency, snapshot.UsdRate);
                }
            }
            return cost > 0 ? (rsl - cost) / cost * 100 : Decimal.Zero;
        }

        public static Decimal CalculateParentalInvestment(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            Decimal rsl = Decimal.Zero;
            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType != null && !asset.AssetType.IsRetirement && asset.AssetType.IsParent)
                {
                    rsl += ConvertToMyr(asset.Value, asset.Currency, snapshot.UsdRate);
                }
            }
            return rsl;
        }

        public static Decimal CalculateParentalInvestmentReturn(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            Decimal rsl = Decimal.Zero;
            Decimal cost = Decimal.Zero;
            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType != null && !asset.AssetType.IsRetirement && asset.AssetType.IsParent)
                {
                    rsl += ConvertToMyr(asset.Value, asset.Currency, snapshot.UsdRate);
                    cost += ConvertToMyr(asset.Cost, asset.Currency, snapshot.UsdRate);
                }
            }
            return rsl - cost;
        }

        public static Decimal CalculateRetirementAsset(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }

            Decimal rsl = Decimal.Zero;

            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType != null && asset.AssetType.IsRetirement)
                {
                    rsl += ConvertToMyr(asset.Value, asset.Currency, snapshot.UsdRate);
                }
            }
            return rsl;
        }
    }
}
