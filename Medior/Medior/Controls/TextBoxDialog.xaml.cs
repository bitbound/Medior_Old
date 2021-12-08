using Medior.Utilities;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Controls
{
    public sealed partial class TextBoxDialog : UserControl
    {
        public TextBoxDialog()
        {
            this.InitializeComponent();
        }

        public bool IsReadOnly { get; set; }
        public string CurrentText { get; set; } = string.Empty;

        public RelayCommand CopyUrl => new(() =>
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(CurrentText);
            Clipboard.SetContent(dataPackage);
            CopiedTip.IsOpen = true;
            Debouncer.Debounce(TimeSpan.FromSeconds(1.5), () =>
            {
                DispatcherQueue.TryEnqueue(() => CopiedTip.IsOpen = false);
            });
        });
    }
}
