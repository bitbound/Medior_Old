using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Medior.Controls
{
    public sealed partial class GatedControl : UserControl
    {
        public static readonly DependencyProperty ChildContentProperty =
            DependencyProperty.Register(
                name: "ChildContent",
                propertyType: typeof(UIElement),
                ownerType: typeof(GatedControl),
                typeMetadata: new PropertyMetadata(defaultValue: false));

        public static readonly DependencyProperty DisabledMessageProperty =
            DependencyProperty.Register(
                name: "DisabledMessage",
                propertyType: typeof(string),
                ownerType: typeof(GatedControl),
                typeMetadata: new PropertyMetadata(defaultValue: string.Empty));

        public GatedControl()
        {
            this.InitializeComponent();
        }

        public UIElement ChildContent
        {
            get => (UIElement)GetValue(ChildContentProperty);
            set => SetValue(ChildContentProperty, value);
        }

        public bool IsLocked
        {
            get => !IsEnabled;
        }

        public string DisabledMessage
        {
            get => (string)GetValue(DisabledMessageProperty);
            set => SetValue(DisabledMessageProperty, value);
        }
    }
}
