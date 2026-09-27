using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
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

namespace winui_portfolio.Views
{
    public sealed partial class AssetType : Page
    {
        public ObservableCollection<Models.AssetType> assetTypes { get; set; } = 
            new ObservableCollection<Models.AssetType>();
       
        private bool _reselect=false;

        public AssetType()
        {
            InitializeComponent();
            assetTypes = ApplicationDataService.AssetTypes;
        }

        private async void snapShotList_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                        DetailFrame.Navigate(typeof(AssetTypeDetail), snapShotList.SelectedItem);
                    }
                    else
                    {
                        _reselect = true;
                        snapShotList.SelectedItem = e.RemovedItems[0];
                    }
                }
                else
                {
                    DetailFrame.Navigate(typeof(AssetTypeDetail), snapShotList.SelectedItem);
                }
            }
            else
            {
                DetailFrame.Navigate(typeof(AssetTypeDetail), snapShotList.SelectedItem);
            }
            
        }

        private async void AddAssetType_Click(object sender, RoutedEventArgs e)
        {
            if(DetailFrame.Content is IDirtyCheckable dirtyCheckable)
            {
                var isDirty = await dirtyCheckable.IsDirtyAsync();
                if (isDirty)
                {
                    var result = await DialogService.Confirm(this.XamlRoot, "未保存","还有没有保存的内容","继续","取消");
                    if(result)
                    {
                        DetailFrame.Navigate(typeof(AssetTypeAdd));
                    }
                }
                else
                {
                    DetailFrame.Navigate(typeof(AssetTypeAdd));
                }
            }
            else
            {
                DetailFrame.Navigate(typeof(AssetTypeAdd));
            }
        }
    }
}
