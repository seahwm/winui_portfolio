using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using winui_portfolio.Models;
using winui_portfolio.Services;

namespace winui_portfolio.Views
{
    public sealed partial class StockDetail : Page, IDirtyCheckable
    {
        private Stock? stock;
        private Asset? parentAsset;
        private Snapshot? parentSnapshot;

        // 回调委托，通知父页面列表已更新
        public Action? OnStockChanged { get; set; }

        public StockDetail()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is StockDetailParameter param)
            {
                stock = param.Stock;
                parentAsset = param.Asset;
                parentSnapshot = param.Snapshot;

                if (stock != null)
                {
                    StockTypeComboBox.ItemsSource = ApplicationDataService.StockTypes;

                    TitleTextBlock.Text = !string.IsNullOrWhiteSpace(stock.Name) ? stock.Name : "股票详情";
                    StockTypeBadgeText.Text = !string.IsNullOrWhiteSpace(stock.StockType) ? stock.StockType : "未分类";
                    StockTypeBadge.Visibility = !string.IsNullOrWhiteSpace(stock.StockType) ? Visibility.Visible : Visibility.Collapsed;

                    NameInput.Text = stock.Name;
                    SymbolInput.Text = stock.Symbol;
                    StockTypeComboBox.SelectedItem = stock.StockType;

                    CurrencyComboBox.SelectedItem = !string.IsNullOrEmpty(stock.Currency) ? stock.Currency : (!string.IsNullOrEmpty(parentAsset?.Currency) ? parentAsset.Currency : "MYR");

                    UnitsInput.Value = (double)stock.Units;
                    AvgPriceInput.Value = (double)stock.AvgPrice;
                    AvgPriceInput.Text = stock.AvgPrice.ToString("N2");
                    CurrentPriceInput.Value = (double)stock.CurrentPrice;
                    CurrentPriceInput.Text = stock.CurrentPrice.ToString("N2");
                    DividendYieldInput.Value = (double)stock.DividendYield;
                    DividendYieldInput.Text = stock.DividendYield.ToString("N2");
                    AccumulatedProfitInput.Value = (double)stock.AccumulatedProfit;
                    AccumulatedProfitInput.Text = stock.AccumulatedProfit.ToString("N2");
                    RemarkInput.Text = stock.Remark ?? string.Empty;
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (stock == null) return;

            stock.Name = NameInput.Text?.Trim() ?? string.Empty;
            stock.Symbol = SymbolInput.Text?.Trim() ?? string.Empty;
            stock.StockType = StockTypeComboBox.SelectedItem as string ?? string.Empty;
            stock.Currency = CurrencyComboBox.SelectedItem as string ?? "MYR";

            stock.Units = double.IsNaN(UnitsInput.Value) ? (decimal.TryParse(UnitsInput.Text, out var u) ? u : Decimal.Zero) : (decimal)UnitsInput.Value;
            stock.AvgPrice = double.IsNaN(AvgPriceInput.Value) ? (decimal.TryParse(AvgPriceInput.Text, out var a) ? a : Decimal.Zero) : (decimal)AvgPriceInput.Value;
            stock.CurrentPrice = double.IsNaN(CurrentPriceInput.Value) ? (decimal.TryParse(CurrentPriceInput.Text, out var c) ? c : Decimal.Zero) : (decimal)CurrentPriceInput.Value;
            stock.DividendYield = double.IsNaN(DividendYieldInput.Value) ? (decimal.TryParse(DividendYieldInput.Text, out var dy) ? dy : Decimal.Zero) : (decimal)DividendYieldInput.Value;
            stock.AccumulatedProfit = double.IsNaN(AccumulatedProfitInput.Value) ? (decimal.TryParse(AccumulatedProfitInput.Text, out var ad) ? ad : Decimal.Zero) : (decimal)AccumulatedProfitInput.Value;
            stock.Remark = RemarkInput.Text?.Trim() ?? string.Empty;

            TitleTextBlock.Text = !string.IsNullOrWhiteSpace(stock.Name) ? stock.Name : "股票详情";
            StockTypeBadgeText.Text = !string.IsNullOrWhiteSpace(stock.StockType) ? stock.StockType : "未分类";
            StockTypeBadge.Visibility = !string.IsNullOrWhiteSpace(stock.StockType) ? Visibility.Visible : Visibility.Collapsed;

            AvgPriceInput.Text = stock.AvgPrice.ToString("N2");
            CurrentPriceInput.Text = stock.CurrentPrice.ToString("N2");
            DividendYieldInput.Text = stock.DividendYield.ToString("N2");
            AccumulatedProfitInput.Text = stock.AccumulatedProfit.ToString("N2");

            ApplicationDataService.SaveSnapshots();
            OnStockChanged?.Invoke();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (stock == null || parentAsset == null) return;

            if (await DialogService.Confirm(this.XamlRoot, "删除股票", $"确定要从持仓中删除 {stock.Name} ({stock.Symbol}) 吗？", "确定", "取消"))
            {
                parentAsset.Stocks.Remove(stock);
                ApplicationDataService.SaveSnapshots();
                OnStockChanged?.Invoke();
                this.Frame.Content = null;
            }
        }

        public Task<bool> IsDirtyAsync()
        {
            if (stock == null) return Task.FromResult(false);

            string currentCurrency = CurrencyComboBox.SelectedItem as string ?? "";

            decimal currentUnits = decimal.TryParse(UnitsInput.Text, out var u) ? u : (!double.IsNaN(UnitsInput.Value) ? (decimal)UnitsInput.Value : stock.Units);
            decimal currentAvg = decimal.TryParse(AvgPriceInput.Text, out var a) ? a : (!double.IsNaN(AvgPriceInput.Value) ? (decimal)AvgPriceInput.Value : stock.AvgPrice);
            decimal currentPrice = decimal.TryParse(CurrentPriceInput.Text, out var p) ? p : (!double.IsNaN(CurrentPriceInput.Value) ? (decimal)CurrentPriceInput.Value : stock.CurrentPrice);
            decimal currentYield = decimal.TryParse(DividendYieldInput.Text, out var y) ? y : (!double.IsNaN(DividendYieldInput.Value) ? (decimal)DividendYieldInput.Value : stock.DividendYield);
            decimal currentProfit = decimal.TryParse(AccumulatedProfitInput.Text, out var d) ? d : (!double.IsNaN(AccumulatedProfitInput.Value) ? (decimal)AccumulatedProfitInput.Value : stock.AccumulatedProfit);

            string currentStockType = StockTypeComboBox.SelectedItem as string ?? string.Empty;

            if ((stock.Name ?? "") != (NameInput.Text ?? "") ||
                (stock.Symbol ?? "") != (SymbolInput.Text ?? "") ||
                (stock.StockType ?? "") != currentStockType ||
                (stock.Currency ?? "") != currentCurrency ||
                stock.Units != currentUnits ||
                stock.AvgPrice != currentAvg ||
                stock.CurrentPrice != currentPrice ||
                stock.DividendYield != currentYield ||
                stock.AccumulatedProfit != currentProfit ||
                (stock.Remark ?? "") != (RemarkInput.Text?.Trim() ?? ""))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }

    public class StockDetailParameter
    {
        public Stock? Stock { get; set; }
        public Asset? Asset { get; set; }
        public Snapshot? Snapshot { get; set; }
    }
}
