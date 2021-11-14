using Medior.Core.Shared.Services;
using System;
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
            //Get-WindowsCapability -Online -Name App.Support.QuickAssist
        }
    }
}
