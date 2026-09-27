using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading.Tasks;
using winui_portfolio.Models;
using winui_portfolio.Services;

namespace winui_portfolio.Views
{
    public sealed partial class SnapshotUpdate : Page, IDirtyCheckable
    {
        public Snapshot? snapshot { get; set; }
        private bool _reselect = false;

        public SnapshotUpdate()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Snapshot s)
            {
                snapshot = s;
                DataContext = snapshot;

                // Set the CalendarDatePicker to the snapshot's current date
                SnapshotDatePicker.Date = snapshot.Date.ToDateTime(TimeOnly.MinValue);
                UsdRateTextBox.Text = snapshot.UsdRate > 0 ? snapshot.UsdRate.ToString("0.####") : "";
            }
        }

        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (DetailFrame.Content is IDirtyCheckable dirtyCheckable)
            {
                var isDirty = await dirtyCheckable.IsDirtyAsync();
                if (isDirty)
                {
                    var result = await DialogService.Confirm(this.XamlRoot, "未保存", "还有没有保存的内容", "继续", "取消");
                    if (!result)
                    {
                        return;
                    }
                }
            }

            if (this.Frame != null && this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
            }
            else
            {
                MainWindow.Current?.MainContentFrame.Navigate(typeof(AssetSnapshot));
            }
        }

        private void SnapshotDatePicker_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args)
        {
            if (snapshot != null && args.NewDate.HasValue)
            {
                snapshot.Date = DateOnly.FromDateTime(args.NewDate.Value.DateTime);
            }
        }

        private void SaveSnapshot_Click(object sender, RoutedEventArgs e)
        {
            if (snapshot != null)
            {
                if (SnapshotDatePicker.Date.HasValue)
                {
                    snapshot.Date = DateOnly.FromDateTime(SnapshotDatePicker.Date.Value.DateTime);
                }
                if (decimal.TryParse(UsdRateTextBox.Text, out decimal rate))
                {
                    snapshot.UsdRate = rate;
                }
                ApplicationDataService.SaveSnapshots();
            }
        }

        private async void AssetListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                        NavigateToAssetDetail();
                    }
                    else
                    {
                        _reselect = true;
                        AssetListBox.SelectedItem = e.RemovedItems.Count > 0 ? e.RemovedItems[0] : null;
                    }
                }
                else
                {
                    NavigateToAssetDetail();
                }
            }
            else
            {
                NavigateToAssetDetail();
            }
        }

        private void NavigateToAssetDetail()
        {
            if (AssetListBox.SelectedItem is Asset selectedAsset)
            {
                DetailFrame.Navigate(typeof(SnapshotAssetDetail), new SnapshotAssetDetailParameter
                {
                    Asset = selectedAsset,
                    Snapshot = snapshot
                });
            }
        }

        private async void AddAsset_Click(object sender, RoutedEventArgs e)
        {
            if (DetailFrame.Content is IDirtyCheckable dirtyCheckable)
            {
                var isDirty = await dirtyCheckable.IsDirtyAsync();
                if (isDirty)
                {
                    var result = await DialogService.Confirm(this.XamlRoot, "未保存", "还有没有保存的内容", "继续", "取消");
                    if (result)
                    {
                        DetailFrame.Navigate(typeof(SnapshotAssetAdd), snapshot);
                    }
                }
                else
                {
                    DetailFrame.Navigate(typeof(SnapshotAssetAdd), snapshot);
                }
            }
            else
            {
                DetailFrame.Navigate(typeof(SnapshotAssetAdd), snapshot);
            }
        }

        public static Visibility GetAvgPriceVisibility(winui_portfolio.Models.AssetType? assetType)
        {
            if (assetType != null && (assetType.IsValOnly || assetType.IsDebt))
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public static string GetAssetIconGlyph(winui_portfolio.Models.AssetType? assetType)
        {
            if (assetType != null && assetType.Name == "股票")
            {
                return "\uE9D2"; // 股票走势图标
            }
            return "\uE8B9";
        }

        public static Microsoft.UI.Xaml.Media.SolidColorBrush GetAssetIconBrush(winui_portfolio.Models.AssetType? assetType)
        {
            if (assetType != null && assetType.Name == "股票")
            {
                return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 16, 124, 65)); // 绿色
            }
            return new Microsoft.UI.Xaml.Media.SolidColorBrush(Windows.UI.Color.FromArgb(255, 0, 120, 212)); // 蓝色
        }

        public static Visibility GetStockBadgeVisibility(winui_portfolio.Models.AssetType? assetType)
        {
            if (assetType != null && assetType.Name == "股票")
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public async Task<bool> IsDirtyAsync()
        {
            // 1. Check inner DetailFrame content first (e.g. SnapshotAssetDetail or SnapshotAssetAdd)
            if (DetailFrame.Content is IDirtyCheckable detailDirty)
            {
                if (await detailDirty.IsDirtyAsync())
                {
                    return true;
                }
            }

            // 2. Check SnapshotDatePicker
            if (snapshot != null && SnapshotDatePicker.Date.HasValue)
            {
                var pickerDate = DateOnly.FromDateTime(SnapshotDatePicker.Date.Value.DateTime);
                if (pickerDate != snapshot.Date)
                {
                    return true;
                }
            }

            // 3. Check UsdRateTextBox
            if (snapshot != null)
            {
                decimal currentRate = 0;
                if (decimal.TryParse(UsdRateTextBox.Text, out decimal parsedRate))
                {
                    currentRate = parsedRate;
                }
                if (currentRate != snapshot.UsdRate)
                {
                    return true;
                }
            }

            return false;
        }

        private static readonly SolidColorBrush ProfitBrush = new(Windows.UI.Color.FromArgb(255, 16, 124, 65));
        private static readonly SolidColorBrush LossBrush = new(Windows.UI.Color.FromArgb(255, 232, 17, 35));
        private static readonly SolidColorBrush NeutralBrush = new(Windows.UI.Color.FromArgb(255, 128, 128, 128));

        public static string FormatCurrency(decimal val)
        {
            return val.ToString("N2");
        }

        public static string GetProfitLossRateText(decimal cost, decimal value)
        {
            if (cost <= 0) return string.Empty;
            decimal rate = ((value - cost) / cost) * 100m;
            if (rate > 0) return $"(+{rate:F2}%)";
            if (rate < 0) return $"({rate:F2}%)";
            return "(0.00%)";
        }

        public static SolidColorBrush GetProfitLossRateBrush(decimal cost, decimal value)
        {
            if (cost <= 0) return NeutralBrush;
            if (value > cost) return ProfitBrush;
            if (value < cost) return LossBrush;
            return NeutralBrush;
        }

        public static Visibility GetRemarkVisibility(string? remark)
        {
            return string.IsNullOrWhiteSpace(remark) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    /// <summary>
    /// Parameter class to pass both Asset and parent Snapshot to the detail page
    /// </summary>
    public class SnapshotAssetDetailParameter
    {
        public Asset? Asset { get; set; }
        public Snapshot? Snapshot { get; set; }
    }
}
