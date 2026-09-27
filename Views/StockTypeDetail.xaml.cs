using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using winui_portfolio.Services;

namespace winui_portfolio.Views
{
    public sealed partial class StockTypeDetail : Page, IDirtyCheckable
    {
        private string? _originalStockType;

        public StockTypeDetail()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            _originalStockType = e.Parameter as string;

            if (_originalStockType != null)
            {
                nameInput.Text = _originalStockType;
                nameInput.IsEnabled = true;
                SaveButton.IsEnabled = true;
                DeleteButton.IsEnabled = true;
            }
            else
            {
                nameInput.Text = string.Empty;
                nameInput.IsEnabled = false;
                SaveButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_originalStockType))
            {
                return;
            }

            string newName = nameInput.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(newName))
            {
                return;
            }

            int index = ApplicationDataService.StockTypes.IndexOf(_originalStockType);
            if (index >= 0)
            {
                ApplicationDataService.StockTypes[index] = newName;
            }
            _originalStockType = newName;
            ApplicationDataService.SaveStockType();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_originalStockType))
            {
                return;
            }

            if (await DialogService.Confirm(this.XamlRoot, "删除股票类型", $"确定要删除 \"{_originalStockType}\" 吗？", "确定", "取消"))
            {
                ApplicationDataService.StockTypes.Remove(_originalStockType);
                ApplicationDataService.SaveStockType();
                this.Frame.Content = null;
            }
        }

        public Task<bool> IsDirtyAsync()
        {
            if ((_originalStockType ?? "") != (nameInput.Text?.Trim() ?? ""))
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
