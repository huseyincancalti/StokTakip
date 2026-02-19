using System;
using System.Windows;
using System.Windows.Controls;
using StokTakip.Helpers;
using StokTakip.Models;
using System.Linq;
using Dapper;

namespace StokTakip.Views
{
    public partial class SalesView : UserControl
    {
        private readonly DatabaseHelper _db;

        public SalesView()
        {
            InitializeComponent();
            _db = DatabaseHelper.Instance;
            LoadProducts();
        }

        public void RefreshData()
        {
            LoadProducts();
            // Mevcut stok bilgisini temizle veya güncelle
            TxtCurrentStock.Text = "Mevcut Stok: -";
            CmbProducts.SelectedIndex = -1;
            TxtQuantity.Clear();
        }

        private void LoadProducts()
        {
            try
            {
                var products = _db.GetAllProducts().ToList();
                CmbProducts.ItemsSource = products;
            }
            catch (Exception ex)
            {
                StokTakip.Helpers.MessageBoxHelper.ShowError($"Ürünler yüklenirken hata: {ex.Message}", "Hata");
            }
        }



        private void CmbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbProducts.SelectedValue is int productId)
            {
                try
                {
                    double stock = _db.GetTotalStock(productId);
                    TxtCurrentStock.Text = $"Mevcut Stok: {stock}";
                }
                catch
                {
                    TxtCurrentStock.Text = "Mevcut Stok: -";
                }
            }
        }

        private void BtnSale_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CmbProducts.SelectedValue == null)
                {
                    StokTakip.Helpers.MessageBoxHelper.ShowWarning("Lütfen bir ürün seçin!", "Uyarı");
                    return;
                }

                if (!double.TryParse(TxtQuantity.Text, out double quantity) || quantity <= 0)
                {
                    StokTakip.Helpers.MessageBoxHelper.ShowWarning("Geçerli bir miktar girin!", "Uyarı");
                    return;
                }

                int productId = (int)CmbProducts.SelectedValue;
                
                // Birimi bul
                string unit = "";
                if (CmbProducts.SelectedItem is Product selectedProduct)
                {
                    unit = selectedProduct.Unit;
                }

                // FIFO Kullanım/Çıkış İşlemi
                string resultInfo = _db.MakeUsage(productId, quantity, unit);

                StokTakip.Helpers.MessageBoxHelper.ShowSuccess($"Çıkış işlemi başarıyla kaydedildi.\n\n{resultInfo}", "Başarılı");
                
                // Formu Temizle & Yenile
                TxtQuantity.Clear();
                CmbProducts.SelectedIndex = -1;
                TxtCurrentStock.Text = "Mevcut Stok: -";
                LoadProducts(); // Stok miktarını güncelle
                
            }
            catch (InvalidOperationException ioEx)
            {
                StokTakip.Helpers.MessageBoxHelper.ShowWarning($"Stok Hatası: {ioEx.Message}", "Yetersiz Stok");
            }
            catch (Exception ex)
            {
                StokTakip.Helpers.MessageBoxHelper.ShowError($"İşlem Hatası: {ex.Message}", "Hata");
            }
        }

    }
}
