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

        public static Decimal CalculateStockReturn(Snapshot snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            Decimal totalCost = Decimal.Zero;
            Decimal totalValue = Decimal.Zero;
            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType.Name.Equals("股票"))
                {
                    totalCost += asset.Cost;
                    totalValue += asset.Value;
                }

            }
            return totalValue - totalCost;
        }

        public static Decimal CalculateTotalWorth(Snapshot snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            Decimal totalWorth = Decimal.Zero;
            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType.IsDebt)
                {
                    continue;
                }
                if (asset.Currency.Equals("USD"))
                {
                    totalWorth += asset.Value * snapshot.UsdRate;

                }
                else
                {
                    totalWorth += asset.Value;
                }
            }
            return totalWorth;
        }

        public static Decimal CalculateTotalNetWorth(Snapshot snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            Decimal totalNetWorth = CalculateTotalWorth(snapshot) - CalculateTotalLiability(snapshot);

            return totalNetWorth;
        }

        public static Decimal CalculateTotalLiability(Snapshot snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }
            Decimal totalLiability = Decimal.Zero;
            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType.IsDebt)
                {
                    totalLiability += asset.Value;
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
                if (asset.AssetType.Name.Equals("股票"))
                {
                    stockAsset += asset.Value;
                }
            }
            return stockAsset;
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
                if (!asset.AssetType.IsRetirement)
                {
                    Decimal val = asset.AssetType.IsDebt ? -asset.Value : asset.Value;
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
                if (!asset.AssetType.IsRetirement && !asset.AssetType.IsParent)
                {
                    Decimal val = asset.AssetType.IsDebt ? -asset.Value : asset.Value;
                    rsl += val;
                }
            }
            return rsl;
        }

        // Dummy method: 请在此实现退休资产计算逻辑
        public static Decimal CalculateRetirementAsset(Snapshot? snapshot)
        {
            if (snapshot == null)
            {
                return Decimal.Zero;
            }

            Decimal rsl = Decimal.Zero;

            foreach (var asset in snapshot.Assets)
            {
                if (asset.AssetType.IsRetirement)
                {
                    rsl += asset.Value;
                }
            }
            return rsl;
        }
    }
}
