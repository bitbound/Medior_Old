using Medior.Core.Shared.MsStore;
using Medior.Pages;
using Medior.Services;
using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Capture;
using Windows.Services.Store;
using WinRT;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            Title = "Medior";

            InitializeComponent();

            var hwnd = WindowNative.GetWindowHandle(this);
            var context = StoreContext.GetDefault();
            InitializeWithWindow.Initialize(context, hwnd);

            _ = ViewModel?.LoadFavorites();
        }

        public MainWindowViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<MainWindowViewModel>();

        public UIElement CustomTitleBar => TitleBarElement;


        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                NavigationFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                var selectedItem = (NavigationViewItem)args.SelectedItem;

                if (selectedItem is null)
                {
                    return;
                }
                this.
                var selectedItemTag = (string)selectedItem.Tag;
                sender.Header = selectedItemTag;
                var pageName = $"Medior.Pages.{selectedItemTag}";
                Type? pageType = Type.GetType(pageName);
                if (pageType is not null)
                {
                    NavigationFrame.Navigate(pageType);
                }
            }
        }
    }
}
