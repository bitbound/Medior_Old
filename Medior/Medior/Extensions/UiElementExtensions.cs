using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Medior.Extensions
{
    public static class UiElementExtensions
    {
        public static async Task<ContentDialogResult> Alert(this UIElement self, string title, string message)
        {
            var dialog = new ContentDialog();
            dialog.Title = title;
            dialog.XamlRoot = self.XamlRoot;
            dialog.Content = message;
            dialog.CloseButtonText = "Close";
            dialog.DefaultButton = ContentDialogButton.Close;
            return await dialog.ShowAsync();
        }

        public static async Task<ContentDialogResult> Confirm(this UIElement self, string title, string message)
        {
            var dialog = new ContentDialog();
            dialog.Title = title;
            dialog.XamlRoot = self.XamlRoot;
            dialog.Content = message;
            dialog.PrimaryButtonText = "Yes";
            dialog.CloseButtonText = "No";
            dialog.DefaultButton = ContentDialogButton.Primary;
            return await dialog.ShowAsync();
        }

        public static async Task<(ContentDialogResult, string)> Prompt(
            this UIElement self, 
            string title, 
            string? message,
            string? placeholderText,
            string? primaryButtonText = "OK",
            string? closeButtonText = "Cancel")
        {
            var sp = new StackPanel() {  Spacing = 10 };

            if (!string.IsNullOrWhiteSpace(message))
            {
                sp.Children.Add(new TextBlock()
                {
                    Text = message
                });
            }

            var input = new TextBox()
            {
                PlaceholderText = placeholderText
            };

            sp.Children.Add(input);

            var dialog = new ContentDialog();
            dialog.XamlRoot = self.XamlRoot;
            dialog.Title =  title;
            dialog.Content = sp;
            dialog.PrimaryButtonText = "Save";
            dialog.CloseButtonText = "Cancel";
            dialog.DefaultButton = ContentDialogButton.Primary;

            var result = await dialog.ShowAsync();

            return (result, input.Text);
        }
    }
}
