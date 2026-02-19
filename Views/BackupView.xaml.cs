using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using StokTakip.Helpers;

namespace StokTakip.Views
{
    public partial class BackupView : UserControl
    {
        private readonly DatabaseHelper _db;

        public BackupView()
        {
            InitializeComponent();
            _db = DatabaseHelper.Instance;
        }

        public void RefreshData()
        {
            // Veri yenilemeye gerek yok
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Veritabanı Dosyası (*.db)|*.db",
                    FileName = $"StokTakip_Yedek_{DateTime.Now:yyyyMMdd_HHmm}.db",
                    Title = "Yedeği Kaydet"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    _db.BackupDatabase(saveFileDialog.FileName);
                    MessageBoxHelper.ShowSuccess($"Yedekleme başarıyla tamamlandı:\n{saveFileDialog.FileName}", "Başarılı");
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Yedekleme hatası: {ex.Message}", "Hata");
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Veritabanı Dosyası (*.db)|*.db",
                    Title = "Yedeği Seç"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    // Onay iste
                    var confirm = new Components.CustomMessageBox(
                        "Onaylıyor musunuz?", 
                        "Bu işlem mevcut tüm verileri silecek ve seçilen yedeği yükleyecektir.\nDevam etmek istiyor musunuz?", 
                        Components.CustomMessageBox.MessageBoxType.Confirmation);
                    
                    if (confirm.ShowDialog() == true)
                    {
                        _db.RestoreDatabase(openFileDialog.FileName);
                        MessageBoxHelper.ShowSuccess("Veriler başarıyla geri yüklendi. Uygulama yenilenecek.", "Başarılı");
                        
                        // Uygulamayı yenile (Basitçe sayfayı yenile veya tümünü)
                        // MainWindow üzerinden bir yenileme tetiklenebilir ama şimdilik yeterli.
                        if (Application.Current.MainWindow is MainWindow mw)
                        {
                            mw.RefreshCurrentView();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxHelper.ShowError($"Geri yükleme hatası: {ex.Message}", "Hata");
            }
        }
    }
}
