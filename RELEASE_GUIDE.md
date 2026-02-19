# Stok Takip Uygulaması - Yayınlama ve GitHub Rehberi

Bu belge, uygulamanızı başkalarıyla paylaşmak (.exe oluşturmak) ve GitHub üzerinde v1.0.0 sürümü olarak yayınlamak için gerekli adımları içerir.

## 1. Uygulamanızı Paylaşma (Portable - Kurulumsuz)

Uygulamanız şu an **"Portable" (Taşınabilir)** olarak ayarlandı. Yani kurulum gerekmez, `.exe` dosyası USB bellekte veya herhangi bir klasörde çalışır.

### Adım 1: Terminalden Çıktı Al (Publish)
Terminal'i açın ve proje klasöründe (`StokTakip` dizini) şu komutu çalıştırın:

```powershell
dotnet publish StokTakip.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

### Adım 2: Dosyayı Bul ve Paylaş
Komut bitince şu klasöre gidin:
`StokTakip\bin\Release\net9.0-windows\win-x64\publish\`

Buradaki **`StokTakip.exe`** dosyasını (ve varsa yanındaki veritabanı yedeğini) bir klasöre koyup **zip** yaparak arkadaşlarınızla paylaşabilirsiniz.
*   **Not:** Bu dosya her bilgisayarda çalışır (.NET yüklü olmasa bile).
*   **Kısayol:** Kullanıcılar dosyayı masaüstüne atabilir veya `Sağ Tık -> Gönder -> Masaüstü (Kısayol Oluştur)` diyebilirler.

---

## 2. GitHub Reposu Sıfırlama ve Yükleme (Tertemiz Sayfa)

Eğer git geçmişiniz karıştıysa veya sıfırdan başlamak istiyorsanız:

### Adım 1: Git Geçmişini Sil ve Başlat
Terminal'de **proje ana klasörünüzde** (genellikle `FIFO-Database` veya `StokTakip`in olduğu yer) şu komutları sırasıyla çalıştırın:

```powershell
# 1. Varsa eski git klasörünü sil (Sıfırlar)
Remove-Item -Recurse -Force .git -ErrorAction SilentlyContinue

# 2. Yeni, tertemiz bir repo başlat
git init

# 3. Dosyaları ekle
git add .
git commit -m "İlk Sürüm: Stok Takip v1.0.0"
```

### Adım 2: GitHub'a Gönder
GitHub'da **boş bir repository** oluşturduktan sonra (örn: `StokTakip`), size verilen komutları girin:

```powershell
git branch -M main
git remote add origin https://github.com/KULLANICI_ADI/StokTakip.git
git push -u origin main
```

---

## 3. GitHub'da v1.0.0 Sürümü (Release) Yayınlama

Kodlarınız yüklendikten sonra, "Release" (Sürüm) oluşturarak `.exe` dosyasını da buraya ekleyebilirsiniz.

1.  GitHub reponuzun ana sayfasına gidin.
2.  Sağ taraftaki **Releases** kısmına tıklayın (veya "Create a new release" yazısını bulun).
3.  **Draft a new release** butonuna tıklayın.
4.  **Choose a tag** kısmına tıklayıp `v1.0.0` yazın ve "Create new tag" seçeneğini seçin.
5.  **Release title** kısmına `v1.0.0 - İlk Sürüm` yazın.
6.  **Describe this release** kısmına sürüm notlarını (yeni özellikler, düzeltmeler) ekleyin.
7.  **Attach binaries by dropping them here...** kısmına, 1. adımda oluşturduğunuz `publish` klasöründeki **`StokTakip.exe`** dosyasını sürükleyip bırakın.
8.  **Publish release** butonuna tıklayın.

Tebrikler! 🎉 Artık projeniz GitHub'da v1.0.0 olarak yayınlandı ve kullanıcılar `.exe` dosyasını indirip direkt kullanabilirler.
