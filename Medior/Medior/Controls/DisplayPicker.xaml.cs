using Medior.Models;
using Medior.Utilities;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Controls
{
    public sealed partial class DisplayPicker : UserControl
    {
        public DisplayPicker()
        {
            InitializeComponent();
            Displays = DisplayHelper.GetDisplays().ToList();
            SelectedDisplay = Displays.First(x => x.IsPrimary);
            DisplayComboBox.SelectedIndex = Displays.IndexOf(SelectedDisplay);
        }

        public List<DisplayInfo> Displays { get; private set; }

        public DisplayInfo? SelectedDisplay { get; set; }
    }
}
