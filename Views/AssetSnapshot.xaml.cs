using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using winui_portfolio.Models;
using winui_portfolio.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace winui_portfolio.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AssetSnapshot : Page
    {
        public ObservableCollection<Snapshot> snapshots { get; set; } =
            new ObservableCollection<Snapshot>();

        private bool _reselect = false;

        public AssetSnapshot()
        {
            InitializeComponent();
            snapshots = ApplicationDataService.Snapshots;
        }

        private async void snapShotList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_reselect)
            {
                _reselect = false;
                return;
            }

            if (SummaryFrame.Content is IDirtyCheckable dirtyCheckable)
            {
                var isDirty = await dirtyCheckable.IsDirtyAsync();
                if (isDirty)
                {
                    var result = await DialogService.Confirm(this.XamlRoot, "未保存", "还有没有保存的内容", "继续", "取消");
                    if (result)
                    {
                        SummaryFrame.Navigate(typeof(SnapshotSummary), snapShotList.SelectedItem);
                    }
                    else
                    {
                        _reselect = true;
                        snapShotList.SelectedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
                    }
                }
                else
                {
                    SummaryFrame.Navigate(typeof(SnapshotSummary), snapShotList.SelectedItem);
                }
            }
            else
            {
                SummaryFrame.Navigate(typeof(SnapshotSummary), snapShotList.SelectedItem);
            }
        }

        private async void AddSnapshot_Click(object sender, RoutedEventArgs e)
        {
            if (SummaryFrame.Content is IDirtyCheckable dirtyCheckable)
            {
                var isDirty = await dirtyCheckable.IsDirtyAsync();
                if (isDirty)
                {
                    var result = await DialogService.Confirm(this.XamlRoot, "未保存", "还有没有保存的内容", "继续", "取消");
                    if (result)
                    {
                        SummaryFrame.Navigate(typeof(SnapshotAdd));
                    }
                }
                else
                {
                    SummaryFrame.Navigate(typeof(SnapshotAdd));
                }
            }
            else
            {
                SummaryFrame.Navigate(typeof(SnapshotAdd));
            }
        }
    }

}
