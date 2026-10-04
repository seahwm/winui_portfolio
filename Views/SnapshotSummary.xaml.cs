using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Text;
using winui_portfolio.Models;
using winui_portfolio.Services;

namespace winui_portfolio.Views
{
    public sealed partial class SnapshotSummary : Page
    {
        public Snapshot? snapshot { get; set; }

        public SnapshotSummary()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Snapshot s)
            {
                snapshot = s;
                DataContext = snapshot;
                
                // 示例/默认值更新（您之后可以随时自行修改此处或调用 UpdateSummaryValues）
                UpdateSummaryValues();
            }
        }

        /// <summary>
        /// 辅助方法：设置各个KPI数值并自动更新卡片界面
        /// </summary>
        public void UpdateSummaryValues()
        {
            TotalAssetsTextBlock.Text = $"RM {SnapshotService.CalculateTotalWorth(snapshot):N2}";
            TotalLiabilitiesTextBlock.Text = $"RM {SnapshotService.CalculateTotalLiability(snapshot):N2}";
            NetAssetsTextBlock.Text = $"RM {SnapshotService.CalculateTotalNetWorth(snapshot):N2}";

            // 股票收益（根据正负数自动适配 赚/亏 样式与收益率）
            decimal stockReturn = SnapshotService.CalculateStockReturn(snapshot);
            decimal stockProfitRate = SnapshotService.CalculateStockAssetsProfitRate(snapshot);
            UpdateStockReturnUI(stockReturn, stockProfitRate);
        }

        /// <summary>
        /// 根据股票收益的数值更新 赚/亏 徽章与色彩样式
        /// </summary>
        public void UpdateStockReturnUI(decimal stockReturns)
        {
            decimal stockProfitRate = SnapshotService.CalculateStockAssetsProfitRate(snapshot);
            UpdateStockReturnUI(stockReturns, stockProfitRate);
        }

        /// <summary>
        /// 根据股票收益与收益率更新 赚/亏 徽章、百分比与色彩样式
        /// </summary>
        public void UpdateStockReturnUI(decimal stockReturns, decimal stockProfitRate)
        {
            var profitColor = Windows.UI.Color.FromArgb(255, 16, 124, 65); // #107C41
            var lossColor = Windows.UI.Color.FromArgb(255, 232, 17, 35);    // #E81123

            if (stockReturns >= 0)
            {
                // 赚 (Profit) - 绿色样式
                StockReturnsTextBlock.Text = $"+RM {stockReturns:N2}";
                StockReturnsTextBlock.Foreground = new SolidColorBrush(profitColor);

                string sign = stockProfitRate > 0 ? "+" : "";
                StockReturnRateTextBlock.Text = $"({sign}{stockProfitRate:F2}%)";
                StockReturnRateTextBlock.Foreground = new SolidColorBrush(profitColor);

                StockReturnStatusTextBlock.Text = "赚";
                StockReturnStatusTextBlock.Foreground = new SolidColorBrush(profitColor);
                StockReturnIcon.Glyph = "\uE9D2"; // Trending Up 图标
                StockReturnIcon.Foreground = new SolidColorBrush(profitColor);
                StockReturnBadgeBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(26, 16, 124, 65)); // 背景浅绿
            }
            else
            {
                // 亏 (Loss) - 红色样式
                StockReturnsTextBlock.Text = $"-RM {Math.Abs(stockReturns):N2}";
                StockReturnsTextBlock.Foreground = new SolidColorBrush(lossColor);

                StockReturnRateTextBlock.Text = $"({stockProfitRate:F2}%)";
                StockReturnRateTextBlock.Foreground = new SolidColorBrush(lossColor);

                StockReturnStatusTextBlock.Text = "亏";
                StockReturnStatusTextBlock.Foreground = new SolidColorBrush(lossColor);
                StockReturnIcon.Glyph = "\uE9D9"; // Trending Down 图标
                StockReturnIcon.Foreground = new SolidColorBrush(lossColor);
                StockReturnBadgeBorder.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(26, 232, 17, 35)); // 背景浅红
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (snapshot != null)
            {
                // Navigate in the main/top contentFrame if available, replacing the entire AssetSnapshot page
                var contentFrame = MainWindow.Current?.MainContentFrame;
                if (contentFrame != null)
                {
                    contentFrame.Navigate(typeof(SnapshotUpdate), snapshot);
                }
                else if (this.Frame != null)
                {
                    this.Frame.Navigate(typeof(SnapshotUpdate), snapshot);
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (snapshot == null)
            {
                return;
            }

            if (await DialogService.Confirm(this.XamlRoot, "删除快照", "确定要删除此快照记录吗？", "确定", "取消"))
            {
                ApplicationDataService.Snapshots.Remove(snapshot);
                ApplicationDataService.SaveSnapshots();
                this.Frame.Content = null;
            }
        }
    }
}

