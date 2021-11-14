using Microsoft.UI.Xaml.Controls;

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
    }
}
