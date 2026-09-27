using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using winui_portfolio.Views;

namespace winui_portfolio.Services
{
    public static class DialogService
    {
        public static async Task<bool> Confirm(XamlRoot xamlRoot,String title,String content, String primaryTxt,String closeTxt)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                PrimaryButtonText = primaryTxt,
                CloseButtonText = closeTxt,
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }
    }
}
