using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ColorHelper;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Haley.MVVM;
using ColorPicker.Models;
using Haley.Utils;
using Haley.WPF.Controls;
using System.DirectoryServices.ActiveDirectory;
using Haley.Models;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;

enum ColorSystem : byte
{
    RGB,
    LAB,
    XYZ,
    HSV,
    HSL,
    CMYK
}
namespace LABA1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    delegate System.Windows.Media.Color FromAnotherToRgbDelegate(string[] mas);
    public partial class MainWindow : Window
    {
        string system_sender = string.Empty;
        private byte active_box_number = 0;

        private Dictionary<string, Action<System.Windows.Media.Color>> from_rgb_to_another;
        private Dictionary<string, FromAnotherToRgbDelegate> from_another_to_rgb;
        private Dictionary<string, Predicate<string[]>> check_dictionary;
        public MainWindow()
        {
            InitializeComponent();

            check_dictionary = new Dictionary<string, Predicate<string[]>>()
            {
                {"RGB", CheckRgb },
                {"HSL", CheckHsl },
                {"HSV", CheckHsv },
                {"CMYK", CheckCmyk },
                {"LAB", CheckLab },
                {"XYZ", CheckXyz }
            };

            from_rgb_to_another = new Dictionary<string, Action<System.Windows.Media.Color>>()
            {
                {"RGB", ToRGB },
                {"HSL", ToHSL },
                {"HSV", ToHSV },
                {"CMYK", ToCMYK },
                {"LAB", ToLAB },
                {"XYZ", ToXYZ }
            };

            from_another_to_rgb = new Dictionary<string, FromAnotherToRgbDelegate>()
            {
                {"RGB",ChangeRgb },
                {"HSL",  HslToRgb},
                {"HSV", HsvToRgb },
                {"CMYK", CmykToRgb },
                {"LAB", LabToRgb },
                {"XYZ", XyzToRgb}
            };
        }

        private bool CheckHex(string hex)
        {
            return long.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out long _);
        }


        private bool CheckRgb(string[] mas)
        {
            bool result = byte.TryParse(mas[0], out byte _) && byte.TryParse(mas[1], out _) && byte.TryParse(mas[2], out _);
            return result;
        }
        private bool CheckHsv(string[] mas)
        {
            bool first = short.TryParse(mas[0], out short H);
            bool second = byte.TryParse(mas[0], out byte S);
            bool third = byte.TryParse(mas[0], out byte V);
            bool result = (first && second && third) && ((H >= 0 && H <= 360) && (S >= 0 && S <= 100) && (V >= 0 && V <= 100));
            return result;
        }
        private bool CheckHsl(string[] mas)
        {
            bool first = short.TryParse(mas[0], out short H);
            bool second = byte.TryParse(mas[0], out byte S);
            bool third = byte.TryParse(mas[0], out byte L);
            bool result = (first && second && third) && ((H >= 0 && H <= 360) && (S >= 0 && S <= 100) && (L >= 0 && L <= 100));
            return result;
        }
        private bool CheckXyz(string[] mas)
        {
            bool first = byte.TryParse(mas[0], out byte X);
            bool second = byte.TryParse(mas[0], out byte Y);
            bool third = byte.TryParse(mas[0], out byte Z);
            bool result = (first && second && third) && ((X >= 0 && X <= 95) && (Y >= 0 && Y <= 100) && (Z >= 0 && Z <= 108));
            return result;
        }
        private bool CheckCmyk(string[] mas)
        {
            bool first = byte.TryParse(mas[0], out byte C);
            bool second = byte.TryParse(mas[0], out byte M);
            bool third = byte.TryParse(mas[0], out byte Y);
            bool fourth = byte.TryParse(mas[0], out byte K);
            bool result = (first && second && third && fourth) && ((C >= 0 && C <= 100) && (M >= 0 && M <= 100) && (Y >= 0 && Y <= 108) && (K >= 0 && K <= 108));
            return result;
        }
        private bool CheckLab(string[] mas)
        {
            bool first = short.TryParse(mas[0], out short L);
            bool second = short.TryParse(mas[0], out short A);
            bool third = short.TryParse(mas[0], out short B);
            bool result = (first && second && third) && ((L >= 0 && L <= 100) && (A >= -128 && A <= 128) && (B >= -128 && B <= 128));
            return result;
        }
        private System.Windows.Media.Color ChangeRgb(string[] mas)
        {
            System.Windows.Media.Color color = new System.Windows.Media.Color();
            color.R = byte.Parse(mas[0]);
            color.G = byte.Parse(mas[1]);
            color.B = byte.Parse(mas[2]);
            return color;
        }
        private System.Windows.Media.Color XyzToRgb(string[] mas)
        {
            ColorHelper.XYZ xyz = new ColorHelper.XYZ(int.Parse(mas[0]), int.Parse(mas[1]), int.Parse(mas[2]));
            ColorHelper.RGB rgb = ColorHelper.ColorConverter.XyzToRgb(xyz);
            System.Windows.Media.Color color = new System.Windows.Media.Color();
            color.R = rgb.R;
            color.G = rgb.G;
            color.B = rgb.B;
            return color;
        }
        private System.Windows.Media.Color LabToRgb(string[] mas)
        {
            double L = double.Parse(mas[0]);
            double A = double.Parse(mas[1]);
            double B = double.Parse(mas[2]);


            double X, Y, Z, x, y, z, f_x, f_y, f_z;

            f_y = (L + 16.0) / 116.0;
            f_x = (A / 500.0) + f_y;
            f_z = f_y - (B / 200.0);

            if (Math.Pow(f_x, 3) > 0.008856)
            {
                x = Math.Pow(f_x, 3);
            }
            else
            {
                x = ((116.0 * f_x) - 16.0) / 903.3;
            }

            if (L > 903.3 * 0.008856)
            {
                y = Math.Pow(((L + 16.0) / 116.0), 3);
            }
            else
            {
                y = L / 903.3;
            }

            if (Math.Pow(f_z, 3) > 0.008856)
            {
                z = Math.Pow(f_z, 3);
            }
            else
            {
                z = ((116.0 * f_z) - 16.0) / 903.3;
            }


            X = x * 95.047;
            Y = y * 100.0;
            Z = z * 108.883;




            XYZ xyz = new XYZ(X, Y, Z);
            RGB rgb = ColorHelper.ColorConverter.XyzToRgb(xyz);

            System.Windows.Media.Color color = new System.Windows.Media.Color();
            color.R = rgb.R;
            color.G = rgb.G;
            color.B = rgb.B;
            return color;

        }


        private bool HexToRgb(string str, out System.Windows.Media.Color? result_color)
        {
            if (uint.TryParse(str, out uint result))
            {
                ColorHelper.RGB rgb = ColorHelper.ColorConverter.HexToRgb(new HEX(str));
                System.Windows.Media.Color color = new System.Windows.Media.Color();
                color.R = rgb.R;
                color.G = rgb.G;
                color.B = rgb.B;
                result_color = color;
                return true;
            }
            result_color = null;
            return false;
        }
        private System.Windows.Media.Color HsvToRgb(string[] mas)
        {
            ColorHelper.HSV hsv = new ColorHelper.HSV(int.Parse(mas[0]), byte.Parse(mas[2]), byte.Parse(mas[2]));
            ColorHelper.RGB rgb = ColorHelper.ColorConverter.HsvToRgb(hsv);
            System.Windows.Media.Color color = new System.Windows.Media.Color();
            color.R = rgb.R;
            color.G = rgb.G;
            color.B = rgb.B;
            return color;
        }
        private System.Windows.Media.Color CmykToRgb(string[] mas)
        {
            ColorHelper.CMYK cmyk = new ColorHelper.CMYK(byte.Parse(mas[0]), byte.Parse(mas[1]), byte.Parse(mas[2]), byte.Parse(mas[3]));

            ColorHelper.RGB rgb = ColorHelper.ColorConverter.CmykToRgb(cmyk);
            System.Windows.Media.Color color = new System.Windows.Media.Color();
            color.R = rgb.R;
            color.G = rgb.G;
            color.B = rgb.B;
            return color;
        }
        private System.Windows.Media.Color HslToRgb(string[] mas)
        {
            ColorHelper.HSL hsl = new ColorHelper.HSL(int.Parse(mas[0]), byte.Parse(mas[1]), byte.Parse(mas[2]));
            ColorHelper.RGB rgb = ColorHelper.ColorConverter.HslToRgb(hsl);
            System.Windows.Media.Color color = new System.Windows.Media.Color();
            color.R = rgb.R;
            color.G = rgb.G;
            color.B = rgb.B;
            return color;
        }

        private void PrintToLabel(string str, ColorSystem system)
        {
            if (first_label.Content is String system1 && system1 == system.ToString())
            {
                first_text_box.Text = str;

            }
            else if (second_label.Content is String system2 && system2 == system.ToString())
            {
                second_text_box.Text = str;
            }
            else if (third_label.Content is String system3 && system3 == system.ToString())
            {
                third_text_box.Text = str;
            }
        }

        private void ToHex()
        {
            System.Drawing.Color another_color = System.Drawing.Color.FromArgb(color_picker.SelectedColor.R, color_picker.SelectedColor.G, color_picker.SelectedColor.B);
            string hex = another_color.R.ToString("X2") + another_color.G.ToString("X2") + another_color.B.ToString("X2");
            hex_color.Text = hex;
        }
        private void ToRGB(System.Windows.Media.Color color)
        {
            string result = String.Format("{0} - {1} - {2}", color.R, color.G, color.B);
            PrintToLabel(result, ColorSystem.RGB);
        }
        private void ToHSL(System.Windows.Media.Color color)
        {
            System.Drawing.Color another_color = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
            float h = another_color.GetHue();
            float s = another_color.GetSaturation() * 100;
            float l = another_color.GetBrightness() * 100;
            string H = string.Format(CultureInfo.InvariantCulture, "{0:F0}", h);
            string S = string.Format(CultureInfo.InvariantCulture, "{0:F0}", s);
            string L = string.Format(CultureInfo.InvariantCulture, "{0:F0}", l);
            string result = String.Format("{0}° - {1}% - {2}%", H, S, L);
            PrintToLabel(result, ColorSystem.HSL);

        }
        private void ToHSV(System.Windows.Media.Color color)
        {
            System.Drawing.Color another_color = System.Drawing.Color.FromArgb(color.R, color.G, color.B);
            float hue = another_color.GetHue();
            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));
            float saturation = (max == 0) ? 0 : 1f - (1f * min / max);
            float value = max / 255f;
            string H = string.Format(CultureInfo.InvariantCulture, "{0:F0}", hue);
            string S = string.Format(CultureInfo.InvariantCulture, "{0:F0}", saturation * 100);
            string V = string.Format(CultureInfo.InvariantCulture, "{0:F0}", value * 100);
            string result = String.Format("{0}° - {1}% - {2}%", H, S, V);
            PrintToLabel(result, ColorSystem.HSV);
        }
        private void ToCMYK(System.Windows.Media.Color color)
        {
            float R = color.R / 255f;
            float G = color.G / 255f;
            float B = color.B / 255f;
            float max = Math.Max(R, Math.Max(G, B));
            float k = (1 - max);
            float c = ((1 - R - k) / (1 - k)) * 100;
            float m = ((1 - G - k) / (1 - k)) * 100;
            float y = ((1 - B - k) / (1 - k)) * 100;
            string C = string.Format(CultureInfo.InvariantCulture, "{0:F0}", c);
            string M = string.Format(CultureInfo.InvariantCulture, "{0:F0}", m);
            string Y = string.Format(CultureInfo.InvariantCulture, "{0:F0}", y);
            string K = string.Format(CultureInfo.InvariantCulture, "{0:F0}", k * 100);
            string result = String.Format("{0}% - {1}% - {2}% - {3}%", C, M, Y, K);
            PrintToLabel(result, ColorSystem.CMYK);
        }
        private void ToXYZ(System.Windows.Media.Color color)
        {
            RGB rgb = new RGB(color.R, color.G, color.B);
            ColorHelper.XYZ xyz = ColorHelper.ColorConverter.RgbToXyz(rgb);
            string X = string.Format(CultureInfo.InvariantCulture, "{0:F0}", xyz.X);
            string Y = string.Format(CultureInfo.InvariantCulture, "{0:F0}", xyz.Y);
            string Z = string.Format(CultureInfo.InvariantCulture, "{0:F0}", xyz.Z);
            string result = String.Format("{0} - {1} - {2}", X, Y, Z);
            PrintToLabel(result, ColorSystem.XYZ);
        }
        private void ToLAB(System.Windows.Media.Color color)
        {
            RGB rgb = new RGB(color.R, color.G, color.B);
            ColorHelper.XYZ xyz = ColorHelper.ColorConverter.RgbToXyz(rgb);

            float x = (float)(xyz.X / 95.047f);
            float y = (float)(xyz.Y / 100.0f);
            float z = (float)(xyz.Z / 108.883f);


            if (x > .008856f)
            {
                x = (float)Math.Pow(x, (1.0f / 3.0f));
            }
            else
            {
                x = (x * 7.787f) + (16.0f / 116.0f);
            }

            if (y > .008856f)
            {
                y = (float)Math.Pow(y, 1.0f / 3.0f);
            }
            else
            {
                y = (y * 7.787f) + (16.0f / 116.0f);
            }

            if (z > .008856f)
            {
                z = (float)Math.Pow(z, 1.0f / 3.0f);
            }
            else
            {
                z = (z * 7.787f) + (16.0f / 116.0f);
            }

            float l = (116.0f * y) - 16.0f;
            float a = 500.0f * (x - y);
            float b = 200.0f * (y - z);



            string L = string.Format(CultureInfo.InvariantCulture, "{0:F0}", l);
            string A = string.Format(CultureInfo.InvariantCulture, "{0:F0}", a);
            string B = string.Format(CultureInfo.InvariantCulture, "{0:F0}", b);

            string result = string.Format("{0} - {1} - {2}", L, A, B);

            PrintToLabel(result, ColorSystem.LAB);

        }
        private void ColorPicker_ColorChanged(object sender, RoutedEventArgs e)
        {
            string? first_system = null, second_system = null, third_system = null;

            if (first_label != null && first_label.Content != null)
            {
                first_system = first_label.Content as String;
                if (!string.IsNullOrEmpty(first_system) && first_system != system_sender)
                {
                    from_rgb_to_another[first_system](color_picker.SelectedColor);

                }
            }

            if (second_label != null && second_label.Content != null)
            {
                second_system = second_label.Content as String;
                if (!string.IsNullOrEmpty(second_system) && second_system != system_sender)
                {
                    from_rgb_to_another[second_system](color_picker.SelectedColor);
                }
            }


            if (third_label != null && third_label.Content != null)
            {
                third_system = third_label.Content as String;
                if (!string.IsNullOrEmpty(third_system) && third_system != system_sender)
                {
                    from_rgb_to_another[third_system](color_picker.SelectedColor);
                }
            }
            ToHex();
            System.Windows.Media.Color color = color_picker.SelectedColor;
            System.Windows.Media.SolidColorBrush brush = new System.Windows.Media.SolidColorBrush(color);
            current_color_label.Fill = brush;
        }


        private void text_box_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && sender is TextBox text_box && text_box.Name != "hex_color")
            {
                string system_string = string.Empty;
                if (text_box.Name == "first_text_box")
                {
                    if (string.IsNullOrEmpty(first_label.Content as String))
                    {
                        System.Windows.MessageBox.Show("choose the first color system");
                        return;
                    }
                    else
                    {
                        system_string = first_label.Content as string;
                    }
                }
                else if (text_box.Name == "second_text_box")
                {
                    if (string.IsNullOrEmpty(second_label.Content as String))
                    {
                        System.Windows.MessageBox.Show("choose the second color system");
                        return;
                    }
                    else
                    {
                        system_string = second_label.Content as string;
                    }
                }
                else if (text_box.Name == "third_text_box")
                {
                    if (string.IsNullOrEmpty(third_label.Content as String))
                    {
                        System.Windows.MessageBox.Show("choose the third color system");
                        return;
                    }
                    else
                    {
                        system_string = third_label.Content as string;
                    }
                }
                string text = text_box.Text;
                if (text.Any<char>(x => char.IsLetter(x)))
                {
                    System.Windows.MessageBox.Show("Format to write: value-value-value or value-value-value-value");
                    text_box.Text = String.Empty;
                    return;
                }
                else
                {
                    string[] mas = text.Split(" - ");
                    string first = mas[0];

                    string second = mas[1];
                    string third = mas[2];
                    if (mas.Length == 4)
                    {
                        string fourth = mas[3];
                    }
                    if (check_dictionary[system_string](mas))
                    {
                        system_sender = system_string;
                        if (system_string == "XYZ")
                        {
                            MessageBox.Show("There may be a loss of color when transferring from XYZ to RGB");
                        }
                        else if (system_string == "LAB")
                        {
                            MessageBox.Show("There may be a loss of color when transferring from LAB to RGB");
                        }
                        color_picker.SelectedColor = from_another_to_rgb[system_string](mas.ToArray());
                        if (system_string == "HSV" || system_string == "HSL")
                        {
                            text_box.Text = String.Format("{0}° - {1}% - {2}%", mas[0], mas[1], mas[2]);
                        }
                        else if (system_string == "CMYK")
                        {
                            text_box.Text = String.Format("{0}% - {1}% - {2}% - {3}%", mas[0], mas[1], mas[2], mas[3]);
                        }
                        system_sender = String.Empty;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Check input format");
                        text_box.Text = String.Empty;
                    }

                }
            }
            else if (e.Key == Key.Enter && sender is TextBox text_box1 && text_box1.Name == "hex_color")
            {
                string hex = text_box1.Text;
                System.Windows.Media.Color? color;
                if (CheckHex(hex) && HexToRgb(hex, out color) && color != null)
                {
                    color_picker.SelectedColor = color.Value;
                }
                else
                {
                    System.Windows.MessageBox.Show("Check hex input format");
                    text_box1.Text = String.Empty;
                }
            }
        }

        private void text_box_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is TextBox current_text_box)
            {
                string text_box_name = current_text_box.Name;
                string system = string.Empty;
                if (text_box_name == "first_text_box")
                {
                    system = first_label.Content as String;
                }
                else if (text_box_name == "second_text_box")
                {
                    system = second_label.Content as String;
                }
                else if (text_box_name == "third_text_box")
                {
                    system = third_label.Content as String;
                }
                ToolTip tt = new ToolTip();
                string content = string.Empty;
                if (system == "RGB")
                {
                    content = "Enter in format: value(0 - 255)-value(0 - 255)-value(0 - 255)";
                }
                else if (system == "HSV")
                {
                    content = "Enter in format: value(0° - 360°)-value(0% - 100%)-value(0% - 100%)";
                }
                else if (system == "HSL")
                {
                    content = "Enter in format: value(0° - 360°)-value(0% - 100%)-value(0% - 100%)";
                }
                else if (system == "XYZ")
                {
                    content = "Enter in format: value(0 - 95)-value(0 - 100)-value(0 - 108)";
                }
                else if (system == "CMYK")
                {
                    content = "Enter in format: value(0% - 100%)-value(0 - 100%)-value(0% - 100%)-value(0% - 100%)";
                }
                else if (system == "LAB")
                {
                    content = "Enter in format: value(0 - 100)-value(-128 - +128)-value(-128 - +128)";
                }
                if (content != string.Empty)
                {
                    tt.Content = content;
                    current_text_box.ToolTip = tt;
                }
            }
        }

        private void text_box_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is TextBox text_box && text_box.ToolTip != null)
            {
                text_box.ToolTip = null;
            }
        }

        private bool WarningMessage()
        {
            string first_system = first_label.Content.ToString() ?? "";
            string third_system = first_label.Content.ToString() ?? "";
            string second_system = first_label.Content.ToString() ?? "";
            return first_system == "RGB" || second_system == "RGB" || third_system == "RGB";
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            active_box_number++;
            string system;
            if (active_box_number == 3)
            {
                var current = sender as CheckBox;
                system = current.Content as String;
                if (system != null)
                {
                    if (first_label.Content == string.Empty)
                    {
                        first_label.Content = system;
                    }
                    else if (second_label.Content == string.Empty)
                    {
                        second_label.Content = system;
                    }
                    else if (third_label.Content == string.Empty)
                    {
                        third_label.Content = system;
                    }

                }
                foreach (var item in panel.Children)
                {
                    var chek_box = item as CheckBox;
                    if (chek_box != null && !(bool)chek_box.IsChecked)
                    {
                        chek_box.IsEnabled = false;
                    }
                }

            }
            else
            {
                CheckBox current_box = sender as CheckBox;
                system = current_box.Content as String;
                if (first_label.Content == string.Empty)
                {
                    first_label.Content = system;
                }
                else if (second_label.Content == string.Empty)
                {
                    second_label.Content = system;
                }
                else if (third_label.Content == string.Empty)
                {
                    third_label.Content = system;
                }
            }
            if ((system == "XYZ" || system == "LAB") && WarningMessage())
            {
                MessageBox.Show("There may be a loss of color when transferring from XYZ or LAB to RGB");
            }
            from_rgb_to_another[system](color_picker.SelectedColor);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            active_box_number--;
            foreach (var item in panel.Children)
            {
                var chek_box = item as CheckBox;
                if (chek_box != null && !chek_box.IsEnabled)
                {
                    chek_box.IsEnabled = true;
                }
            }
            if (sender is CheckBox checkBox && checkBox.Content is String check_box_text)
            {
                if (first_label.Content == check_box_text)
                {
                    first_label.Content = string.Empty;
                    first_text_box.Text = string.Empty;
                }
                else if (second_label.Content == check_box_text)
                {
                    second_label.Content = string.Empty;
                    second_text_box.Text = string.Empty;
                }
                else if (third_label.Content == check_box_text)
                {
                    third_label.Content = string.Empty;
                    third_text_box.Text = string.Empty;
                }
            }
        }
    }

}
