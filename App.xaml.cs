using System.Configuration;
using System.Data;
using System.Windows;

namespace StokTakip;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Kültürü Türkçe (Türkiye) olarak ayarla
            var cultureInfo = new System.Globalization.CultureInfo("tr-TR");
            
            // Para birimi sembolü ve ondalık ayracı gibi ayarların kesinleşmesi için
            System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            
            System.Threading.Thread.CurrentThread.CurrentCulture = cultureInfo;
            System.Threading.Thread.CurrentThread.CurrentUICulture = cultureInfo;

            // WPF UI Binding (XML) için de Türkçe dilini zorla
            // Bu olmadan StringFormat={}{0:C2} dolar ($) gösterebilir
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement), 
                new FrameworkPropertyMetadata(
                    System.Windows.Markup.XmlLanguage.GetLanguage(cultureInfo.IetfLanguageTag)));
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            StokTakip.Helpers.MessageBoxHelper.ShowError($"Beklenmeyen bir hata oluştu: {e.Exception.Message}\n\nDetaylar:\n{e.Exception.StackTrace}", "Kritik Hata");
            
            // Uygulamanın çökmesini engelle (Mümkünse)
            e.Handled = true;
        }
    }

