using Medior.Extensions;
using Medior.ViewModels;
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
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class QrCodeCreatorPage : Page
    {
        public QrCodeCreatorPage()
        {
            this.InitializeComponent();
        }

        public QrCodeCreatorViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<QrCodeCreatorViewModel>();

        public AsyncRelayCommand GenerateCode => new(async () =>
        {
            if (!Uri.TryCreate(ViewModel.InputText, UriKind.Absolute, out _))
            {
                await this.Alert("Invalid URL", "Input text is not a valid URL.");
                return;
            }

            var result = await ViewModel.GenerateQrCode();

            if (!result.IsSuccess)
            {
                await this.Alert("Generator Failed", "Failed to generate a QR code.");
            }
        });

        public AsyncRelayCommand SaveQrCodeImage => new(async () =>
        {
            try
            {
                var picker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary
                };
                picker.FileTypeChoices.Add("JPG Image", new List<string>() { ".jpg" });
                MainWindow.Instance?.InitializeObject(picker);
                var storageItem = await picker.PickSaveFileAsync();
                if (storageItem is null)
                {
                    return;
                }

                using var writeStream = await storageItem.OpenStreamForWriteAsync();
                ViewModel.QrCodeBitmap?.Save(writeStream, ImageFormat.Jpeg);
                await this.Alert("Save Successful", "Image saved.");
            }
            catch
            {
                await this.Alert("Save Failed", "Failed to save image.  Make sure you have write access to the directory.");
            }
        });
    }
}
