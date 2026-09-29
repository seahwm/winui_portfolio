using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using winui_portfolio.Models;
using winui_portfolio.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace winui_portfolio.Views
{
    // A simple view model for a metric card
    public class MetricViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Change { get; set; } = string.Empty;
        public Brush ValueBrush { get; set; } = new SolidColorBrush(Colors.Black); // Default color
        public Brush ChangeBrush { get; set; } = new SolidColorBrush(Colors.Gray); // Default color
        public Visibility ChangeVisibility => string.IsNullOrEmpty(Change) ? Visibility.Collapsed : Visibility.Visible;
        public string Tooltip { get; set; } = string.Empty;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Dashboard : Page
    {
        /// <summary>
        /// 个股持仓分布饼图显示的最大数量。
        /// 设为 0 时：显示全部股票比例（方便全面观察并调仓）；
        /// 设为具体正数（如 5、10）时：仅显示前 N 只重仓股，其余自动合并为“其他 (Others)”。
        /// </summary>
        public int MaxHoldingsDisplayCount { get; set; } = 0;

        public Dashboard()
        {
            InitializeComponent();
            SetupSampleCharts();
            LoadDashboardMetrics();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            LoadDashboardMetrics();
            UpdateLineChart();
            UpdateStockPieCharts();
        }

        private void LoadDashboardMetrics()
        {
            // 使用最新的 snapshot（按日期倒序取第一个）
            var latestSnapshot = ApplicationDataService.Snapshots
                .OrderByDescending(s => s.Date)
                .FirstOrDefault();

            if (latestSnapshot == null)
            {
                TotalAssetsTextBlock.Text = "RM 0.00";
                TotalDebtTextBlock.Text = "RM 0.00";
                NetWorthTextBlock.Text = "RM 0.00";
                StockAssetsTextBlock.Text = "RM 0.00";
                StockReturnsTextBlock.Text = "RM 0.00";
                LiquidityWithoutDebtTextBlock.Text = "RM 0.00";
                LiquidityWithoutParentsWithoutDebtTextBlock.Text = "RM 0.00";
                RetirementAssetTextBlock.Text = "RM 0.00";
                return;
            }

            decimal totalWorth = SnapshotService.CalculateTotalWorth(latestSnapshot);
            TotalAssetsTextBlock.Text = $"RM {totalWorth:N2}";
            TotalDebtTextBlock.Text = $"RM {SnapshotService.CalculateTotalLiability(latestSnapshot):N2}";
            NetWorthTextBlock.Text = $"RM {SnapshotService.CalculateTotalNetWorth(latestSnapshot):N2}";

            decimal stockAssets = SnapshotService.CalculateStockAssets(latestSnapshot);
            StockAssetsTextBlock.Text = $"RM {stockAssets:N2}";
            if (StockAssetsRatioTextBlock != null)
            {
                StockAssetsRatioTextBlock.Text = totalWorth > 0
                    ? $"占总资产 {(stockAssets / totalWorth):P1}"
                    : "占总资产 0.0%";
            }

            decimal stockReturn = SnapshotService.CalculateStockReturn(latestSnapshot);
            if (stockReturn >= 0)
            {
                StockReturnsTextBlock.Text = $"+RM {stockReturn:N2}";
                StockReturnsTextBlock.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 124, 65));
            }
            else
            {
                StockReturnsTextBlock.Text = $"-RM {Math.Abs(stockReturn):N2}";
                StockReturnsTextBlock.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 232, 17, 35));
            }

            decimal stockProfitRate = SnapshotService.CalculateStockAssetsProfitRate(latestSnapshot);
            if (StockReturnsBadgeBorder != null && StockReturnsBadgeTextBlock != null)
            {
                string sign = stockProfitRate >= 0 ? "+" : "";
                StockReturnsBadgeTextBlock.Text = $"{sign}{stockProfitRate:F2}%";
                StockReturnsBadgeBorder.Visibility = Visibility.Visible;
                if (stockProfitRate >= 0)
                {
                    StockReturnsBadgeBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 230, 244, 234));
                    StockReturnsBadgeTextBlock.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 124, 65));
                }
                else
                {
                    StockReturnsBadgeBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 253, 231, 233));
                    StockReturnsBadgeTextBlock.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 232, 17, 35));
                }
            }

            LiquidityWithoutDebtTextBlock.Text = $"RM {SnapshotService.CalculateLiquidityAssetWithoutDebt(latestSnapshot):N2}";
            LiquidityWithoutParentsWithoutDebtTextBlock.Text = $"RM {SnapshotService.CalculateLiquidityAssetWithoutParentsWithoutDebt(latestSnapshot):N2}";
            RetirementAssetTextBlock.Text = $"RM {SnapshotService.CalculateRetirementAsset(latestSnapshot):N2}";
        }

        private void AssetTrendComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateLineChart();
        }

        private void UpdateLineChart()
        {
            if (AssetHistoryChart == null)
            {
                return;
            }

            var snapshots = ApplicationDataService.Snapshots
                .OrderBy(s => s.Date)
                .ToList();

            if (snapshots.Count == 0)
            {
                AssetHistoryChart.Series = Array.Empty<ISeries>();
                return;
            }

            var values = new List<double>();
            var labels = new List<string>();

            int selectedIndex = AssetTrendComboBox?.SelectedIndex ?? 0;
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            string seriesName = "净资产 (Net Worth)";
            SKColor strokeColor = SKColor.Parse("#0078D4");
            SKColor fillColor = SKColor.Parse("#1A0078D4");
            bool isPercentage = false;

            switch (selectedIndex)
            {
                case 0: // 净资产 (Net Worth)
                    seriesName = "净资产 (Net Worth)";
                    strokeColor = SKColor.Parse("#0078D4");
                    fillColor = SKColor.Parse("#1A0078D4");
                    isPercentage = false;
                    foreach (var snapshot in snapshots)
                    {
                        labels.Add(snapshot.Date.ToShortDateString());
                        values.Add((double)SnapshotService.CalculateTotalNetWorth(snapshot));
                    }
                    break;

                case 1: // 股票资产 (Stock Assets)
                    seriesName = "股票资产 (Stock Assets)";
                    strokeColor = SKColor.Parse("#5C2D91");
                    fillColor = SKColor.Parse("#1A5C2D91");
                    isPercentage = false;
                    foreach (var snapshot in snapshots)
                    {
                        labels.Add(snapshot.Date.ToShortDateString());
                        values.Add((double)SnapshotService.CalculateStockAssets(snapshot));
                    }
                    break;

                case 2: // 流动资产 (无父母/无债务)
                    seriesName = "流动资产 (无父母/无债务)";
                    strokeColor = SKColor.Parse("#008272");
                    fillColor = SKColor.Parse("#1A008272");
                    isPercentage = false;
                    foreach (var snapshot in snapshots)
                    {
                        labels.Add(snapshot.Date.ToShortDateString());
                        values.Add((double)SnapshotService.CalculateLiquidityAssetWithoutParentsWithoutDebt(snapshot));
                    }
                    break;

                case 3: // 股票收益率 (Stock Return Rate)
                    seriesName = "股票收益率 (Stock Return Rate)";
                    strokeColor = SKColor.Parse("#107C41");
                    fillColor = SKColor.Parse("#1A107C41");
                    isPercentage = true;
                    foreach (var snapshot in snapshots)
                    {
                        labels.Add(snapshot.Date.ToShortDateString());
                        values.Add((double)SnapshotService.CalculateStockAssetsProfitRate(snapshot));
                    }
                    break;

                case 4: // 父母投资资产 (Parents Investment Assets)
                    seriesName = "父母投资资产 (Parents Investment Assets)";
                    strokeColor = SKColor.Parse("#CA5010");
                    fillColor = SKColor.Parse("#1ACA5010");
                    isPercentage = false;
                    foreach (var snapshot in snapshots)
                    {
                        labels.Add(snapshot.Date.ToShortDateString());
                        values.Add((double)SnapshotService.CalculateParentalInvestment(snapshot));
                    }
                    break;

                case 5: // 父母投资收益率 (Parents Return Rate)
                    seriesName = "父母投资收益率 (Parents Return Rate)";
                    strokeColor = SKColor.Parse("#D13438");
                    fillColor = SKColor.Parse("#1AD13438");
                    isPercentage = true;
                    foreach (var snapshot in snapshots)
                    {
                        labels.Add(snapshot.Date.ToShortDateString());
                        values.Add((double)SnapshotService.CalculateParentalInvestmentProfitRate(snapshot));
                    }
                    break;
            }

            AssetHistoryChart.Series = new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = seriesName,
                    Values = values,
                    Stroke = new SolidColorPaint(strokeColor) { StrokeThickness = 3 },
                    Fill = isPercentage ? null : new SolidColorPaint(fillColor),
                    GeometrySize = 7,
                    LineSmoothness = 0.5,
                    YToolTipLabelFormatter = point => isPercentage
                        ? $"{point.Coordinate.PrimaryValue:F2}%"
                        : $"RM {point.Coordinate.PrimaryValue:N2}"
                }
            };

            AssetHistoryChart.XAxes = new Axis[]
            {
                new Axis
                {
                    Labels = labels,
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            };

            AssetHistoryChart.YAxes = new Axis[]
            {
                new Axis
                {
                    Labeler = value => isPercentage ? $"{value:0.##}%" : "RM " + value.ToString("N2"),
                    LabelsPaint = new SolidColorPaint(SKColors.Gray)
                }
            };
        }

        private void SetupSampleCharts()
        {
            // 1. 折线图：资产变化趋势 (Line Chart)
            UpdateLineChart();

            // 2, 3, 4. 饼图：股票分布剖析 (Pie Charts)
            UpdateStockPieCharts();
        }

        private void UpdateStockPieCharts()
        {
            if (MarketPieChart == null || StockPieChart == null || StockTypePieChart == null)
            {
                return;
            }

            // 使用最新的 snapshot（按日期倒序取第一个）
            var latestSnapshot = ApplicationDataService.Snapshots
                .OrderByDescending(s => s.Date)
                .FirstOrDefault();

            if (latestSnapshot == null || latestSnapshot.Assets == null)
            {
                MarketPieChart.Series = Array.Empty<ISeries>();
                StockPieChart.Series = Array.Empty<ISeries>();
                StockTypePieChart.Series = Array.Empty<ISeries>();
                return;
            }

            var allStock = new List<Stock>();
            foreach (var a in latestSnapshot.Assets)
            {
                if (a.IsStock && a.Stocks != null)
                {
                    foreach (var s in a.Stocks)
                    {
                        allStock.Add(s);
                    }
                }
            }

            // 计算每只股票折算成基准货币(MYR)的价值
            decimal totalVal = Decimal.Zero;
            var stockValList = new List<(Stock Stock, decimal ConvertedValue)>();

            foreach (var s in allStock)
            {
                var val = (s.Currency != null && s.Currency.Equals("USD", StringComparison.OrdinalIgnoreCase))
                    ? s.TotalValue * latestSnapshot.UsdRate
                    : s.TotalValue;

                if (val > 0)
                {
                    totalVal += val;
                    stockValList.Add((s, val));
                }
            }

            if (totalVal <= 0 || stockValList.Count == 0)
            {
                MarketPieChart.Series = Array.Empty<ISeries>();
                StockPieChart.Series = Array.Empty<ISeries>();
                StockTypePieChart.Series = Array.Empty<ISeries>();
                return;
            }

            // 2. 饼图：股票市场分布 (Market Allocation)
            // 按货币(市场)汇总折算后的基准货币价值
            var marketMap = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in stockValList)
            {
                string curr = string.IsNullOrWhiteSpace(item.Stock.Currency) ? "MYR" : item.Stock.Currency.Trim().ToUpperInvariant();
                string marketLabel = curr switch
                {
                    "USD" => "美股 (USD)",
                    "MYR" => "马股 (MYR)",
                    "HKD" => "港股 (HKD)",
                    "CNY" or "RMB" => "A股 (CNY)",
                    "SGD" => "新股 (SGD)",
                    _ => $"{curr} (其他)"
                };

                if (!marketMap.ContainsKey(marketLabel))
                {
                    marketMap[marketLabel] = Decimal.Zero;
                }
                marketMap[marketLabel] += item.ConvertedValue;
            }

            MarketPieChart.Series = marketMap
                .OrderByDescending(kv => kv.Value)
                .Select(kv => new PieSeries<double>
                {
                    Name = kv.Key,
                    Values = new double[] { (double)kv.Value },
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 12,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"{point.Context.Series.Name} {point.StackedValue!.Share:P1}",
                    ToolTipLabelFormatter = point => $"{point.Context.Series.Name}: {point.StackedValue!.Share:P1} (RM {point.Coordinate.PrimaryValue:N2})"
                })
                .ToArray();

            // 3. 饼图：个股持仓分布 (Holdings Allocation)
            // 按代码或名称聚合持仓价值；采用右侧图例 (Legend) 展示名称与占比
            StockPieChart.LegendPosition = LegendPosition.Right;

            var stockMap = new Dictionary<string, decimal>();
            foreach (var item in stockValList)
            {
                string stockKey = !string.IsNullOrWhiteSpace(item.Stock.Name)
                    ? item.Stock.Name.Trim()
                    : (!string.IsNullOrWhiteSpace(item.Stock.Symbol) ? item.Stock.Symbol.Trim() : "未知股票");

                if (!stockMap.ContainsKey(stockKey))
                {
                    stockMap[stockKey] = Decimal.Zero;
                }
                stockMap[stockKey] += item.ConvertedValue;
            }

            var orderedStocks = stockMap.OrderByDescending(kv => kv.Value).ToList();
            var stockSeriesList = new List<ISeries>();

            // 若 MaxHoldingsDisplayCount > 0 且持仓数超出限制，取前 N 只，其余汇总为“其他”；若为 0 则展示全部股票比例
            if (MaxHoldingsDisplayCount > 0 && orderedStocks.Count > MaxHoldingsDisplayCount)
            {
                var topStocks = orderedStocks.Take(MaxHoldingsDisplayCount);
                var otherStocksVal = orderedStocks.Skip(MaxHoldingsDisplayCount).Sum(kv => kv.Value);

                foreach (var top in topStocks)
                {
                    decimal share = totalVal > 0 ? (top.Value / totalVal) : 0;
                    stockSeriesList.Add(new PieSeries<double>
                    {
                        Name = $"{top.Key} ({share:P1})",
                        Values = new double[] { (double)top.Value },
                        ToolTipLabelFormatter = point => $"{top.Key}: {point.StackedValue!.Share:P1} (RM {point.Coordinate.PrimaryValue:N2})"
                    });
                }

                if (otherStocksVal > 0)
                {
                    decimal otherShare = totalVal > 0 ? (otherStocksVal / totalVal) : 0;
                    stockSeriesList.Add(new PieSeries<double>
                    {
                        Name = $"其他 ({otherShare:P1})",
                        Values = new double[] { (double)otherStocksVal },
                        ToolTipLabelFormatter = point => $"其他: {point.StackedValue!.Share:P1} (RM {point.Coordinate.PrimaryValue:N2})"
                    });
                }
            }
            else
            {
                foreach (var s in orderedStocks)
                {
                    decimal sShare = totalVal > 0 ? (s.Value / totalVal) : 0;
                    stockSeriesList.Add(new PieSeries<double>
                    {
                        Name = $"{s.Key} ({sShare:P1})",
                        Values = new double[] { (double)s.Value },
                        ToolTipLabelFormatter = point => $"{s.Key}: {point.StackedValue!.Share:P1} (RM {point.Coordinate.PrimaryValue:N2})"
                    });
                }
            }
            StockPieChart.Series = stockSeriesList.ToArray();

            // 4. 饼图：股票类型风格 (Stock Style Allocation)
            // 按股票类型风格汇总价值，直接在扇区上显示名称与占比
            var typeMap = new Dictionary<string, decimal>();
            foreach (var item in stockValList)
            {
                string typeKey = !string.IsNullOrWhiteSpace(item.Stock.StockType)
                    ? item.Stock.StockType.Trim()
                    : "未分类 (Unclassified)";

                if (!typeMap.ContainsKey(typeKey))
                {
                    typeMap[typeKey] = Decimal.Zero;
                }
                typeMap[typeKey] += item.ConvertedValue;
            }

            StockTypePieChart.Series = typeMap
                .OrderByDescending(kv => kv.Value)
                .Select(kv => new PieSeries<double>
                {
                    Name = kv.Key,
                    Values = new double[] { (double)kv.Value },
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 12,
                    DataLabelsPosition = PolarLabelsPosition.Middle,
                    DataLabelsFormatter = point => $"{point.Context.Series.Name} {point.StackedValue!.Share:P1}",
                    ToolTipLabelFormatter = point => $"{point.Context.Series.Name}: {point.StackedValue!.Share:P1} (RM {point.Coordinate.PrimaryValue:N2})"
                })
                .ToArray();
        }
    }
}
