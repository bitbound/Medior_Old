using Medior.Models;
using Medior.Utilities;
using Microsoft.UI.Xaml.Controls;

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
