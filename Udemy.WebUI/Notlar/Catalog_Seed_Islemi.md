# Catalog API Seed (Veri Tohumlama) İşlemi Yönergesi

Bu doküman, Catalog API'de varsayılan kategorilerin (Yazılım, İşletme vb.) proje her başladığında otomatik olarak eklenmesini sağlayan yapıyı anlatır.

---

## 1. Adım: Seed Helper Dosyasının Oluşturulması

Veritabanı işlemlerini yapmak için özel bir yardımcı sınıf (Helper) oluşturduk.

**Konum:** `Udemy.Catalog.API\Services\DatabaseSeedHelper.cs`

**Yaptığı İşler:**
1.  **Bağlantı Kurar:** `DatabaseOptions` üzerinden MongoDB bağlantı bilgilerini okur.
2.  **Kontrol Eder:** Eklenecek her kategori için (örneğin "Yazılım") veritabanında var mı diye bakar.
3.  **Ekler (Idempotent):** Sadece *eksik* olanları ekler. Böylece proje her başladığında aynı veriyi tekrar tekrar eklemez.

**İncelemek İçin:**
> [DatabaseSeedHelper.cs](file:///c:/Users/kadir/OneDrive/Desktop/Projeler/Udemy/Catalog/Udemy.Catalog.API/Services/DatabaseSeedHelper.cs) dosyasına bakabilirsiniz.

---

## 2. Adım: Program.cs Entegrasyonu

Oluşturduğumuz bu `Helper` sınıfının projenin açılışında çalışması gerekiyordu. Bunun için `Program.cs` dosyasının en altına kod ekledik.

**Konum:** `Udemy.Catalog.API\Program.cs`

**Eklenen Kodlar (Satır ~64):**

```csharp
// Seed Data
using (var scope = app.Services.CreateScope())
{
    var serviceProvider = scope.ServiceProvider;
    // Helper sınıfımızdaki metodu çağırıyoruz
    await Udemy.Catalog.API.Services.DatabaseSeedHelper.SeedCategoriesAsync(serviceProvider);
}
```

**Bu Kod Ne İşe Yarıyor?**
*   `CreateScope()`: Geçici bir servis kapsamı oluşturur (Database bağlantısı vb. servisleri alabilmek için).
*   `SeedCategoriesAsync(...)`: Yazdığımız metodu çalıştırır ve veritabanına eksik kategorileri basar.

**İncelemek İçin:**
> [Program.cs](file:///c:/Users/kadir/OneDrive/Desktop/Projeler/Udemy/Catalog/Udemy.Catalog.API/Program.cs#L64) dosyasının son satırlarına bakabilirsiniz.

---

## 3. Sonuç

Bu iki işlem sayesinde:

1.  Projeyi `Docker` üzerinden veya `Visual Studio` ile başlattığınızda.
2.  `App` ayağa kalkar kalkmaz veritabanına bakar.
3.  "Yazılım" kategorisi yoksa ekler.
4.  Kullanıcı arayüzünde "Yazılım" kategorisi hazır gelir.

---

## Ekstra: Discount API ve Order API

Aynı mantık diğerlerinde de var:

*   **Discount API:** `Program.cs` içinde `DbMigrationHelper.EnsureDatabaseSetup` çağrılıyor. (Tablo oluşturuyor)
*   **Order API:** `Program.cs` içinde `.Database.Migrate()` çağrılıyor. (Tabloları kuruyor)
