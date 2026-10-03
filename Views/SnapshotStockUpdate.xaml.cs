using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using winui_portfolio.Models;
using winui_portfolio.Services;

namespace winui_portfolio.Views
{
    public sealed partial class SnapshotStockUpdate : Page, IDirtyCheckable
    {
        public static SnapshotStockUpdate? Current { get; private set; }
        public Snapshot? Snapshot { get; set; }
        public Asset? Asset { get; set; }

        public ObservableCollection<Stock> stocks { get; set; } = new ObservableCollection<Stock>();

        private bool _reselect = false;

        public SnapshotStockUpdate()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Current = this;
            if (e.Parameter is SnapshotStockParameter param)
            {
                Snapshot = param.Snapshot;
                Asset = param.Asset;

                if (Asset != null)
                {
                    AssetTitleTextBlock.Text = !string.IsNullOrWhiteSpace(Asset.Name) ? $"{Asset.Name} - 股票详情" : "股票详情";
                }

                if (Snapshot != null)
                {
                    DateTextBlock.Text = $"({Snapshot.Date:yyyy-MM-dd})";
                }

                RefreshStocksList();
            }
        }

        private void RefreshStocksList()
        {
            stocks.Clear();
            if (Asset?.Stocks != null)
            {
                foreach (var s in Asset.Stocks)
                {
                    stocks.Add(s);
                }
            }

            StockCountTextBlock.Text = $"{stocks.Count} 只股票";
            UpdateCashSummary();
        }

        private void UpdateCashSummary()
        {
            if (Asset == null || Snapshot == null) return;

            decimal usdRate = Snapshot.UsdRate > 0 ? Snapshot.UsdRate : 1m;
            bool hasUsd = (Asset.Currency == "USD");

            // 1. 账户总资金 (折算成 MYR)
            decimal accountTotalMyr = (Asset.Currency == "USD") ? (Asset.Value * usdRate) : Asset.Value;

            // 2. 股票总市值 (每只股票折算成 MYR)
            decimal stocksTotalMyr = 0m;
            if (Asset.Stocks != null)
            {
                foreach (var stock in Asset.Stocks)
                {
                    decimal stockVal = stock.Units * stock.CurrentPrice;
                    if (stock.Currency == "USD")
                    {
                        hasUsd = true;
                        stocksTotalMyr += stockVal * usdRate;
                    }
                    else
                    {
                        stocksTotalMyr += stockVal;
                    }
                }
            }

            // 3. 闲置现金 (MYR)
            decimal idleCashMyr = accountTotalMyr - stocksTotalMyr;

            // 4. 现金占比
            decimal cashRate = accountTotalMyr > 0 ? (idleCashMyr / accountTotalMyr) * 100 : 0m;

            // 5. 更新 UI 显示
            AccountTotalValueTextBlock.Text = $"RM {accountTotalMyr:N2}";
            StocksTotalValueTextBlock.Text = $"RM {stocksTotalMyr:N2}";
            IdleCashValueTextBlock.Text = $"RM {idleCashMyr:N2}";

            if (hasUsd)
            {
                UsdRateHintTextBlock.Text = $"(含USD折算, 汇率: {usdRate:F2})";
                UsdRateHintTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                UsdRateHintTextBlock.Text = string.Empty;
                UsdRateHintTextBlock.Visibility = Visibility.Collapsed;
            }

            if (idleCashMyr < 0)
            {
                IdleCashValueTextBlock.Foreground = LossBrush;
                CashRatioTextBlock.Foreground = LossBrush;
                CashRatioTextBlock.Text = $"透支 {Math.Abs(cashRate):F1}%";
                CashRatioBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(30, 232, 17, 35));
            }
            else
            {
                IdleCashValueTextBlock.Foreground = ProfitBrush;
                CashRatioTextBlock.Foreground = NeutralBrush;
                CashRatioTextBlock.Text = $"占比 {cashRate:F1}%";
                CashRatioBadge.Background = (Brush)Application.Current.Resources["SubtleFillColorSecondaryBrush"];
            }
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (DetailFrame.Content is IDirtyCheckable dirtyCheckable)
            {
                var isDirty = await dirtyCheckable.IsDirtyAsync();
                if (isDirty)
                {
                    var result = await DialogService.Confirm(this.XamlRoot, "未保存", "还有没有保存的内容", "继续", "取消");
                    if (!result)
                    {
                        return;
                    }
                }
            }

