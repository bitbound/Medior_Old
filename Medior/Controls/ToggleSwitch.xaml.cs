using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Medior.Controls
{
    /// <summary>
    /// Interaction logic for ToggleSwitch.xaml
    /// </summary>
    public partial class ToggleSwitch : UserControl
    {
        public ToggleSwitch()
        {
            InitializeComponent();
        }

        public event EventHandler<bool>? Switched;

        public bool IsOn
        {
            get
            {
                if (ButtonToggle.Tag.ToString() == "On")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (value)
                {
                    ButtonToggle.Tag = "On";
                    BorderToggle.Background = new SolidColorBrush(Colors.Gray);
                    var ca = new ColorAnimation(Colors.DeepSkyBlue, TimeSpan.FromSeconds(.25), FillBehavior.HoldEnd);
                    BorderToggle.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                    var da = new DoubleAnimation(10, TimeSpan.FromSeconds(.25), FillBehavior.HoldEnd);
                    TranslateTransform.BeginAnimation(TranslateTransform.XProperty, da);
                }
                else
                {
                    ButtonToggle.Tag = "Off";
                    BorderToggle.Background = new SolidColorBrush(Colors.DeepSkyBlue);
                    var ca = new ColorAnimation(Colors.Gray, TimeSpan.FromSeconds(.25), FillBehavior.HoldEnd);
                    BorderToggle.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
                    var da = new DoubleAnimation(-10, TimeSpan.FromSeconds(.25), FillBehavior.HoldEnd);
                    TranslateTransform.BeginAnimation(TranslateTransform.XProperty, da);
                }
                Switched?.Invoke(this, value);
            }
        }

        private void ButtonToggle_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            IsOn = !IsOn;
            Task.Run(() =>
            {
                System.Threading.Thread.Sleep(500);
                Dispatcher.Invoke(() => IsEnabled = true);
            });
        }
    }
}
