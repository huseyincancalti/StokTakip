using System;
using System.Windows;
using System.Windows.Controls;
using StokTakip.Helpers;
using StokTakip.Models;

namespace StokTakip.Views
{
    public partial class ProductsView : UserControl
    {
        private readonly DatabaseHelper _db;
        private bool _isInitialized = false;

        public ProductsView()
        {
            _db = DatabaseHelper.Instance;
            InitializeComponent();
            _isInitialized = true;
            LoadProducts();
        }

        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalItems = 0;

        public void RefreshData()
        {
            _currentPage = 1; // Listeyi başa sar
            LoadProducts();
            PnlEditOverlay.Visibility = Visibility.Collapsed;
        }

        private void LoadProducts()
        {
            try
            {
                _totalItems = _db.GetProductCount();
                var products = _db.GetProductsPaged(_currentPage, _pageSize);
                GridProducts.ItemsSource = products;

                UpdatePaginationUI();
            }
            catch (Exception ex)
            {
                StokTakip.Helpers.MessageBoxHelper.ShowError($"Ürünler yüklenirken hata: {ex.Message}", "Hata");
            }
        }

        private void CmbPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;

            if (CmbPageSize.SelectedItem is ComboBoxItem item)
            {
                string content = item.Content.ToString();
                if (content == "Tümü")
                {
                    _pageSize = int.MaxValue;
                }
                else
                {
                    if (int.TryParse(content, out int size))
                    {
                        _pageSize = size;
                    }
                }
                
                _currentPage = 1; // Sayfa boyutu değişince başa dön
                LoadProducts();
            }
        }

        private void UpdatePaginationUI()
        {
            if (_pageSize == int.MaxValue)
            {
                TxtPageInfo.Text = "Tümü";
                BtnPrevPage.IsEnabled = false;
                BtnNextPage.IsEnabled = false;
                return;
            }

            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (totalPages < 1) totalPages = 1;

            if (_currentPage > totalPages) _currentPage = totalPages;

            TxtPageInfo.Text = $"Sayfa {_currentPage} / {totalPages}";

            BtnPrevPage.IsEnabled = _currentPage > 1;
            BtnNextPage.IsEnabled = _currentPage < totalPages;
        }

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadProducts();
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_totalItems / _pageSize);
            if (_currentPage < totalPages)
            {
                _currentPage++;
                LoadProducts();
            }
        }

        private Product? _selectedProduct;

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Product product)
            {
                _selectedProduct = product;

                if (_selectedProduct != null)
                {
                    // Formu doldur
                    TxtEditName.Text = _selectedProduct.Name;
                    
                    // Birim Combobox'ını ayarla
                    foreach(ComboBoxItem item in CmbEditUnit.Items)
                    {
                        if (item.Content.ToString() == _selectedProduct.Unit)
                        {
                            CmbEditUnit.SelectedItem = item;
                            break;
                        }
                    }

                    TxtEditCriticalStock.Text = _selectedProduct.CriticalStockLevel.ToString();

                    // Overlay'i aç
                    PnlEditOverlay.Visibility = Visibility.Visible;
                }
            }
        }

        private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            PnlEditOverlay.Visibility = Visibility.Collapsed;
            _selectedProduct = null;
        }

        private void BtnSaveEdit_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_selectedProduct == null) return;

                if (string.IsNullOrWhiteSpace(TxtEditName.Text))
                {
                    StokTakip.Helpers.MessageBoxHelper.ShowWarning("Ürün adı boş olamaz!", "Uyarı");
                    return;
                }

                if (!double.TryParse(TxtEditCriticalStock.Text, out double criticalStock)) criticalStock = 0;

                string unit = (CmbEditUnit.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";

                // Verileri güncelle
                _selectedProduct.Name = TxtEditName.Text.Trim();
                _selectedProduct.Unit = unit;
                _selectedProduct.CriticalStockLevel = criticalStock;

                // Veritabanına kaydet
                _db.UpdateProduct(_selectedProduct);

                StokTakip.Helpers.MessageBoxHelper.ShowInfo("Ürün güncellendi!", "Bilgi");
                
                PnlEditOverlay.Visibility = Visibility.Collapsed;
                LoadProducts(); // Listeyi yenile
            }
            catch (Exception ex)
            {
                StokTakip.Helpers.MessageBoxHelper.ShowError($"Güncelleme hatası: {ex.Message}", "Hata");
            }
        }
    }
}
