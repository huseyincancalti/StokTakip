using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using StokTakip.Helpers;
using StokTakip.Models;
using Dapper;
using System.Collections.Generic;

namespace StokTakip.Views
{
    public partial class HistoryView : UserControl
    {
        private readonly DatabaseHelper _db;
        private bool _isInitialized = false;

        public HistoryView()
        {
            _db = DatabaseHelper.Instance;
            InitializeComponent();
            _isInitialized = true;
            RefreshData();
        }

        private int _currentSalesPage = 1;
        private int _salesPageSize = 10;
        private int _totalSales = 0;

        private int _currentStockPage = 1;
        private int _stockPageSize = 10;
        private int _totalStock = 0;

        public void RefreshData()
        {
            try
            {
                _currentSalesPage = 1;
                _currentStockPage = 1;
                LoadSalesHistory();
                LoadStockHistory();
            }
            catch (Exception ex)
            {
                StokTakip.Helpers.MessageBoxHelper.ShowWarning($"Veriler yüklenirken hata oluştu: {ex.Message}", "Hata");
            }
        }

        private void LoadSalesHistory()
        {
            try
            {
                _totalSales = _db.GetSalesHistoryCount();
                var sales = _db.GetSalesHistoryPaged(_currentSalesPage, _salesPageSize).ToList();
                GridSalesHistory.ItemsSource = sales;
                
                TxtNoSales.Visibility = sales.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                GridSalesHistory.Visibility = sales.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

                UpdateSalesPaginationUI();
            }
            catch (Exception ex)
            {
                 StokTakip.Helpers.MessageBoxHelper.ShowError($"Satış geçmişi yüklenirken hata: {ex.Message}", "Hata");
            }
        }

        private void LoadStockHistory()
        {
            try
            {
                _totalStock = _db.GetStockHistoryCount();
                var stocks = _db.GetStockHistoryPaged(_currentStockPage, _stockPageSize).ToList();
                GridStockHistory.ItemsSource = stocks;

                TxtNoStock.Visibility = stocks.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                GridStockHistory.Visibility = stocks.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

                UpdateStockPaginationUI();
            }
            catch (Exception ex)
            {
                StokTakip.Helpers.MessageBoxHelper.ShowError($"Stok geçmişi yüklenirken hata: {ex.Message}", "Hata");
            }
        }

        // --- Sales Pagination ---
        private void CmbSalesPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;

            if (CmbSalesPageSize.SelectedItem is ComboBoxItem item)
            {
                string content = item.Content.ToString();
                if (content == "Tümü") _salesPageSize = int.MaxValue;
                else if (int.TryParse(content, out int size)) _salesPageSize = size;

                _currentSalesPage = 1;
                LoadSalesHistory();
            }
        }

        private void UpdateSalesPaginationUI()
        {
            if (_salesPageSize == int.MaxValue)
            {
                TxtSalesPageInfo.Text = "Tümü";
                BtnPrevSales.IsEnabled = false;
                BtnNextSales.IsEnabled = false;
                return;
            }

            int totalPages = (int)Math.Ceiling((double)_totalSales / _salesPageSize);
            if (totalPages < 1) totalPages = 1;

            if (_currentSalesPage > totalPages) _currentSalesPage = totalPages;

            TxtSalesPageInfo.Text = $"Sayfa {_currentSalesPage} / {totalPages}";

            BtnPrevSales.IsEnabled = _currentSalesPage > 1;
            BtnNextSales.IsEnabled = _currentSalesPage < totalPages;
        }

        private void BtnPrevSales_Click(object sender, RoutedEventArgs e)
        {
            if (_currentSalesPage > 1)
            {
                _currentSalesPage--;
                LoadSalesHistory();
            }
        }

        private void BtnNextSales_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_totalSales / _salesPageSize);
            if (_currentSalesPage < totalPages)
            {
                _currentSalesPage++;
                LoadSalesHistory();
            }
        }

        // --- Stock Pagination ---
        private void CmbStockPageSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;

            if (CmbStockPageSize.SelectedItem is ComboBoxItem item)
            {
                string content = item.Content.ToString();
                if (content == "Tümü") _stockPageSize = int.MaxValue;
                else if (int.TryParse(content, out int size)) _stockPageSize = size;

                _currentStockPage = 1;
                LoadStockHistory();
            }
        }

        private void UpdateStockPaginationUI()
        {
             if (_stockPageSize == int.MaxValue)
            {
                TxtStockPageInfo.Text = "Tümü";
                BtnPrevStock.IsEnabled = false;
                BtnNextStock.IsEnabled = false;
                return;
            }

            int totalPages = (int)Math.Ceiling((double)_totalStock / _stockPageSize);
            if (totalPages < 1) totalPages = 1;

            if (_currentStockPage > totalPages) _currentStockPage = totalPages;

            TxtStockPageInfo.Text = $"Sayfa {_currentStockPage} / {totalPages}";

            BtnPrevStock.IsEnabled = _currentStockPage > 1;
            BtnNextStock.IsEnabled = _currentStockPage < totalPages;
        }

        private void BtnPrevStock_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStockPage > 1)
            {
                _currentStockPage--;
                LoadStockHistory();
            }
        }

        private void BtnNextStock_Click(object sender, RoutedEventArgs e)
        {
            int totalPages = (int)Math.Ceiling((double)_totalStock / _stockPageSize);
            if (_currentStockPage < totalPages)
            {
                _currentStockPage++;
                LoadStockHistory();
            }
        }
        private void GridSalesHistory_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem is SalesHistoryModel selectedSale)
            {
                string breakdownInfo = string.IsNullOrEmpty(selectedSale.Breakdown) 
                    ? "Detay bilgisi yok." 
                    : selectedSale.Breakdown.Replace(", ", "\n");

                string message = $"📦 Ürün: {selectedSale.ProductName}\n" +
                                 $"📅 Tarih: {selectedSale.Date:dd.MM.yyyy HH:mm}\n" +
                                 $"🔢 Miktar: {selectedSale.Quantity} {GetUnit(selectedSale.ProductName)}\n" +
                                 $"💰 Toplam Maliyet: {selectedSale.CalculatedCost:C2}\n\n" +
                                 $"📋 FIFO Maliyet Detayı:\n{breakdownInfo}";

                StokTakip.Helpers.MessageBoxHelper.ShowInfo(message, "Satış İşlem Detayı");
            }
        }

        private void GridStockHistory_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem is StockHistoryModel selectedStock)
            {
                string status = selectedStock.RemainingQuantity > 0 ? "Aktif" : "Tükendi";
                
                string message = $"📦 Ürün: {selectedStock.ProductName}\n" +
                                 $"📅 Tarih: {selectedStock.DateAdded:dd.MM.yyyy HH:mm}\n" +
                                 $"💲 Birim Maliyet: {selectedStock.CostPrice:C2}\n" +
                                 $"Example Miktar: {selectedStock.OriginalQuantity}\n" +
                                 $"📉 Kalan Miktar: {selectedStock.RemainingQuantity}\n" +
                                 $"📊 Durum: {status}";

                StokTakip.Helpers.MessageBoxHelper.ShowInfo(message, "Stok Giriş Detayı");
            }
        }

        private string GetUnit(string productName)
        {
            // Basitçe ürün adından birimi tahmin etmeye çalışabiliriz veya DB'den çekebiliriz.
            // Şimdilik boş bırakalım veya global bir cache varsa oradan alalım.
            return ""; 
        }
    }
}
