using StdOttStandard;
using System;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace TimtableFH
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ColorPickerPage : Page
    {
        private readonly double blackToWhiteHeightFactor;

        private Color currentColor;
        private SetableValue<Color?> setableValue;

        public ColorPickerPage()
        {
            this.InitializeComponent();

            blackToWhiteHeightFactor = gidColors.RowDefinitions[1].Height.Value;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            (Color color, SetableValue<Color?> value) = ((Color, SetableValue<Color?>))e.Parameter;

            setableValue = value;
            SetCurrentColor(color);

            base.OnNavigatedTo(e);
        }

        private void SetCurrentColor(Color color)
        {
            currentColor = color;
            rectCurrentColor.Fill = new SolidColorBrush(color);
        }

        private Color GetColor(double value, double brightness)
        {
            byte r, g, b;

            byte raise, fall;

            switch (GetArea(value, out raise, out fall))
            {
                case 0:
                    r = byte.MaxValue;
                    g = raise;
                    b = byte.MinValue;
                    break;

                case 1:
                    r = fall;
                    g = byte.MaxValue;
                    b = byte.MinValue;
                    break;

                case 2:
                    r = byte.MinValue;
                    g = byte.MaxValue;
                    b = raise;
                    break;

                case 3:
                    r = byte.MinValue;
                    g = fall;
                    b = byte.MaxValue;
                    break;

                case 4:
                    r = raise;
                    g = byte.MinValue;
                    b = byte.MaxValue;
                    break;

                case 5:
                    r = byte.MaxValue;
                    g = byte.MinValue;
                    b = fall;
                    break;

                default:
                    r = 127;
                    g = 127;
                    b = 127;
                    break;
            }

            if (brightness < 0.5)
            {
                brightness *= 2;

                r = (byte)(r * brightness);
                g = (byte)(g * brightness);
                b = (byte)(b * brightness);
            }
            else if (brightness > 0.5)
            {
                brightness = (1 - brightness) * 2;

                r = (byte)(byte.MaxValue - (byte.MaxValue - r) * brightness);
                g = (byte)(byte.MaxValue - (byte.MaxValue - g) * brightness);
                b = (byte)(byte.MaxValue - (byte.MaxValue - b) * brightness);
            }

            return Color.FromArgb(byte.MaxValue, r, g, b);
        }

        private int GetArea(double value, out byte startToEnd, out byte endToStart)
        {
            double colorArea = 1 - blackToWhiteHeightFactor;

            if (value > colorArea)
            {
                startToEnd = 0;
                endToStart = byte.MaxValue;

                return -1;
            }

            double partSize = colorArea / 6.0;
            int area = (int)Math.Floor(value / partSize);

            startToEnd = (byte)((value - area * partSize) / partSize * byte.MaxValue);
            endToStart = (byte)(((area + 1) * partSize - value) / partSize * byte.MaxValue);

            return area;
        }

        private void GidColors_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            SetCurrentColor(e.GetCurrentPoint(gidColors).Position);
        }

        private void GidColors_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetCurrentColor(e.GetPosition(gidColors));
        }

        private void SetCurrentColor(Point pos)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X > gidColors.ActualWidth || pos.Y > gidColors.ActualHeight) return;

            Thickness margin = gidPointer.Margin;
            margin.Left = pos.X;
            margin.Top = pos.Y;
            gidPointer.Margin = margin;

            gidPointer.Visibility = Visibility.Visible;

            SetCurrentColor(GetColor(pos.Y / gidColors.ActualHeight, pos.X / gidColors.ActualWidth));
        }

        private void AbbAccept_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setableValue.SetValue(currentColor);
            Frame.GoBack();
        }

        private void AbbBack_Tapped(object sender, TappedRoutedEventArgs e)
        {
            setableValue.SetValue(null);
            Frame.GoBack();
        }
    }
}
