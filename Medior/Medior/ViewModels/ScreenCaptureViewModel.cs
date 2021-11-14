using Medior.Core.Shared.Services;
using System.Diagnostics;

namespace Medior.ViewModels
{
    public class ScreenCaptureViewModel
    {
        private readonly IProcessEx _processEx;

        public ScreenCaptureViewModel(IProcessEx processEx)
        {
            _processEx = processEx;
        }

        public void StartScreenClip()
        {
            _processEx.Start(new ProcessStartInfo()
            {
                FileName = "ms-screenclip:",
                UseShellExecute = true
            });
        }
    }
}
