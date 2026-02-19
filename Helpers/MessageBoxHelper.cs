using System.Windows;
using StokTakip.Views.Components;

namespace StokTakip.Helpers
{
    public static class MessageBoxHelper
    {
        public static void ShowInfo(string message, string title = "Bilgi")
        {
            var msgBox = new CustomMessageBox(title, message, CustomMessageBox.MessageBoxType.Info);
            msgBox.Owner = Application.Current.MainWindow;
            msgBox.ShowDialog();
        }

        public static void ShowSuccess(string message, string title = "Başarılı")
        {
            var msgBox = new CustomMessageBox(title, message, CustomMessageBox.MessageBoxType.Success);
            msgBox.Owner = Application.Current.MainWindow;
            msgBox.ShowDialog();
        }

        public static void ShowWarning(string message, string title = "Uyarı")
        {
            var msgBox = new CustomMessageBox(title, message, CustomMessageBox.MessageBoxType.Warning);
            msgBox.Owner = Application.Current.MainWindow;
            msgBox.ShowDialog();
        }

        public static void ShowError(string message, string title = "Hata")
        {
            var msgBox = new CustomMessageBox(title, message, CustomMessageBox.MessageBoxType.Error);
            msgBox.Owner = Application.Current.MainWindow;
            msgBox.ShowDialog();
        }

        public static bool ShowConfirmation(string message, string title = "Onay")
        {
            var msgBox = new CustomMessageBox(title, message, CustomMessageBox.MessageBoxType.Confirmation);
            msgBox.Owner = Application.Current.MainWindow;
            return msgBox.ShowDialog() == true;
        }
    }
}
