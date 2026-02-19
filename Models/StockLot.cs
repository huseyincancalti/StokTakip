using System;

namespace StokTakip.Models
{
    public class StockLot
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime DateAdded { get; set; }
        public decimal CostPrice { get; set; } // Birim Alış Fiyatı
        public double OriginalQuantity { get; set; }
        public double RemainingQuantity { get; set; } // Satıldıkça düşecek
    }
}
