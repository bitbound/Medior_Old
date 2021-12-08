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
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;

namespace Medior.ViewModels
{
    public class ScreenCaptureViewModel : ViewModelBase
    {
        private readonly IApiService _apiService;
        private readonly IChrono _chrono;
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
            IApiService apiService,
            IChrono chrono,
            ILogger<ScreenCaptureViewModel> logger)
        {
            _screenRecorder = screenRecorder;
            _processEx = processEx;
            _fileSystem = fileSystem;
            _apiService = apiService;
            _chrono = chrono;
            _logger = logger;
        }

        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set
            {
                SetProperty(ref _currentImage, value);
                CurrentView = ScreenCaptureView.Image;
            }
        }

        public byte[]? CurrentImageBytes { get; private set; }

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
        private ScreenCaptureView PreviousView { get; set; }
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
        public async Task<Result<string>> ShareImage()
        {
            try
            {
                if (CurrentImageBytes is null)
                {
                    return Result.Fail<string>("There is no image to upload.");
                }

                var filename = $"Screenshot-{_chrono.Now:yyyy-MM-dd HHmm}.jpg";
                var result = await _apiService.ShareImage(filename, CurrentImageBytes);
                if (!result.IsSuccess)
                {
                    return result;
                }

                return Result.Ok(result.Value ?? "");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while sharing image.");
                return Result.Fail<string>("An error occurred while uploading the image.");
            }
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
                var content = Clipboard.GetContent();

                var formats = content.AvailableFormats.ToList();

                if (!formats.Contains("Bitmap"))
                {
                    return;
                }

                var bitmapStreamRef = await content.GetBitmapAsync();
                using var stream = await bitmapStreamRef.OpenReadAsync();

                using var bitmap = new Bitmap(stream.AsStream());
                using var ms = new MemoryStream();
                bitmap.Save(ms, ImageFormat.Jpeg);
                CurrentImageBytes = ms.ToArray();

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
