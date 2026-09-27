using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using winui_portfolio.Models;
using winui_portfolio.Services;

namespace winui_portfolio.Views
{
    public sealed partial class StockAdd : Page, IDirtyCheckable
    {
        private Asset? parentAsset;
        private Snapshot? parentSnapshot;
        private bool _isSaved = false;

        public Action<Stock>? OnStockAdded { get; set; }

        public StockAdd()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SnapshotStockParameter param)
            {
                parentAsset = param.Asset;
                parentSnapshot = param.Snapshot;

                string effectiveCurrency = !string.IsNullOrEmpty(parentAsset?.Currency) ? parentAsset.Currency : "MYR";
                CurrencyComboBox.SelectedItem = effectiveCurrency;

                StockTypeComboBox.ItemsSource = ApplicationDataService.StockTypes;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (parentAsset == null) return;

            string name = NameInput.Text?.Trim() ?? string.Empty;
            string symbol = SymbolInput.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(symbol))
            {
                // 至少填写名称或代码
                return;
            }

            Stock newStock = new Stock
            {
                Name = !string.IsNullOrWhiteSpace(name) ? name : symbol,
                Symbol = symbol,
                StockType = StockTypeComboBox.SelectedItem as string ?? string.Empty,
                Currency = CurrencyComboBox.SelectedItem as string ?? "MYR",
                Units = double.IsNaN(UnitsInput.Value) ? (decimal.TryParse(UnitsInput.Text, out var u) ? u : Decimal.Zero) : (decimal)UnitsInput.Value,
                AvgPrice = double.IsNaN(AvgPriceInput.Value) ? (decimal.TryParse(AvgPriceInput.Text, out var a) ? a : Decimal.Zero) : (decimal)AvgPriceInput.Value,
                CurrentPrice = double.IsNaN(CurrentPriceInput.Value) ? (decimal.TryParse(CurrentPriceInput.Text, out var c) ? c : Decimal.Zero) : (decimal)CurrentPriceInput.Value,
                DividendYield = double.IsNaN(DividendYieldInput.Value) ? (decimal.TryParse(DividendYieldInput.Text, out var dy) ? dy : Decimal.Zero) : (decimal)DividendYieldInput.Value,
                AccumulatedProfit = double.IsNaN(AccumulatedProfitInput.Value) ? (decimal.TryParse(AccumulatedProfitInput.Text, out var ap) ? ap : Decimal.Zero) : (decimal)AccumulatedProfitInput.Value,
                Remark = RemarkInput.Text?.Trim() ?? string.Empty
            };

            parentAsset.Stocks.Add(newStock);
            ApplicationDataService.SaveSnapshots();

            _isSaved = true;
            this.Frame.Content = null;
            OnStockAdded?.Invoke(newStock);
        }

        public Task<bool> IsDirtyAsync()
        {
            if (_isSaved)
            {
                return Task.FromResult(false);
            }

            if (!string.IsNullOrWhiteSpace(NameInput.Text) ||
                !string.IsNullOrWhiteSpace(SymbolInput.Text) ||
                StockTypeComboBox.SelectedIndex >= 0 ||
                !string.IsNullOrWhiteSpace(RemarkInput.Text) ||
                UnitsInput.Value > 0 ||
                AvgPriceInput.Value > 0 ||
                CurrentPriceInput.Value > 0)
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}
