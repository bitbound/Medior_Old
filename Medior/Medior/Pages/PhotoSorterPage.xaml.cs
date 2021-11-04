using Medior.Extensions;
using Medior.ViewModels;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
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
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PhotoSorterPage : Page
    {
        public PhotoSorterPage()
        {
            this.InitializeComponent();
        }

        public PhotoSorterViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<PhotoSorterViewModel>();

        private async void AddJobButton_Click(object sender, RoutedEventArgs e)
        {
            var (result, newName) = await this.Prompt("New Sort Job",
                "Enter a name for the new sort job.",
                "Sort job name",
                "Save");

            if (result == ContentDialogResult.Primary)
            {
                if (string.IsNullOrWhiteSpace(newName))
                {
                    await this.Alert("Name Required", "You must specify a name.");
                    return;
                }
                ViewModel.CreateNewSortJob(newName.Trim());
            }
        }
    }
}
