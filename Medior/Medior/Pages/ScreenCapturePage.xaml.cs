using CommunityToolkit.Diagnostics;
using Medior.BaseTypes;
using Medior.Controls;
using Medior.Enums;
using Medior.Extensions;
using Medior.Models.Messages;
using Medior.Services;
using Medior.Utilities;
using Medior.ViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using Windows.Storage.Pickers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScreenCapturePage : Page
    {
        private readonly ILogger<ScreenCapturePage> _logger = Ioc.Default.GetRequiredService<ILogger<ScreenCapturePage>>();
        private readonly IMessagePublisher _messagePublisher = Ioc.Default.GetRequiredService<IMessagePublisher>();
        private readonly IAuthService _authService = Ioc.Default.GetRequiredService<IAuthService>();

        public ScreenCapturePage()
        {
            InitializeComponent();
            ViewModel.RegisterClipboardChangedHandler();
        }

        public AsyncRelayCommand CaptureScreenShot => new(async () =>
         {
             Guard.IsNotNull(MainWindow.Instance, nameof(MainWindow.Instance));

             MainWindow.Instance.Minimize();

             ViewModel.StartScreenClip();

             var clipProc = Process.GetProcessesByName("ScreenClippingHost").FirstOrDefault();
             if (clipProc is not null)
             {
                 await clipProc.WaitForExitAsync();
             }

             MainWindow.Instance.Restore();
         });

        public AsyncRelayCommand CaptureVideo => new(async () =>
         {
             Guard.IsNotNull(MainWindow.Instance, nameof(MainWindow.Instance));

             var displayPicker = new DisplayPicker();

             var dialogResult = await this.ShowDialog("Select a display to capture", displayPicker);
             if (dialogResult != ContentDialogResult.Primary)
             {
                 return;
             }

             var selectedDisplay = displayPicker.SelectedDisplay;

             if (selectedDisplay is null)
             {
                 return;
             }

             var filename = $"{DateTime.Now:yyyyMMdd-HHmm-ss}.wmv";

             var filePath = Path.Combine(AppFolders.RecordingsPath, filename);

             var result = await ViewModel.StartVideoCapture(selectedDisplay, filePath);

             if (!result.IsSuccess)
             {
                 await this.Alert("Capture Error", "An error occurred while capturing the video.");
                 return;
             }

             var fileSavePicker = new FileSavePicker
             {
                 SuggestedStartLocation = PickerLocationId.VideosLibrary,
                 SuggestedFileName = $"MediorCapture-{filename}"
             };
             fileSavePicker.FileTypeChoices.Add("Windows Media Video", new List<string>() { ".wmv" });
             MainWindow.Instance.InitializeObject(fileSavePicker);
             var saveResult = await fileSavePicker.PickSaveFileAsync();
             if (saveResult is not null)
             {
                 File.Copy(filePath, saveResult.Path, true);
             }
             File.Delete(filePath);
         });

        public AsyncRelayCommand SaveImage => new(async () =>
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
                 await writeStream.WriteAsync(ViewModel.CurrentImageBytes.AsMemory());
                 await this.Alert("Save Successful", "Screenshot saved.");
             }
             catch (Exception ex)
             {
                 _logger.LogError(ex, "Error while saving image.");
                 await this.Alert("Save Failed", "Failed to save screenshot.  Make sure you have write access to the directory.");
             }
         });

        public AsyncRelayCommand ShareImage => new(async () =>
         {
             try
             {
                 if (!EnvironmentHelper.IsDebug)
                 {
                     if (!_authService.IsSignedIn)
                     {
                         await this.Alert("Sign In Required", "You must be signed in to use this feature.");
                         return;
                     }
                 }

                 _messagePublisher.Messenger.Send(new IsLoadingMessage(true));
                 var result = await ViewModel.ShareImage();
                 _messagePublisher.Messenger.Send(new IsLoadingMessage(false));

                 if (!result.IsSuccess)
                 {
                     await this.Alert("Upload Failed", result.Error ?? "");
                     return;
                 }

                 var dialog = new TextBoxDialog()
                 {
                     IsReadOnly = true,
                     CurrentText = result.Value ?? "",
                 };

                 var dialogResult = await this.ShowDialog("Image URL", dialog, "OK", null);
             }
             finally
             {
                 _messagePublisher.Messenger.Send(new IsLoadingMessage(false));
             }
         });

        public RelayCommand StopVideoCapture => new(() => ViewModel.StopVideoCapture());
        public ScreenCaptureViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<ScreenCaptureViewModel>();

        private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.UnregisterClipboardChangeHandler();
        }
    }
}