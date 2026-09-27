using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using winui_portfolio.Models;
using winui_portfolio.Services;

using System.Threading.Tasks;

namespace winui_portfolio.Views
{
    public sealed partial class SnapshotAssetAdd : Page, IDirtyCheckable
    {
        private Snapshot? parentSnapshot;

        public SnapshotAssetAdd()
        {
            InitializeComponent();

            // Populate AssetType ComboBox
            AssetTypeComboBox.ItemsSource = ApplicationDataService.AssetTypes;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Snapshot s)
            {
                parentSnapshot = s;
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

        private void AssetTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedType = AssetTypeComboBox.SelectedItem as Models.AssetType;
            if (selectedType != null && (selectedType.IsValOnly || selectedType.IsDebt))
            {
                CostInput.Value = 0;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (parentSnapshot == null) return;

            var selectedType = AssetTypeComboBox.SelectedItem as Models.AssetType;
            var noCost = selectedType != null && (selectedType.IsValOnly || selectedType.IsDebt);

            var newAsset = new Asset
            {
                Name = NameInput.Text,
                Cost = noCost ? 0 : (double.IsNaN(CostInput.Value) ? 0 : (decimal)CostInput.Value),
                Value = double.IsNaN(ValueInput.Value) ? 0 : (decimal)ValueInput.Value,
                AssetType = selectedType,
                Currency = CurrencyComboBox.SelectedItem as string ?? "MYR",
                Remark = RemarkInput.Text?.Trim() ?? string.Empty
            };

            parentSnapshot.Assets.Add(newAsset);
            ApplicationDataService.SaveSnapshots();
            this.Frame.Content = null;
        }

        public Task<bool> IsDirtyAsync()
        {
            var selectedType = AssetTypeComboBox.SelectedItem as Models.AssetType;
            var noCost = selectedType != null && (selectedType.IsValOnly || selectedType.IsDebt);

            if (!string.IsNullOrEmpty(NameInput.Text) ||
                !string.IsNullOrWhiteSpace(RemarkInput.Text) ||
                AssetTypeComboBox.SelectedItem != null ||
                (!noCost && !double.IsNaN(CostInput.Value) && CostInput.Value != 0) ||
                (!double.IsNaN(ValueInput.Value) && ValueInput.Value != 0))
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
