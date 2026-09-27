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
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using winui_portfolio.Models;
using winui_portfolio.Services;

namespace winui_portfolio.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SnapshotAdd : Page, IDirtyCheckable
    {
        public SnapshotAdd()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (SnapshotDatePicker.Date == null)
            {
                return;
            }

            var selectedDate = DateOnly.FromDateTime(SnapshotDatePicker.Date.Value.DateTime);
            decimal usdRate = 0;
            if (decimal.TryParse(UsdRateTextBox.Text, out decimal rate))
            {
                usdRate = rate;
            }

            var newSnapshot = new Snapshot
            {
                Date = selectedDate,
                UsdRate = usdRate,
                Assets = new ObservableCollection<Asset>()
            };

            int insertIndex = 0;
            while (insertIndex < ApplicationDataService.Snapshots.Count && ApplicationDataService.Snapshots[insertIndex].Date > selectedDate)
            {
                insertIndex++;
            }
            ApplicationDataService.Snapshots.Insert(insertIndex, newSnapshot);
            ApplicationDataService.SaveSnapshots();

            SnapshotDatePicker.Date = null;
            UsdRateTextBox.Text = string.Empty;
            this.Frame.Content = null;
        }

        public Task<bool> IsDirtyAsync()
        {
            if (SnapshotDatePicker.Date != null || !string.IsNullOrWhiteSpace(UsdRateTextBox.Text))
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
