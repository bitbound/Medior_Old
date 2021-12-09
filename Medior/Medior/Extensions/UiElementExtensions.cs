using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace Medior.Extensions
{
    public static class UiElementExtensions
    {
        public static async Task<ContentDialogResult> Alert(this UIElement self, string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                XamlRoot = self.XamlRoot,
                Content = message,
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close
            };
            return await dialog.ShowAsync();
        }

        public static async Task<ContentDialogResult> Confirm(this UIElement self, string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                XamlRoot = self.XamlRoot,
                Content = message,
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Primary
            };
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
            var sp = new StackPanel() { Spacing = 10 };

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

            var dialog = new ContentDialog
            {
                XamlRoot = self.XamlRoot,
                Title = title,
                Content = sp,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                DefaultButton = ContentDialogButton.Primary
            };

            var result = await dialog.ShowAsync();

            return (result, input.Text);
        }

        public static async Task<ContentDialogResult> ShowDialog(
            this UIElement self, 
            string title, 
            UIElement content,
            string? primaryButtonText = "OK",
            string? closeButtonText = "Cancel")
        {
            var dialog = new ContentDialog
            {
                Title = title,
                XamlRoot = self.XamlRoot,
                Content = content,
                PrimaryButtonText = primaryButtonText,
                CloseButtonText = closeButtonText,
                DefaultButton = ContentDialogButton.Primary
            };
            return await dialog.ShowAsync();
        }
    }
}