            // 返回二级页面 SnapshotUpdate
            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                MainWindow.Current?.MainContentFrame.Navigate(typeof(SnapshotUpdate), Snapshot);
            }
        }

        private void SaveSnapshot_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataService.SaveSnapshots();
        }

        private async void StockListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_reselect)
            {
                _reselect = false;
                return;
            }

            if (DetailFrame.Content is IDirtyCheckable dirtyCheckable)
            {
                var isDirty = await dirtyCheckable.IsDirtyAsync();
                if (isDirty)
                {
                    var result = await DialogService.Confirm(this.XamlRoot, "未保存", "还有没有保存的内容", "继续", "取消");
                    if (result)
                    {
                        NavigateToStockDetail();
                    }
                    else
                    {
                        _reselect = true;
                        StockListBox.SelectedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
                    }
                }
                else
                {
                    NavigateToStockDetail();
                }
            }
            else
            {
                NavigateToStockDetail();
            }
        }

        private void NavigateToStockDetail()
        {
            if (StockListBox.SelectedItem is Stock selectedStock)
            {
                DetailFrame.Navigate(typeof(StockDetail), new StockDetailParameter
                {
                    Stock = selectedStock,
                    Asset = Asset,
                    Snapshot = Snapshot
                });

                if (DetailFrame.Content is StockDetail detailPage)
                {
                    detailPage.OnStockChanged = () =>
                    {
                        RefreshStocksList();
                    };
                }
            }
        }

        private async void AddStock_Click(object sender, RoutedEventArgs e)
        {
            if (DetailFrame.Content is IDirtyCheckable dirtyCheckable)
            {
                var isDirty = await dirtyCheckable.IsDirtyAsync();
                if (isDirty)
                {
                    var result = await DialogService.Confirm(this.XamlRoot, "未保存", "还有没有保存的内容", "继续", "取消");
                    if (result)
                    {
                        NavigateToStockAdd();
                    }
                }
                else
                {
                    NavigateToStockAdd();
                }
            }
            else
            {
                NavigateToStockAdd();
            }
        }

        private void NavigateToStockAdd()
        {
            StockListBox.SelectedItem = null;
            DetailFrame.Navigate(typeof(StockAdd), new SnapshotStockParameter
            {
                Snapshot = Snapshot,
                Asset = Asset
            });

            if (DetailFrame.Content is StockAdd addPage)
            {
                addPage.OnStockAdded = (newStock) =>
                {
                    RefreshStocksList();
                    DetailFrame.Content = null;
                };
            }
        }

        public async Task<bool> IsDirtyAsync()
        {
            if (DetailFrame.Content is IDirtyCheckable detailDirty)
            {
                if (await detailDirty.IsDirtyAsync())
                {
                    return true;
                }
            }
            return false;
        }

        private static readonly SolidColorBrush ProfitBrush = new(Windows.UI.Color.FromArgb(255, 16, 124, 65));
        private static readonly SolidColorBrush LossBrush = new(Windows.UI.Color.FromArgb(255, 232, 17, 35));
        private static readonly SolidColorBrush NeutralBrush = new(Windows.UI.Color.FromArgb(255, 128, 128, 128));

        public static string FormatCurrency(decimal val)
        {
            return val.ToString("N2");
        }

        public static string GetStockWeightText(decimal units, decimal currentPrice, string currency)
        {
            if (Current?.Asset == null) return "占比 0.0%";
            decimal usdRate = Current.Snapshot?.UsdRate > 0 ? Current.Snapshot.UsdRate : 1m;
            decimal accountTotalMyr = (Current.Asset.Currency == "USD") ? (Current.Asset.Value * usdRate) : Current.Asset.Value;
            if (accountTotalMyr <= 0) return "占比 0.0%";

            decimal stockVal = units * currentPrice;
            decimal stockValMyr = (currency == "USD") ? (stockVal * usdRate) : stockVal;
            decimal weight = (stockValMyr / accountTotalMyr) * 100m;
            return $"占比 {weight:F1}%";
        }

        public static string GetProfitLossAmountAndRateText(decimal profitLoss, decimal profitLossRate, string currency)
        {
            if (profitLoss > 0)
            {
                return $"+{currency} {profitLoss:N2} (+{profitLossRate:F2}%)";
            }
            else if (profitLoss < 0)
            {
                return $"-{currency} {Math.Abs(profitLoss):N2} ({profitLossRate:F2}%)";
            }
            else
            {
                return $"{currency} 0.00 (0.00%)";
            }
        }

        public static SolidColorBrush GetAmountBrush(decimal amount)
        {
            if (amount > 0) return ProfitBrush;
            if (amount < 0) return LossBrush;
            return NeutralBrush;
        }

        public static string GetProfitLossRateText(decimal cost, decimal value)
        {
            if (cost <= 0) return string.Empty;
            decimal rate = ((value - cost) / cost) * 100m;
            if (rate > 0) return $"(+{rate:F2}%)";
            if (rate < 0) return $"({rate:F2}%)";
            return "(0.00%)";
        }

        public static SolidColorBrush GetProfitLossRateBrush(decimal cost, decimal value)
        {
            if (cost <= 0) return NeutralBrush;
            if (value > cost) return ProfitBrush;
            if (value < cost) return LossBrush;
            return NeutralBrush;
        }

        public static Visibility GetRemarkVisibility(string? remark)
        {
            return string.IsNullOrWhiteSpace(remark) ? Visibility.Collapsed : Visibility.Visible;
        }

        public static Visibility GetAccumulatedProfitVisibility(decimal accumulatedProfit)
        {
            return accumulatedProfit != 0m ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// Parameter class for navigating between SnapshotUpdate and SnapshotStockUpdate
    /// </summary>
    public class SnapshotStockParameter
    {
        public Snapshot? Snapshot { get; set; }
        public Asset? Asset { get; set; }
    }
}
