using Medior.AppModules.ScreenCapture.Enums;
using Medior.AppModules.ScreenCapture.Services;
using Medior.BaseTypes;
using Medior.Models;
using Medior.Services;
using Medior.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;


namespace Medior.ViewModels
{
    public class ScreenCaptureViewModel : ViewModelBase
    {
        private readonly IFileSystem _fileSystem;

        private readonly ILogger<ScreenCaptureViewModel> _logger;

        private readonly IProcessEx _processEx;

        private readonly IScreenRecorder _screenRecorder;

        private CancellationTokenSource _cts = new();

        private BitmapImage? _currentImage;

        private ScreenCaptureView _currentView;

        public ScreenCaptureViewModel(IProcessEx processEx,
            IFileSystem fileSystem,
            IScreenRecorder screenRecorder,
            ILogger<ScreenCaptureViewModel> logger)
        {
            _screenRecorder = screenRecorder;
            _processEx = processEx;
            _fileSystem = fileSystem;
            _logger = logger;
        }

        public Bitmap? CurrentBitmap { get; private set; }

        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set
            {
                SetProperty(ref _currentImage, value);
                CurrentView = ScreenCaptureView.Image;
            }
        }

        private ScreenCaptureView PreviousView { get; set; }
        public ScreenCaptureView CurrentView
        {
            get => _currentView;
            set
            {
                PreviousView = CurrentView;
                SetProperty(ref _currentView, value);
                InvokePropertyChanged(nameof(IsCurrentView));
            }
        }

        public Visibility IsCurrentView(ScreenCaptureView screenCaptureView)
        {
            return CurrentView == screenCaptureView ?
                Visibility.Visible :
                Visibility.Collapsed;
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

        public async Task<Result> StartVideoCapture(DisplayInfo display, string targetPath)
        {
            try
            {
                CurrentView = ScreenCaptureView.Recording;

                _cts?.Dispose();
                _cts = new CancellationTokenSource();

                _fileSystem.CreateDirectory(Path.GetDirectoryName(targetPath) ?? "");
                using var destStream = _fileSystem.CreateFile(targetPath);

                var result = await _screenRecorder.CaptureVideo(display, 15, destStream, _cts.Token);

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
            finally
            {
                CurrentView = PreviousView;
            }
        }

        public void StopVideoCapture()
        {
            _cts?.Cancel();
        }

        public void UnregisterSubscriptions()
        {
            Clipboard.ContentChanged -= Clipboard_ContentChanged;
        }
        private async void Clipboard_ContentChanged(object? sender, object e)
        {
            try
            {
                CurrentBitmap?.Dispose();

                var content = Clipboard.GetContent();

                var formats = content.AvailableFormats.ToList();

                if (!formats.Contains("Bitmap"))
                {
                    return;
                }

                var bitmapStreamRef = await content.GetBitmapAsync();
                using var stream = await bitmapStreamRef.OpenReadAsync();

                CurrentBitmap = new Bitmap(stream.AsStreamForRead());
                stream.Seek(0);
                var image = new BitmapImage();
                await image.SetSourceAsync(stream);
                CurrentImage = image;
            }
            catch (Exception ex) 
            { 
                if (EnvironmentHelper.IsDebug)
                {
                    _logger.LogError(ex, "Error while getting clipboard content.");
                }
            }

        }
    }
}
