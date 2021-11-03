using Medior.Core.Shared.MsStore;
using Medior.Pages;
using System;
using System.Windows;
using System.Windows.Interop;
using Windows.Services.Store;
using WinRT;

namespace Medior
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SorterMenuButton_Checked(object sender, RoutedEventArgs e)
        {
            NavigationFrame?.Navigate(new SorterPage());
        }

        private void ServiceMenuButton_Checked(object sender, RoutedEventArgs e)
        {
            NavigationFrame?.Navigate(new ServicePage());
        }

        private void AboutMenuButton_Checked(object sender, RoutedEventArgs e)
        {
            NavigationFrame?.Navigate(new AboutPage());
        }

        private void NavigationFrame_Initialized(object sender, EventArgs e)
        {
            NavigationFrame?.Navigate(new SorterPage());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var context = StoreContext.GetDefault();
            var initWindow = ((object)context).As<IInitializeWithWindow>();
            initWindow.Initialize(new WindowInteropHelper(this).Handle);
        }
    }
}
