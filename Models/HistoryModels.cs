using System;

namespace StokTakip.Models
{
    public class SalesHistoryModel
    {
        public DateTime Date { get; set; }
        public string ProductName { get; set; } = "";
        public double Quantity { get; set; }
        // Aşevinde kar/satış fiyatı yok, sadece maliyet gösterilebilir veya gizlenir.
        public decimal CalculatedCost { get; set; }
        public string Breakdown { get; set; } = "";
    }

    public class StockHistoryModel
    {
        public DateTime DateAdded { get; set; }
        public string ProductName { get; set; } = "";
        public decimal CostPrice { get; set; }
        public double OriginalQuantity { get; set; }
        public double RemainingQuantity { get; set; }
    }
}
