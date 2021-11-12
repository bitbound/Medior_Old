using CommunityToolkit.Diagnostics;
using Medior.Core.Shared.MsStore;
using Medior.Extensions;
using Medior.Models;
using Medior.Pages;
using Medior.Services;
using Medior.ViewModels;
using Microsoft.Extensions.DependencyInjection;
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
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Versioning;
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
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            Instance = this;

            Title = "Medior";
            this.SetStoreContext();
            ViewModel.LoadMenuItems();

            InitializeComponent();

            LoadSelectedModule();

            //this.SetWindowSize(1000, 700);
        }

        public static MainWindow? Instance { get; private set; }
        public UIElement CustomTitleBar => TitleBarElement;
        public MainWindowViewModel ViewModel { get; } = ServiceContainer.Instance.GetRequiredService<MainWindowViewModel>();

        private void AppModuleSearch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ViewModel.FilterModules(sender.Text);
        }

        private void CtrlF_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            AppModuleSearch.Focus(FocusState.Programmatic);
        }

        private void LoadSelectedModule()
        {
            var pageName = $"Medior.Pages.{ViewModel.SelectedModule?.PageName}";
            var pageType = Type.GetType(pageName);
            if (pageType is not null)
            {
                NavigationFrame.Navigate(pageType);
            }
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            
            if (args.IsSettingsSelected)
            {
                ViewModel.SelectedModule = ViewModel.SettingsAppModule;
            }
            else if (args.SelectedItem is AppModule appModule)
            {
                ViewModel.SelectedModule = appModule;
            }
            else
            {
                return;
            }

            LoadSelectedModule();
        }
    }
}
