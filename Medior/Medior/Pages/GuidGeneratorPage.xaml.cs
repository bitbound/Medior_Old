using Medior.Core.Shared.Utilities;
using Medior.Services;
using Medior.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GuidGeneratorPage : Page
    {
        public GuidGeneratorPage()
        {
            this.InitializeComponent();
        }

        public GuidGeneratorViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<GuidGeneratorViewModel>();

        public RelayCommand Copy => new(() =>
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(ViewModel.CurrentGuid);
            Clipboard.SetContent(dataPackage);
            CopiedTip.IsOpen = true;
            Debouncer.Debounce(TimeSpan.FromSeconds(1.5), () =>
            {
                DispatcherQueue.TryEnqueue(() => CopiedTip.IsOpen = false);
            });
        });

        public RelayCommand Refresh => new(() =>
        {
            ViewModel.CurrentGuid = Guid.NewGuid().ToString();
        });
    }
}
