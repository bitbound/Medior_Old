using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace Medior.Pages
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        public string? Version => typeof(AboutPage)?.Assembly?.GetName()?.Version?.ToString();

        private async void ContactHyperlink_Click(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("mailto:hello@lucency.co?subject=Message from Medior App"));
        }
    }
}
