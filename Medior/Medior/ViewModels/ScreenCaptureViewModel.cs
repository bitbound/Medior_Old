using CommunityToolkit.Diagnostics;
using Medior.BaseTypes;
using Medior.Extensions;
using Medior.Models;
using Medior.Services;
using Medior.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Mvvm.Input;
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
        private bool _isRecording;
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

        public BitmapImage? CurrentImage
        {
            get => _currentImage;
            set
            {
                SetProperty(ref _currentImage, value);
                InvokePropertyChanged(nameof(IsIntroTextVisible));
                InvokePropertyChanged(nameof(IsCaptureImageVisible));
            }
        }

        public bool IsCaptureImageVisible
        {
            get => CurrentImage is not null && !IsRecording;
        }

        public bool IsIntroTextVisible
        {
            get => CurrentImage is null && !IsRecording;
        }
        public bool IsRecording
        {
            get => _isRecording;
            set
            {
                SetProperty(ref _isRecording, value);
                InvokePropertyChanged(nameof(IsIntroTextVisible));
                InvokePropertyChanged(nameof(IsCaptureImageVisible));
            }
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
            IsRecording = true;

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

        public void StopVideoCapture()
        {
            IsRecording = false;
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

                var bitmap = await content.GetBitmapAsync();
                using var stream = await bitmap.OpenReadAsync();

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
