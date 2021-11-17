using CommunityToolkit.Diagnostics;
using Medior.Extensions;
using Medior.Utilities;
using Medior.ViewModels;
using Microsoft.Extensions.Primitives;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Graphics.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ScreenCapturePage : Page
    {
        public ScreenCapturePage()
        {
            InitializeComponent();
            ViewModel.RegisterSubscriptions();
        }

        public ScreenCaptureViewModel ViewModel { get; } = Ioc.Default.GetRequiredService<ScreenCaptureViewModel>();

        public AsyncRelayCommand CaptureScreenShot => new(async () => {
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

            var captureItem = await MainWindow.Instance.InvokeGraphicsCapturePicker();

            if (captureItem is null)
            {
                return;
            }

            try
            {
                var filename = $"{DateTime.Now:yyyyMMdd-HHmm-ss}.mp4";
                var filePath = Path.Combine(Path.GetTempPath(), "Medior", "Recordings", filename);
                Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? "");
                var bounds = captureItem.Size;
                var stopwatch = Stopwatch.StartNew();

                using var destStream = File.Create(filePath);
                //using var encoder = new VideoEncoder();

                using var bitmap = new Bitmap(bounds.Width, bounds.Height);
                using var graphics = Graphics.FromImage(bitmap);
                var size = bounds.Width * Bitmap.GetPixelFormatSize(bitmap.PixelFormat) / 8 * bounds.Height;
                var tmpArray = new byte[size];

                var transcoder = new MediaTranscoder
                {
                    HardwareAccelerationEnabled = true
                };

                var mp4Profile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD1080p);

                var videoProperties = VideoEncodingProperties.CreateUncompressed(
                    MediaEncodingSubtypes.Argb32,
                    (uint)bounds.Width,
                    (uint)bounds.Height);
                
                var videoDescriptor = new VideoStreamDescriptor(videoProperties);
                var sourceStream = new MediaStreamSource(videoDescriptor);
                var cts = new CancellationTokenSource();
                cts.CancelAfter(5000);

                sourceStream.SampleRequested += (sender, args) =>
                {
                    if (cts.Token.IsCancellationRequested)
                    {
                        args.Request.Sample = null;
                        return;
                    }

                    graphics.CopyFromScreen(Point.Empty, Point.Empty, new Size(bounds.Width, bounds.Height));
                    var bd = bitmap.LockBits(new Rectangle(0, 0, bounds.Width, bounds.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    
                    Marshal.Copy(bd.Scan0, tmpArray, 0, size);

                    bitmap.UnlockBits(bd);

                    var timestamp = stopwatch.Elapsed;

                    args.Request.Sample = MediaStreamSample.CreateFromBuffer(tmpArray.AsBuffer(), timestamp);
                };

                var prepareResult = await transcoder.PrepareMediaStreamSourceTranscodeAsync(sourceStream, destStream.AsRandomAccessStream(), mp4Profile);
                
                await prepareResult.TranscodeAsync();
                ViewModel.StartVideoCapture();
            }
            catch (Exception ex)
            {
                    
            }
        });

        private void Page_Unloaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.UnregisterSubscriptions();
        }
    }
}
