namespace StokTakip.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Unit { get; set; } = "Adet"; // KG, Adet, vs.
        public double CriticalStockLevel { get; set; }
        public double CurrentStock { get; set; } // Veritabanında yok, sorgu ile doldurulacak
    }
}
