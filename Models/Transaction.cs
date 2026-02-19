using System;

namespace StokTakip.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public DateTime Date { get; set; }
        public double Quantity { get; set; }
        // Aşevi mantığında satış fiyatı ve kar yoktur.
        // Sadece maliyet (stoktan düşülen değer) tutulur.
        public decimal CalculatedCost { get; set; } // FIFO'dan gelen maliyet
        public decimal SalePrice { get; set; } = 0; // Legacy column support
        public decimal Profit { get; set; } = 0; // Legacy column support
        public string Breakdown { get; set; } = ""; // Detaylı kullanım bilgisi
    }
}
