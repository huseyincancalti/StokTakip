using System;
using System.Windows;
using System.Windows.Controls;
using StokTakip.Helpers;
using StokTakip.Models;
using System.Linq;
using Dapper;

namespace StokTakip.Views
{
    public partial class StockEntryView : UserControl
    {
        private readonly DatabaseHelper _db;

        public StockEntryView()
        {
            InitializeComponent();
            _db = DatabaseHelper.Instance;
            LoadProducts();

        }

        public void RefreshData()
        {
            LoadProducts();
            ClearForm();
            // Modu varsayılana çek
            RbExisting.IsChecked = true;
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
        
        private void Mode_Checked(object sender, RoutedEventArgs e)
        {
            if (PnlNew == null || PnlExisting == null) return;

            if (RbNew.IsChecked == true)
            {
                PnlNew.Visibility = Visibility.Visible;
                PnlExisting.Visibility = Visibility.Collapsed;
            }
            else
            {
                PnlNew.Visibility = Visibility.Collapsed;
                PnlExisting.Visibility = Visibility.Visible;
                LoadProducts(); // Listeyi tazele
            }
        }

        private void CmbProducts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbProducts.SelectedItem is Product selectedProduct)
            {
                if (LblCostPrice != null)
                    LblCostPrice.Text = $"Birim Maliyet Fiyatı (TL/{selectedProduct.Unit}):";
                
                if (LblQuantity != null)
                    LblQuantity.Text = $"Miktar ({selectedProduct.Unit}):";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int productId = 0;

                // 1. Ürün Belirleme
                if (RbNew.IsChecked == true)
                {
                    // Yeni Ürün Ekleme Mantığı
                    string unit = (CmbNewUnit.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";

                    if (string.IsNullOrWhiteSpace(TxtNewProductName.Text) || string.IsNullOrWhiteSpace(unit))
                    {
                        StokTakip.Helpers.MessageBoxHelper.ShowWarning("Lütfen ürün adı ve birimini seçiniz.", "Eksik Bilgi");
                        return;
                    }

                    if (!double.TryParse(TxtNewCriticalStock.Text, out double criticalStock)) criticalStock = 0;

                    var newProduct = new Product
                    {
                        Name = TxtNewProductName.Text.Trim(),
                        Unit = unit,
                        CriticalStockLevel = criticalStock
                    };

                    _db.AddProduct(newProduct);
                    
                    // Eklenen ürünün ID'sini bul (Basitçe son eklenen veya isme göre)
                    // Not: Gerçek projede AddProduct ID dönmeli. Şimdilik isimden bulalım.
                    using var conn = _db.GetConnection();
                    productId = conn.QuerySingle<int>("SELECT Id FROM Products WHERE Name = @Name ORDER BY Id DESC LIMIT 1", new { newProduct.Name });
                }
                else
                {
                    // Mevcut Ürün Seçimi
                    if (CmbProducts.SelectedValue == null)
                    {
                        StokTakip.Helpers.MessageBoxHelper.ShowWarning("Lütfen bir ürün seçin!", "Uyarı");
                        return;
                    }
                    productId = (int)CmbProducts.SelectedValue;
                }

                // 2. Stok Ekleme (Opsiyonel: Eğer miktar girildiyse)
                if (!string.IsNullOrWhiteSpace(TxtQuantity.Text))
                {
                    bool isCostValid = decimal.TryParse(TxtCostPrice.Text, out decimal costPrice);
                    bool isQtyValid = double.TryParse(TxtQuantity.Text, out double quantity);

                    if (isCostValid && costPrice >= 0 && isQtyValid && quantity > 0)
                    {
                         var stockLot = new StockLot
                        {
                            ProductId = productId,
                            CostPrice = costPrice,
                            OriginalQuantity = quantity
                        };
                        _db.AddStockEntry(stockLot);
                    }
                    else
                    {
                         StokTakip.Helpers.MessageBoxHelper.ShowError("Miktar 0'dan büyük olmalı, fiyat ise negatif olmamalıdır!", "Hata");
                         return;
                    }
                }

                StokTakip.Helpers.MessageBoxHelper.ShowSuccess("İşlem başarıyla tamamlandı!", "Bilgi");
                
                // Formu Temizle
                ClearForm();
                LoadProducts(); // ComboBox'ı güncelle
            }
            catch (Exception ex)
            {
                StokTakip.Helpers.MessageBoxHelper.ShowError($"Hata: {ex.Message}", "İşlem Başarısız");
            }
        }

        private void ClearForm()
        {
            TxtNewProductName.Clear();
            CmbNewUnit.SelectedIndex = -1;
            TxtNewCriticalStock.Clear();
            TxtCostPrice.Clear();
            TxtQuantity.Clear();
            CmbProducts.SelectedIndex = -1;
        }
    }
}
