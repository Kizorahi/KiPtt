using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace KiPtt {
    public partial class MainWindow : FluentWindow {
        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hwnd);
        [DllImport("gdi32.dll")]
        private static extern uint GetPixel(IntPtr hdc, int nXPos, int nYPos);
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int X;
            public int Y;
        }

        private string _rawHex = "8A2BE2";
        private string _rawRgb = "138, 43, 226";
        private string _rawHsl = "271, 76%, 53%";
        private string _rawHsv = "271, 81%, 89%";

        public MainWindow() {
            InitializeComponent();
            UpdateButtonsUI();
        }

        private async void StartEyeDropper(object sender, RoutedEventArgs e) {
            this.WindowState = WindowState.Minimized;
            await Task.Delay(1200);
            GetCursorPos(out POINT point);
            IntPtr hdc = GetDC(IntPtr.Zero);
            uint pixel = GetPixel(hdc, point.X, point.Y);
            ReleaseDC(IntPtr.Zero, hdc);
            byte r = (byte)(pixel & 0x000000FF);
            byte g = (byte)((pixel & 0x0000FF00) >> 8);
            byte b = (byte)((pixel & 0x00FF0000) >> 16);
            _rawHex = $"{r:X2}{g:X2}{b:X2}";
            _rawRgb = $"{r}, {g}, {b}";
            CalculateHslHsv(r, g, b, out _rawHsl, out _rawHsv);
            this.WindowState = WindowState.Normal;
            this.Activate();
            ResultText.Text = $"#{_rawHex}";
            ColorPreview.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
            UpdateButtonsUI();
            StatusText.Text = "";
        }

        private void CopyColor_Click(object sender, RoutedEventArgs e) {
            if (sender is Wpf.Ui.Controls.Button btn && btn.Tag is string rawValue) {
                Clipboard.SetText(rawValue);
                StatusText.Text = $"Скопировано в буфер: {rawValue}";
            }
        }

        private void UpdateButtonsUI() {
            BtnHex.Content = $"HEX: {_rawHex}";
            BtnHex.Tag = _rawHex;

            BtnRgb.Content = $"RGB: {_rawRgb}";
            BtnRgb.Tag = _rawRgb;

            BtnHsl.Content = $"HSL: {_rawHsl}";
            BtnHsl.Tag = _rawHsl;

            BtnHsv.Content = $"HSV: {_rawHsv}";
            BtnHsv.Tag = _rawHsv;
        }

        private void CalculateHslHsv(byte r, byte g, byte b, out string hsl, out string hsv) {
            float rd = r / 255f;
            float gd = g / 255f;
            float bd = b / 255f;

            float max = Math.Max(rd, Math.Max(gd, bd));
            float min = Math.Min(rd, Math.Min(gd, bd));
            float delta = max - min;

            float h = 0;
            if (delta > 0) {
                if (max == rd) h = (gd - bd) / delta + (gd < bd ? 6 : 0);
                else if (max == gd) h = (bd - rd) / delta + 2;
                else if (max == bd) h = (rd - gd) / delta + 4;
                h *= 60;
            }

            float l = (max + min) / 2f;
            float sHsl = delta == 0 ? 0 : delta / (1 - Math.Abs(2 * l - 1));
            hsl = $"{Math.Round(h)}, {Math.Round(sHsl * 100)}%, {Math.Round(l * 100)}%";

            float sHsv = max == 0 ? 0 : delta / max;
            hsv = $"{Math.Round(h)}, {Math.Round(sHsv * 100)}%, {Math.Round(max * 100)}%";
        }
    }
}