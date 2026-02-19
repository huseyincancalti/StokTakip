using System.Windows;
using StokTakip.Views;

namespace StokTakip;

public partial class MainWindow : Window
{
    private readonly System.Collections.Generic.Dictionary<string, System.Windows.Controls.UserControl> _views = new();

    public MainWindow()
    {
        InitializeComponent();

        // Varsayılan olarak Satış sayfasını aç
        MenuSales.IsChecked = true;
    }

    private void Menu_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.RadioButton rb && rb.Tag is string viewName)
        {
            NavigateTo(viewName);
        }
    }

    private void NavigateTo(string viewName)
    {
        if (!_views.ContainsKey(viewName))
        {
            switch (viewName)
            {
                case "ProductsView":
                    _views[viewName] = new ProductsView();
                    break;
                case "StockEntryView":
                    _views[viewName] = new StockEntryView();
                    break;
                case "SalesView":
                    _views[viewName] = new SalesView();
                    break;
                case "HistoryView":
                    _views[viewName] = new HistoryView();
                    break;
                case "BackupView":
                     _views[viewName] = new BackupView();
                     break;
            }
        }

        var view = _views[viewName];
        MainFrame.Content = view;

        // Verileri tazele
        try
        {
            ((dynamic)view).RefreshData();
        }
        catch (Microsoft.CSharp.RuntimeBinder.RuntimeBinderException)
        {
            // Metot yoksa yoksay
        }
    }
    
    public void RefreshCurrentView()
    {
        if (MainFrame.Content is not null)
        {
             try { ((dynamic)MainFrame.Content).RefreshData(); } catch { }
        }
    }

    // --- Verileri Silme (Reset) Mantığı ---

    private System.Windows.Threading.DispatcherTimer _resetTimer;
    private int _resetCooldown;

    private void InitializeResetTimer()
    {
        _resetTimer = new System.Windows.Threading.DispatcherTimer();
        _resetTimer.Interval = TimeSpan.FromSeconds(1);
        _resetTimer.Tick += ResetTimer_Tick;
    }

    private void BtnReset_Click(object sender, RoutedEventArgs e)
    {
        PnlResetConfirm.Visibility = Visibility.Visible;
        
        // Sayacı Başlat
        _resetCooldown = 2; // 2 saniye (Test için kısa tutulabilir)
        BtnConfirmReset.Content = $"Onayla ({_resetCooldown})";
        BtnConfirmReset.IsEnabled = false;

        if (_resetTimer == null) InitializeResetTimer();
        _resetTimer.Stop(); // Ensure it's not running
        _resetTimer.Start();
    }

    private void ResetTimer_Tick(object? sender, EventArgs e)
    {
        _resetCooldown--;
        if (_resetCooldown > 0)
        {
            BtnConfirmReset.Content = $"Onayla ({_resetCooldown})";
        }
        else
        {
            BtnConfirmReset.Content = "🗑️ Onayla";
            BtnConfirmReset.IsEnabled = true;
            _resetTimer.Stop();
        }
    }

    private void BtnCancelReset_Click(object sender, RoutedEventArgs e)
    {
        PnlResetConfirm.Visibility = Visibility.Collapsed;
        if (_resetTimer != null && _resetTimer.IsEnabled) _resetTimer.Stop();
    }
    
    private void BtnResetBackup_Click(object sender, RoutedEventArgs e)
    {
        // Önce sıfırlama ekranını kapat
        PnlResetConfirm.Visibility = Visibility.Collapsed;
        if (_resetTimer != null && _resetTimer.IsEnabled) _resetTimer.Stop();
        
        // Direk yedekleme mantığını çalıştır (BackupView'daki kodu buraya taşıyabiliriz veya oradan çağırabiliriz)
        // Kod tekrarını önlemek için BackupView'a gitmek daha mantıklı olabilir ama kullanıcı "Farklı Kaydet" bekliyor.
        // Hızlıca SaveFileDialog açalım.
        
        try
        {
             Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Veritabanı Dosyası (*.db)|*.db",
                FileName = $"StokTakip_OtomatikYedek_{DateTime.Now:yyyyMMdd_HHmm}.db",
                Title = "Silmeden Önce Yedekle"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                StokTakip.Helpers.DatabaseHelper.Instance.BackupDatabase(saveFileDialog.FileName);
                StokTakip.Helpers.MessageBoxHelper.ShowSuccess($"Yedekleme başarıyla alındı:\n{saveFileDialog.FileName}", "İşlem Tamam");
            }
        }
        catch (Exception ex)
        {
             StokTakip.Helpers.MessageBoxHelper.ShowError($"Yedekleme hatası: {ex.Message}", "Hata");
        }
    }

    private void BtnConfirmReset_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StokTakip.Helpers.DatabaseHelper.Instance.ClearDatabase();
            StokTakip.Helpers.MessageBoxHelper.ShowSuccess("Tüm veriler başarıyla silindi ve sistem sıfırlandı.", "Bilgi");
            
            // Formu ve Sayfaları Yenile
            PnlResetConfirm.Visibility = Visibility.Collapsed;
            
            RefreshCurrentView();
        }
        catch (Exception ex)
        {
            StokTakip.Helpers.MessageBoxHelper.ShowError($"Silme hatası: {ex.Message}", "Hata");
        }
    }
}