using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Linq;

namespace StokTakip.Helpers
{
    public class DatabaseHelper
    {
        private static DatabaseHelper? _instance;
        private readonly string _connectionString;

        public static DatabaseHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DatabaseHelper();
                return _instance;
            }
        }

        public void ClearDatabase()
        {
            using var connection = GetConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Sırayla sil (Foreign Key kurallarına dikkat ederek - önce child, sonra parent)
                connection.Execute("DELETE FROM Transactions", transaction: transaction);
                connection.Execute("DELETE FROM StockLots", transaction: transaction);
                connection.Execute("DELETE FROM Products", transaction: transaction);
                
                // Sequence'leri sıfırla (Auto Increment ID'ler 1'den başlasın)
                connection.Execute("DELETE FROM sqlite_sequence WHERE name IN ('Transactions', 'StockLots', 'Products')", transaction: transaction);

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        private DatabaseHelper()
        {
            // Veritabanı dosyası exe'nin yanında oluşsun (Taşınabilir olması için)
            string dbFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StokTakip.db");
            _connectionString = $"Data Source={dbFile}";
            
            InitializeDatabase();
        }

        public IDbConnection GetConnection()
        {
            return new SqliteConnection(_connectionString);
        }

        public void InitializeDatabase()
        {
            using var connection = GetConnection();
            connection.Open();

            // Tabloları oluşturma sorgusu
            string createTablesSql = @"
                CREATE TABLE IF NOT EXISTS Products (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Unit TEXT NOT NULL,
                    CriticalStockLevel REAL DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS StockLots (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductId INTEGER NOT NULL,
                    DateAdded DATETIME NOT NULL,
                    CostPrice REAL NOT NULL,
                    OriginalQuantity REAL NOT NULL,
                    RemainingQuantity REAL NOT NULL,
                    FOREIGN KEY(ProductId) REFERENCES Products(Id)
                );

                CREATE TABLE IF NOT EXISTS Transactions (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    ProductId INTEGER NOT NULL,
                    Date DATETIME NOT NULL,
                    Quantity REAL NOT NULL,
                    CalculatedCost REAL NOT NULL,
                    FOREIGN KEY(ProductId) REFERENCES Products(Id)
                );";

            connection.Execute(createTablesSql);

            // Migration: Transactions tablosuna Breakdown ve SalePrice sütunlarını kontrol et
            try 
            {
                // Breakdown sütunu yoksa ekle
                connection.Execute("ALTER TABLE Transactions ADD COLUMN Breakdown TEXT DEFAULT ''");
            } 
            catch { /* Sütun zaten varsa hata verir, yoksay */ }

            try 
            {
                // SalePrice sütunu yoksa ekle (Eski veritabanı uyumluluğu için, varsa zaten sorun yok)
                // NOT NULL constraint hatası alanlar için bu sütunun varlığından emin oluyoruz
                // Ancak "NOT NULL" constraint'i ALTER TABLE ile eklenen sütuna veremeyiz (varsayılan değer olmadan).
                // Eğer tablo zaten varsa ve SalePrice yoksa ekleriz.
                connection.Execute("ALTER TABLE Transactions ADD COLUMN SalePrice REAL DEFAULT 0");
            } 
            catch { /* Sütun zaten varsa hata verir, yoksay */ }

            try 
            {
                // Profit sütunu yoksa ekle (Legacy veritabanı uyumluluğu için)
                connection.Execute("ALTER TABLE Transactions ADD COLUMN Profit REAL DEFAULT 0");
            } 
            catch { /* Sütun zaten varsa hata verir, yoksay */ }
            
            try
            {
                // Eğer önceki çalışma sırasında (Kültür ayarlanmadan önce) '$' simgesi ile kayıt yapıldıysa düzelt
                connection.Execute("UPDATE Transactions SET Breakdown = REPLACE(Breakdown, '$', '₺') WHERE Breakdown LIKE '%$%'");
            }
            catch { /* Hata yok say */ }

            // Eğer ürün yoksa örnek veri ekle (Seed)
            try
            {
                /* 
                // Deployment için Seed özelliği kapatıldı.
                int productCount = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Products");
                if (productCount == 0)
                {
                    SeedDatabase((SqliteConnection)connection);
                }
                */
            }
            catch (Exception ex)
            {
                // Seed hatası uygulamayı durdurmasın
                System.Diagnostics.Debug.WriteLine($"Seed hatası: {ex.Message}");
            }
        }

        private void SeedDatabase(Microsoft.Data.Sqlite.SqliteConnection connection)
        {
             // 1. Ürünleri Ekle (Aşevi Konsepti)
             var products = new[]
             {
                 new Models.Product { Name = "Pirinç", Unit = "KG", CriticalStockLevel = 50 }, // 1
                 new Models.Product { Name = "Bulgur", Unit = "KG", CriticalStockLevel = 50 }, // 2
                 new Models.Product { Name = "Kuru Fasulye", Unit = "KG", CriticalStockLevel = 40 }, // 3
                 new Models.Product { Name = "Nohut", Unit = "KG", CriticalStockLevel = 40 }, // 4
                 new Models.Product { Name = "Kırmızı Mercimek", Unit = "KG", CriticalStockLevel = 30 }, // 5
                 new Models.Product { Name = "Yeşil Mercimek", Unit = "KG", CriticalStockLevel = 10 }, // 6
                 new Models.Product { Name = "Ayçiçek Yağı", Unit = "Litre", CriticalStockLevel = 100 }, // 7
                 new Models.Product { Name = "Zeytinyağı", Unit = "Litre", CriticalStockLevel = 20 }, // 8
                 new Models.Product { Name = "Domates Salçası", Unit = "KG", CriticalStockLevel = 25 }, // 9
                 new Models.Product { Name = "Biber Salçası", Unit = "KG", CriticalStockLevel = 10 }, // 10
                 new Models.Product { Name = "Makarna (Burgu)", Unit = "Paket", CriticalStockLevel = 200 }, // 11
                 new Models.Product { Name = "Şehriye", Unit = "KG", CriticalStockLevel = 20 }, // 12
                 new Models.Product { Name = "Un", Unit = "KG", CriticalStockLevel = 100 }, // 13
                 new Models.Product { Name = "Toz Şeker", Unit = "KG", CriticalStockLevel = 50 }, // 14
                 new Models.Product { Name = "Tuz", Unit = "KG", CriticalStockLevel = 20 }, // 15
                 new Models.Product { Name = "Çay", Unit = "KG", CriticalStockLevel = 30 }, // 16
                 new Models.Product { Name = "Soğan", Unit = "KG", CriticalStockLevel = 50 }, // 17
                 new Models.Product { Name = "Patates", Unit = "KG", CriticalStockLevel = 100 }, // 18
                 new Models.Product { Name = "Kıyma", Unit = "KG", CriticalStockLevel = 10 }, // 19
                 new Models.Product { Name = "Kuşbaşı Et", Unit = "KG", CriticalStockLevel = 10 } // 20
             };

             foreach (var p in products)
             {
                 connection.Execute("INSERT INTO Products (Name, Unit, CriticalStockLevel) VALUES (@Name, @Unit, @CriticalStockLevel)", p);
             }

             // 2. Rastgele Geçmiş ve Stok Oluşturma
             var rnd = new Random();
             int productId = 1;

             foreach (var p in products)
             {
                 // Her ürüne 2-4 parti stok girişi yapalım
                 int entries = rnd.Next(2, 5);
                 double totalStock = 0;

                 for (int i = 0; i < entries; i++)
                 {
                     double qty = rnd.Next(20, 100); // 20 ile 100 arası miktar
                     
                     // Fiyatları biraz dalgalandıralım (örn: pirinç 25-45 TL arası)
                     double basePrice = GetBasePrice(p.Name);
                     double price = basePrice + rnd.NextDouble() * (basePrice * 0.4) - (basePrice * 0.2); 
                     
                     // Tarihleri geriye doğru yayalım
                     int daysAgo = rnd.Next(10, 60);

                     var stockSql = @"
                        INSERT INTO StockLots (ProductId, DateAdded, CostPrice, OriginalQuantity, RemainingQuantity) 
                        VALUES (@ProductId, @DateAdded, @CostPrice, @OriginalQuantity, @RemainingQuantity)";
                     
                     connection.Execute(stockSql, new 
                     {
                         ProductId = productId,
                         DateAdded = DateTime.Now.AddDays(-daysAgo),
                         CostPrice = Math.Round(price, 2),
                         OriginalQuantity = qty,
                         RemainingQuantity = qty // Şimdilik hepsi duruyor gibi ekleyip sonra düşeceğiz
                     });
                     
                     totalStock += qty;
                 }

                 // Rastgele 1-3 çıkış işlemi (Usage) yapalım
                 int usages = rnd.Next(1, 4);
                 for (int i = 0; i < usages; i++)
                 {
                     double qtyUsed = rnd.Next(5, 30);
                     if (totalStock < qtyUsed) continue; // Stok yetmezse çıkış yapma

                     // Transaction kaydı ekle (Maliyet hesabı karmaşık olduğu için burada basitleştiriyoruz, 
                     // gerçek FIFO burada çalışmıyor seed olduğu için, ortalama fiyattan gömüyoruz)
                     // Amaç geçmişte veri görünsün.
                     
                     double avgPrice = GetBasePrice(p.Name);
                     decimal totalCost = (decimal)(qtyUsed * avgPrice);
                     string breakdown = $"{qtyUsed} {p.Unit} x {avgPrice:C2} (Seed Verisi)";

                     int daysAgo = rnd.Next(1, 9);
                     
                     var transSql = @"
                        INSERT INTO Transactions (ProductId, Date, Quantity, CalculatedCost, SalePrice, Profit, Breakdown) 
                        VALUES (@ProductId, @Date, @Quantity, @CalculatedCost, 0, 0, @Breakdown)";

                     connection.Execute(transSql, new 
                     {
                         ProductId = productId,
                         Date = DateTime.Now.AddDays(-daysAgo),
                         Quantity = qtyUsed,
                         CalculatedCost = totalCost,
                         Breakdown = breakdown
                     });

                     // StockLot'lardan düşme simülasyonu (Basitçe en eski tarihliden düşelim)
                     // Bu seed olduğu için %100 FIFO tutarlılığı aramıyoruz ama kalan miktar mantıklı olsun.
                     // Rastgele bir lot'tan düşelim ki "Remaining" 0 olmayanlar da olsun.
                     connection.Execute(@"
                        UPDATE StockLots 
                        SET RemainingQuantity = RemainingQuantity - @Qty 
                        WHERE Id = (SELECT Id FROM StockLots WHERE ProductId = @Pid AND RemainingQuantity >= @Qty LIMIT 1)",
                        new { Qty = qtyUsed, Pid = productId });
                        
                     totalStock -= qtyUsed;
                 }

                 productId++;
             }
        }

        private double GetBasePrice(string productName)
        {
            return productName switch
            {
                "Pirinç" => 45.0,
                "Bulgur" => 25.0,
                "Kuru Fasulye" => 70.0,
                "Nohut" => 60.0,
                "Kırmızı Mercimek" => 35.0,
                "Yeşil Mercimek" => 40.0,
                "Ayçiçek Yağı" => 55.0,
                "Zeytinyağı" => 250.0,
                "Domates Salçası" => 80.0,
                "Biber Salçası" => 120.0,
                "Makarna (Burgu)" => 15.0,
                "Şehriye" => 20.0,
                "Un" => 400.0 / 50.0, // 50kg çuval fiyatından birim
                "Toz Şeker" => 35.0,
                "Tuz" => 10.0,
                "Çay" => 180.0,
                "Soğan" => 12.0,
                "Patates" => 15.0,
                "Kıyma" => 450.0,
                "Kuşbaşı Et" => 500.0,
                _ => 50.0
            };
        }

        // --- CRUD Methods ---

        public void AddProduct(Models.Product product)
        {
            try
            {
                using var connection = GetConnection();
                string sql = "INSERT INTO Products (Name, Unit, CriticalStockLevel) VALUES (@Name, @Unit, @CriticalStockLevel)";
                connection.Execute(sql, product);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ürün eklenirken hata oluştu: {ex.Message}", ex);
            }
        }

        public void UpdateProduct(Models.Product product)
        {
            try
            {
                using var connection = GetConnection();
                string sql = "UPDATE Products SET Name = @Name, Unit = @Unit, CriticalStockLevel = @CriticalStockLevel WHERE Id = @Id";
                connection.Execute(sql, product);
            }
            catch (Exception ex)
            {
                throw new Exception($"Ürün güncellenirken hata oluştu: {ex.Message}", ex);
            }
        }

        public int GetProductCount()
        {
            using var connection = GetConnection();
            return connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Products");
        }

        public IEnumerable<Models.Product> GetProductsPaged(int page, int pageSize)
        {
            try
            {
                using var connection = GetConnection();
                var offset = (page - 1) * pageSize;
                var sql = @"
                    SELECT 
                        p.*, 
                        IFNULL(SUM(s.RemainingQuantity), 0) as CurrentStock
                    FROM Products p
                    LEFT JOIN StockLots s ON p.Id = s.ProductId
                    GROUP BY p.Id
                    LIMIT @PageSize OFFSET @Offset";
                return connection.Query<Models.Product>(sql, new { PageSize = pageSize, Offset = offset });
            }
            catch (Exception ex)
            {
                throw new Exception($"Ürünler listelenirken hata oluştu: {ex.Message}", ex);
            }
        }

        // Tümü (ComboBox vb. için)
        public IEnumerable<Models.Product> GetAllProducts()
        {
            using var connection = GetConnection();
            return connection.Query<Models.Product>("SELECT * FROM Products");
        }

        public int GetSalesHistoryCount()
        {
            using var connection = GetConnection();
            return connection.ExecuteScalar<int>("SELECT COUNT(*) FROM Transactions");
        }

        public IEnumerable<Models.SalesHistoryModel> GetSalesHistoryPaged(int page, int pageSize)
        {
             using var connection = GetConnection();
             var offset = (page - 1) * pageSize;
             var sql = @"
                SELECT 
                    t.Date,
                    p.Name as ProductName,
                    t.Quantity,
                    t.CalculatedCost,
                    t.Breakdown
                FROM Transactions t
                JOIN Products p ON t.ProductId = p.Id
                ORDER BY t.Date DESC
                LIMIT @PageSize OFFSET @Offset";
             return connection.Query<Models.SalesHistoryModel>(sql, new { PageSize = pageSize, Offset = offset });
        }

        public int GetStockHistoryCount()
        {
            using var connection = GetConnection();
            return connection.ExecuteScalar<int>("SELECT COUNT(*) FROM StockLots");
        }

        public IEnumerable<Models.StockHistoryModel> GetStockHistoryPaged(int page, int pageSize)
        {
            using var connection = GetConnection();
            var offset = (page - 1) * pageSize;
            var sql = @"
                SELECT 
                    s.DateAdded,
                    p.Name as ProductName,
                    s.CostPrice,
                    s.OriginalQuantity,
                    s.RemainingQuantity
                FROM StockLots s
                JOIN Products p ON s.ProductId = p.Id
                ORDER BY s.DateAdded DESC
                LIMIT @PageSize OFFSET @Offset";
            return connection.Query<Models.StockHistoryModel>(sql, new { PageSize = pageSize, Offset = offset });
        }

        public void AddStockEntry(Models.StockLot stockLot)
        {
            try
            {
                using var connection = GetConnection();
                stockLot.DateAdded = DateTime.Now;
                stockLot.RemainingQuantity = stockLot.OriginalQuantity; // İlk girişte kalan = orijinal

                string sql = @"
                    INSERT INTO StockLots (ProductId, DateAdded, CostPrice, OriginalQuantity, RemainingQuantity) 
                    VALUES (@ProductId, @DateAdded, @CostPrice, @OriginalQuantity, @RemainingQuantity)";
                
                connection.Execute(sql, stockLot);
            }
            catch (Exception ex)
            {
                throw new Exception($"Stok girişi yapılırken hata oluştu: {ex.Message}", ex);
            }
        }

        public double GetTotalStock(int productId)
        {
            try
            {
                using var connection = GetConnection();
                return connection.ExecuteScalar<double>("SELECT IFNULL(SUM(RemainingQuantity), 0) FROM StockLots WHERE ProductId = @ProductId", new { ProductId = productId });
            }
            catch (Exception ex)
            {
                throw new Exception($"Stok miktarı hesaplanırken hata oluştu: {ex.Message}", ex);
            }
        }

        // --- FIFO Core Logic ---

        // Metod ismi MakeSale -> MakeUsage olarak değişti (Aşevi mantığı)
        // Metod ismi MakeSale -> MakeUsage olarak değişti (Aşevi mantığı)
        public string MakeUsage(int productId, double quantityUsed, string unit)
        {
            using var connection = GetConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // 1. Toplam stok kontrolü
                double totalStock = connection.ExecuteScalar<double>(
                    "SELECT IFNULL(SUM(RemainingQuantity), 0) FROM StockLots WHERE ProductId = @ProductId", 
                    new { ProductId = productId }, 
                    transaction);

                if (totalStock < quantityUsed)
                {
                    throw new InvalidOperationException($"Yetersiz Stok! Mevcut: {totalStock}, İstenen: {quantityUsed}");
                }

                // 2. FIFO için partileri çek (Eskiden yeniye)
                var lots = connection.Query<Models.StockLot>(
                    "SELECT * FROM StockLots WHERE ProductId = @ProductId AND RemainingQuantity > 0 ORDER BY DateAdded ASC", 
                    new { ProductId = productId }, 
                    transaction).ToList();

                double needed = quantityUsed;
                decimal totalCost = 0;
                List<string> breakdownList = new List<string>();

                foreach (var lot in lots)
                {
                    if (needed <= 0) break;

                    double deduction = 0;

                    if (lot.RemainingQuantity <= needed)
                    {
                        // Bu partiyi bitir
                        deduction = lot.RemainingQuantity;
                        totalCost += (decimal)deduction * lot.CostPrice;
                        
                        lot.RemainingQuantity = 0;
                        needed -= deduction;
                    }
                    else
                    {
                        // Bu partiden ihtiyacın kadar al
                        deduction = needed;
                        totalCost += (decimal)deduction * lot.CostPrice;

                        lot.RemainingQuantity -= deduction;
                        needed = 0;
                    }

                    breakdownList.Add($"{deduction} {unit} x {lot.CostPrice:C2}");

                    // Veritabanını güncelle
                    connection.Execute(
                        "UPDATE StockLots SET RemainingQuantity = @RemainingQuantity WHERE Id = @Id", 
                        new { lot.RemainingQuantity, lot.Id }, 
                        transaction);
                }

                string breakdown = string.Join(", ", breakdownList);

                // ComboBox için tüm ürünleri getir
                // This line seems to be misplaced here, as this is a data access layer method.
                // It's likely intended for a UI layer (e.g., StockEntryView.xaml.cs)
                // var products = _db.GetAllProducts().ToList();
                // CmbProducts.ItemsSource = products;

                // 3. Kullanım Kaydı (Transaction) Oluştur
                var usageRecord = new Models.Transaction
                {
                    ProductId = productId,
                    Date = DateTime.Now,
                    Quantity = quantityUsed,
                    CalculatedCost = totalCost,
                    SalePrice = 0, // Legacy column constraint fix
                    Profit = 0, // Legacy column constraint fix
                    Breakdown = breakdown
                };

                // Breakdown sütunu var mı kontrolü için migration mantığı InitializeDatabase'de olmalı ama
                // burada basitçe INSERT'e ekliyoruz. Eğer sütun yoksa hata verebilir, bu yüzden InitializeDatabase'i güncellemek şart.
                string insertTransSql = @"
                    INSERT INTO Transactions (ProductId, Date, Quantity, CalculatedCost, SalePrice, Profit, Breakdown) 
                    VALUES (@ProductId, @Date, @Quantity, @CalculatedCost, @SalePrice, @Profit, @Breakdown)";

                connection.Execute(insertTransSql, usageRecord, transaction);

                transaction.Commit();

                return $"{breakdown}\nToplam Maliyet: {totalCost:C2}";
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw; // Hatayı yukarı fırlat (UI yakalasın)
            }
        }

        // --- Backup & Restore ---

        public void BackupDatabase(string destinationPath)
        {
            try
            {
                string dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StokTakip.db");

                // Veritabanı dosyasının varlığını kontrol et
                if (!File.Exists(dbPath))
                    throw new FileNotFoundException("Veritabanı dosyası bulunamadı!");

                // SQLite genellikle dosyayı kilitler, bu yüzden güvenli kopya için bağlantıları temizle
                SqliteConnection.ClearAllPools();

                File.Copy(dbPath, destinationPath, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Yedekleme sırasında hata oluştu: {ex.Message}", ex);
            }
        }

        public void RestoreDatabase(string sourcePath)
        {
            try
            {
                // 1. Dosya Doğrulama (Basitçe SQLite başlığına veya tablo varlığına bakılabilir)
                if (!IsValidDatabase(sourcePath))
                    throw new Exception("Seçilen dosya geçerli bir Stok Takip veritabanı değil veya bozuk.");

                string currentDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StokTakip.db");

                // 2. Mevcut bağlantıları kopar
                SqliteConnection.ClearAllPools();

                // 3. Dosyayı değiştir ve üzerine yaz
                File.Copy(sourcePath, currentDbPath, true);
            }
            catch (Exception ex)
            {
                throw new Exception($"Geri yükleme sırasında hata oluştu: {ex.Message}", ex);
            }
        }

        private bool IsValidDatabase(string filePath)
        {
            try
            {
                // Geçici olarak bağlanıp tabloları kontrol et
                using var conn = new SqliteConnection($"Data Source={filePath}");
                conn.Open();
                
                // Kritik tabloların varlığını kontrol et
                var tables = conn.Query<string>("SELECT name FROM sqlite_master WHERE type='table' AND name IN ('Products', 'StockLots', 'Transactions')").ToList();
                
                return tables.Count == 3;
            }
            catch
            {
                return false;
            }
        }
    }
}
