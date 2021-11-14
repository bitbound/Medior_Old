using Medior.Core.Shared.Services;

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
            _processEx.Start("ms-screenclip:");
        }
    }
}
