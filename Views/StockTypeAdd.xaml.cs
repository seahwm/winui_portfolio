using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using winui_portfolio.Services;

namespace winui_portfolio.Views
{
    public sealed partial class StockTypeAdd : Page, IDirtyCheckable
    {
        public StockTypeAdd()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string name = nameInput.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            if (!ApplicationDataService.StockTypes.Contains(name))
            {
                ApplicationDataService.StockTypes.Add(name);
                ApplicationDataService.SaveStockType();
            }

            this.Frame.Content = null;
        }

        public Task<bool> IsDirtyAsync()
        {
            if (!string.IsNullOrWhiteSpace(nameInput.Text))
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
