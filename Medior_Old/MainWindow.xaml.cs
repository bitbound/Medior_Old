using MahApps.Metro.Controls;
using Medior.Core.Shared.MsStore;
using Medior.Pages;
using Medior.ViewModels;
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
    public partial class MainWindow : MetroWindow
    {
        private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var context = StoreContext.GetDefault();
            var initWindow = ((object)context).As<IInitializeWithWindow>();
            initWindow.Initialize(new WindowInteropHelper(this).Handle);

            _ = ViewModel?.LoadMenuItems();
        }
    }
}
