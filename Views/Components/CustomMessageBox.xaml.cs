using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace StokTakip.Views.Components
{
    public partial class CustomMessageBox : Window
    {
        public enum MessageBoxType
        {
            Info,
            Success,
            Warning,
            Error,
            Confirmation
        }

        public CustomMessageBox(string title, string message, MessageBoxType type)
        {
            InitializeComponent();
            
            TxtTitle.Text = title;
            TxtMessage.Text = message;

            ApplyTheme(type);
        }

        private void ApplyTheme(MessageBoxType type)
        {
            switch (type)
            {
                case MessageBoxType.Info:
                    TxtIcon.Text = "ℹ️";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155")); // Slate
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6")); // Blue
                    break;
                    
                case MessageBoxType.Success:
                    TxtIcon.Text = "✅";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46")); // Dark Green
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")); // Emerald
                    HeaderBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ECFDF5")); // Light Emerald
                    break;

                case MessageBoxType.Warning:
                    TxtIcon.Text = "⚠️";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#9A3412")); // Orange
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F97316")); // Orange
                    HeaderBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF7ED")); // Light Orange
                    break;

                case MessageBoxType.Error:
                    TxtIcon.Text = "⛔";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B")); // Red
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")); // Red
                    HeaderBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF2F2")); // Light Red
                    break;

                case MessageBoxType.Confirmation:
                    TxtIcon.Text = "❓";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#334155"));
                    BtnOk.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3B82F6"));
                    BtnOk.Content = "Evet";
                    BtnCancel.Visibility = Visibility.Visible;
                    BtnCancel.Content = "Hayır";
                    break;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText($"Başlık: {TxtTitle.Text}\nMesaj:\n{TxtMessage.Text}");
                BtnCopy.Content = "✅ Kopyalandı";
            }
            catch { /* Panoya erişim hatası vb. yoksay */ }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
