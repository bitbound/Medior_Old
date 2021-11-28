using Medior.BaseTypes;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Media.Imaging;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Medior.ViewModels
{
    public class QrCodeCreatorViewModel : ViewModelBase
    {
        private readonly ILogger<QrCodeCreatorViewModel> _logger;
        private string _inputText = string.Empty;
        private BitmapImage? _qrCodeImage;

        public QrCodeCreatorViewModel(ILogger<QrCodeCreatorViewModel> logger)
        {
            _logger = logger;
        }
        public string InputText
        {
            get => _inputText;
            set => SetProperty(ref _inputText, value);
        }

        public BitmapImage? QrCodeImage
        {
            get => _qrCodeImage;
            set => SetProperty(ref _qrCodeImage, value);
        }

        public Bitmap? QrCodeBitmap { get; set; }

        public async Task<Result> GenerateQrCode()
        {
            try
            {
                QrCodeBitmap?.Dispose();

                using var qrGenerator = new QRCodeGenerator();
                using var qrCodeData = qrGenerator.CreateQrCode(InputText, QRCodeGenerator.ECCLevel.Q);
                using var qrCode = new QRCode(qrCodeData);
                QrCodeBitmap = qrCode.GetGraphic(20);
                var image = new BitmapImage();
                using var ms = new InMemoryRandomAccessStream();
                QrCodeBitmap.Save(ms.AsStreamForWrite(), ImageFormat.Jpeg);
                ms.Seek(0);
                await image.SetSourceAsync(ms);
                QrCodeImage = image;
                return Result.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while generating QR code.");
                return Result.Fail(ex);
            }
        }
    }
}
