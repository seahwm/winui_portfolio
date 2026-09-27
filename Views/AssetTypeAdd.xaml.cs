using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Threading.Tasks;
using winui_portfolio.Models;
using winui_portfolio.Services;

namespace winui_portfolio.Views
{
    public sealed partial class AssetTypeAdd : Page, IDirtyCheckable
    {
        public AssetTypeAdd()
        {
            InitializeComponent();
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationDataService.AssetTypes.Add(new Models.AssetType
            {
                Name = nameInput.Text,
                IsDebt = isDebt.IsChecked ?? false,
                IsRetirement = isRetirement.IsChecked ?? false,
                IsValOnly = isValOnly.IsChecked ?? false,
                IsParent = isParent.IsChecked ?? false
            });
            ApplicationDataService.SaveAssetType();
            this.Frame.Content=null;
        }

        public async Task<bool> IsDirtyAsync()
        {

            if (isDebt.IsChecked!=null && isDebt.IsChecked.Value ||
                isRetirement.IsChecked!=null && isRetirement.IsChecked.Value ||
                isValOnly.IsChecked!=null && isValOnly.IsChecked.Value ||
                isParent.IsChecked!=null && isParent.IsChecked.Value || nameInput.Text.Length!=0)
            {
                return true;
            }
            return false;
        }
    }
}
