using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using winui_portfolio.Services;

namespace winui_portfolio.Views
{
    public sealed partial class StockType : Page
    {
        public ObservableCollection<string> stockTypes { get; set; } =
            new ObservableCollection<string>();

        private bool _reselect = false;

        public StockType()
        {
            InitializeComponent();
            stockTypes = ApplicationDataService.StockTypes;
        }

        private async void stockTypeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_reselect)
            {
                _reselect = false;
                return;
            }

            if (stockTypeList.SelectedItem == null)
            {
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
                        DetailFrame.Navigate(typeof(StockTypeDetail), stockTypeList.SelectedItem);
                    }
                    else
                    {
                        if (e.RemovedItems.Count > 0)
                        {
                            _reselect = true;
                            stockTypeList.SelectedItem = e.RemovedItems[0];
                        }
                    }
                }
                else
                {
                    DetailFrame.Navigate(typeof(StockTypeDetail), stockTypeList.SelectedItem);
                }
            }
            else
            {
                DetailFrame.Navigate(typeof(StockTypeDetail), stockTypeList.SelectedItem);
            }
        }

        private async void AddStockType_Click(object sender, RoutedEventArgs e)
        {
            if (DetailFrame.Content is IDirtyCheckable dirtyCheckable)
            {
                var isDirty = await dirtyCheckable.IsDirtyAsync();
                if (isDirty)
                {
                    var result = await DialogService.Confirm(this.XamlRoot, "未保存", "还有没有保存的内容", "继续", "取消");
                    if (result)
                    {
                        stockTypeList.SelectedItem = null;
                        DetailFrame.Navigate(typeof(StockTypeAdd));
                    }
                }
                else
                {
                    stockTypeList.SelectedItem = null;
                    DetailFrame.Navigate(typeof(StockTypeAdd));
                }
            }
            else
            {
                stockTypeList.SelectedItem = null;
                DetailFrame.Navigate(typeof(StockTypeAdd));
            }
        }
    }
}
