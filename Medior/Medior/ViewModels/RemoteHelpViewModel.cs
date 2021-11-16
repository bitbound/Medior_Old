using Medior.Services;
using System;
using System.Diagnostics;
using System.IO;

namespace Medior.ViewModels
{
    public class RemoteHelpViewModel
    {
        private readonly IProcessEx _processEx;
        private readonly string _quickAssistPath = Path.Combine(Environment.SystemDirectory, "quickassist.exe");

        public RemoteHelpViewModel(IProcessEx processEx)
        {
            _processEx = processEx;
        }

        public void StartQuickAssist()
        {
            _processEx.Start(new ProcessStartInfo()
            {
                FileName = _quickAssistPath,
                UseShellExecute = true
            });
            // Get-WindowsCapability -Online -Name App.Support.QuickAssist
            // HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System PromptOnSecureDesktop 0
        }
    }
}
