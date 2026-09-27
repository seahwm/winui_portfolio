using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using winui_portfolio.Models;
using winui_portfolio.Services;

using System.Threading.Tasks;

namespace winui_portfolio.Views
{
    public sealed partial class SnapshotAssetDetail : Page, IDirtyCheckable
    {
        private Asset? asset;
        private Snapshot? parentSnapshot;

        public SnapshotAssetDetail()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SnapshotAssetDetailParameter param)
            {
                asset = param.Asset;
                parentSnapshot = param.Snapshot;

                if (asset != null)
                {
                    TitleTextBlock.Text = asset.Name ?? "资产详情";
                    NameInput.Text = asset.Name;
                    CostInput.Value = (double)asset.Cost;
                    CostInput.Text = FormatCurrency(asset.Cost);
                    ValueInput.Value = (double)asset.Value;
                    ValueInput.Text = FormatCurrency(asset.Value);

                    // Populate AssetType ComboBox
                    AssetTypeComboBox.ItemsSource = ApplicationDataService.AssetTypes;
                    if (asset.AssetType != null)
                    {
                        AssetTypeComboBox.SelectedItem = ApplicationDataService.AssetTypes
                            .FirstOrDefault(t => t.Name == asset.AssetType.Name);
                    }

                    // Set Currency ComboBox
                    CurrencyComboBox.SelectedItem = asset.Currency ?? "MYR";

                    RemarkInput.Text = asset.Remark ?? string.Empty;

                    UpdateStockCardVisibility();
                }
            }
        }

        private void AssetTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = AssetTypeComboBox.SelectedItem as Models.AssetType;
            if (selectedType != null && (selectedType.IsValOnly || selectedType.IsDebt))
            {
                CostInput.Value = 0;
            }
            UpdateStockCardVisibility();
        }

        private void UpdateStockCardVisibility()
        {
            var selectedType = AssetTypeComboBox.SelectedItem as Models.AssetType;
            bool isStockType = (selectedType != null && selectedType.Name == "股票") || (asset != null && asset.IsStock);

            if (StockHoldingCard != null)
            {
                StockHoldingCard.Visibility = isStockType ? Visibility.Visible : Visibility.Collapsed;
                if (isStockType && asset != null)
                {
                    StockCountDescText.Text = $"当前包含 {asset.Stocks.Count} 只股票";
                }
            }
        }

        private void OpenStockPage_Click(object sender, RoutedEventArgs e)
        {
            if (asset == null || parentSnapshot == null) return;

            // 自动同步当前表单基本信息
            asset.Name = NameInput.Text;
            asset.Cost = (decimal)CostInput.Value;
            asset.Value = (decimal)ValueInput.Value;
            asset.AssetType = AssetTypeComboBox.SelectedItem as Models.AssetType;
            asset.Currency = CurrencyComboBox.SelectedItem as string ?? "MYR";
            asset.Remark = RemarkInput.Text?.Trim() ?? string.Empty;

            var contentFrame = MainWindow.Current?.MainContentFrame;
            if (contentFrame != null)
            {
                contentFrame.Navigate(typeof(SnapshotStockUpdate), new SnapshotStockParameter
                {
                    Snapshot = parentSnapshot,
                    Asset = asset
                });
            }
            else if (this.Frame != null)
            {
                this.Frame.Navigate(typeof(SnapshotStockUpdate), new SnapshotStockParameter
                {
                    Snapshot = parentSnapshot,
                    Asset = asset
                });
            }
        }

        public Visibility GetCostVisibility(object selectedItem)
        {
            if (selectedItem is Models.AssetType assetType && (assetType.IsValOnly || assetType.IsDebt))
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public int GetValueColumn(object selectedItem)
        {
            if (selectedItem is Models.AssetType assetType && (assetType.IsValOnly || assetType.IsDebt))
            {
                return 0;
            }
            return 1;
        }

        public int GetValueColumnSpan(object selectedItem)
        {
            if (selectedItem is Models.AssetType assetType && (assetType.IsValOnly || assetType.IsDebt))
            {
                return 2;
            }
            return 1;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (asset == null) return;

            var selectedType = AssetTypeComboBox.SelectedItem as Models.AssetType;
            var noCost = selectedType != null && (selectedType.IsValOnly || selectedType.IsDebt);

            asset.Name = NameInput.Text;
            asset.Cost = noCost ? 0 : (decimal)CostInput.Value;
            asset.Value = (decimal)ValueInput.Value;
            asset.AssetType = selectedType;
            asset.Currency = CurrencyComboBox.SelectedItem as string ?? "MYR";
            asset.Remark = RemarkInput.Text?.Trim() ?? string.Empty;

            TitleTextBlock.Text = asset.Name ?? "资产详情";
            CostInput.Text = FormatCurrency(asset.Cost);
            ValueInput.Text = FormatCurrency(asset.Value);
            UpdateStockCardVisibility();
            ApplicationDataService.SaveSnapshots();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (asset == null || parentSnapshot == null)
            {
                return;
            }

            if (await DialogService.Confirm(this.XamlRoot, "删除资产", "确定要删除此资产吗？", "确定", "取消"))
            {
                parentSnapshot.Assets.Remove(asset);
                ApplicationDataService.SaveSnapshots();
                this.Frame.Content = null;
            }
        }

        public Task<bool> IsDirtyAsync()
        {
            if (asset == null) return Task.FromResult(false);

            var currentType = AssetTypeComboBox.SelectedItem as Models.AssetType;
            var currentCurrency = CurrencyComboBox.SelectedItem as string;
            var noCost = currentType != null && (currentType.IsValOnly || currentType.IsDebt);

            decimal currentCost = asset.Cost;
            if (decimal.TryParse(CostInput.Text, out decimal parsedCost))
            {
                currentCost = parsedCost;
            }
            else if (!double.IsNaN(CostInput.Value))
            {
                currentCost = (decimal)CostInput.Value;
            }

            decimal currentValue = asset.Value;
            if (decimal.TryParse(ValueInput.Text, out decimal parsedVal))
            {
                currentValue = parsedVal;
            }
            else if (!double.IsNaN(ValueInput.Value))
            {
                currentValue = (decimal)ValueInput.Value;
            }

            if ((asset.Name ?? "") != (NameInput.Text ?? "") ||
                (!noCost && asset.Cost != currentCost) ||
                asset.Value != currentValue ||
                (asset.AssetType?.Name ?? "") != (currentType?.Name ?? "") ||
                (asset.Currency ?? "") != (currentCurrency ?? "") ||
                (asset.Remark ?? "") != (RemarkInput.Text?.Trim() ?? ""))
            {
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public string FormatCurrency(decimal val)
        {
            return val.ToString("N2");
        }
    }
}
