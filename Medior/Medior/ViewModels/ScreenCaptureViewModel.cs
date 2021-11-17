using Medior.BaseTypes;
using Medior.Services;
using Medior.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Capture;


namespace Medior.ViewModels
{
    public class ScreenCaptureViewModel : ViewModelBase
    {
        private readonly IEnvironmentService _environmentService;
        private readonly ILogger<ScreenCaptureViewModel> _logger;
        private readonly IProcessEx _processEx;
        private BitmapImage? _currentImage;

        public ScreenCaptureViewModel(IProcessEx processEx, 
            IEnvironmentService environmentService,
            ILogger<ScreenCaptureViewModel> logger)
        {
            _processEx = processEx;
            _environmentService = environmentService;
            _logger = logger;
        }

        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set => SetProperty(ref _currentImage, value);
        }

        public void RegisterSubscriptions()
        {
            Clipboard.ContentChanged += Clipboard_ContentChanged;
        }
        public void StartScreenClip()
        {
            _processEx.Start(new ProcessStartInfo()
            {
                FileName = "ms-screenclip:",
                UseShellExecute = true
            });
        }

        public async Task<Result> StartVideoCapture(
            GraphicsCaptureItem captureItem, 
            string targetPath)
        {
            var result = await VideoEncoder.Encode(captureItem, targetPath);

            if (!result.IsSuccess)
            {
                if (result.Exception is not null)
                {
                    _logger.LogError(result.Exception, "Error while capturing video.");
                }
                else
                {
                    _logger.LogError(result.Error);
                }
            }
            return result;
        }

        public void UnregisterSubscriptions()
        {
            Clipboard.ContentChanged -= Clipboard_ContentChanged;
        }

        private async void Clipboard_ContentChanged(object? sender, object e)
        {
            try
            {
                var content = Clipboard.GetContent();

                var formats = content.AvailableFormats.ToList();

                if (!formats.Contains("Bitmap"))
                {
                    return;
                }

                var bitmap = await content.GetBitmapAsync();
                using var stream = await bitmap.OpenReadAsync();

                var image = new BitmapImage();
                await image.SetSourceAsync(stream);
                CurrentImage = image;
            }
            catch (Exception ex) 
            { 
                if (_environmentService.IsDebug)
                {
                    _logger.LogError(ex, "Error while getting clipboard content.");
                }
            }

        }
    }
}
