using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel;
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

        public string? Version
        {
            get
            {
                try
                {
                    var version = Package.Current?.Id?.Version ?? new PackageVersion();
                    return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
                }
                catch
                {
                    return typeof(AboutPage)?.Assembly?.GetName()?.Version?.ToString();
                }
            }
        }

        private async void ContactHyperlink_Click(Microsoft.UI.Xaml.Documents.Hyperlink sender, Microsoft.UI.Xaml.Documents.HyperlinkClickEventArgs args)
        {
            await Launcher.LaunchUriAsync(new Uri("mailto:hello@lucency.co?subject=Message from Medior App"));
        }
    }
}
