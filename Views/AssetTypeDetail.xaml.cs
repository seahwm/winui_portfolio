using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppNotifications;
using Microsoft.Windows.AppNotifications.Builder;
using System;
using System.Threading.Tasks;
using winui_portfolio.Models;
using winui_portfolio.Services;

namespace winui_portfolio.Views
{
    public sealed partial class AssetTypeDetail : Page,IDirtyCheckable
    {
        public Models.AssetType? assetType { get; set; }

        public AssetTypeDetail()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            assetType = e.Parameter as Models.AssetType;
            DataContext = assetType;

            if (assetType != null)
            {
                nameInput.Text = assetType.Name;
                isDebt.IsChecked = assetType.IsDebt;
                isRetirement.IsChecked = assetType.IsRetirement;
                isValOnly.IsChecked = assetType.IsValOnly;
                isParent.IsChecked = assetType.IsParent;

                bool isReadOnly = assetType.IsReadOnly;
                isDebt.IsEnabled = !isReadOnly;
                isRetirement.IsEnabled = !isReadOnly;
                isValOnly.IsEnabled = !isReadOnly;
                isParent.IsEnabled = !isReadOnly;
                nameInput.IsEnabled = !isReadOnly;

                SaveButton.IsEnabled = !isReadOnly;
                DeleteButton.IsEnabled = !isReadOnly;

                ReadOnlyNotice.IsOpen = isReadOnly;
            }
            else
            {
                nameInput.Text = string.Empty;
                ReadOnlyNotice.IsOpen = false;
                SaveButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (assetType == null || assetType.IsReadOnly)
            {
                return;
            }
            assetType.Name = nameInput.Text;
            assetType.IsDebt = isDebt.IsChecked ?? false;
            assetType.IsRetirement = isRetirement.IsChecked ?? false;
            assetType.IsValOnly = isValOnly.IsChecked ?? false;
            assetType.IsParent = isParent.IsChecked ?? false;
            ApplicationDataService.SaveAll();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (assetType == null || assetType.IsReadOnly)
            {
                return;
            }

            if(await DialogService.Confirm(this.XamlRoot,"Delete Asset Type","Are you sure you want to delete this asset type?","Yes","No"))
            {
                ApplicationDataService.AssetTypes.Remove(assetType);
                ApplicationDataService.SaveAll();
                this.Frame.Content = null;
            }
        }

        public async Task<bool> IsDirtyAsync()
        {

            if(assetType.IsDebt!= isDebt.IsChecked || 
                assetType.IsRetirement != isRetirement.IsChecked ||
                assetType.IsValOnly != isValOnly.IsChecked||
                assetType.IsParent != isParent.IsChecked ||
                assetType.Name != nameInput.Text)
            {
                return true;
            }
            return false;
        }
    }
}
