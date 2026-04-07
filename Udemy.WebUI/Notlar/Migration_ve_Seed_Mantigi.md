# Migration (Göç) ve Seed (Tohumlama) Mantığı

Bu doküman, Mikroservis projemizde veritabanlarını nasıl otomatik hazırladığımızı, neden bazı yerlerde "Migration" yapıp bazı yerlerde yapmadığımızı anlatır.

---

## 1. Migration (Veritabanı Göçü) Nedir?

**Migration**, ilişkisel veritabanlarında (SQL Server, PostgreSQL, MySQL) veri yapısının (Schema) kod ile senkronize edilmesidir.

**SQL veritabanları "katı" (strict) kurallara sahiptir:**
*   Veri eklemeden önce **Tablo** olmalı.
*   Tablonun hangi **kolonları** olacağı kesin olmalı (String mi? Int mi? Boş olabilir mi?).
*   Bu tablo yoksa `INSERT` işlemi yapamazsınız, hata alırsınız.

Bu yüzden proje ilk ayağa kalktığında veritabanında tablolar yoksa, **kodlarımız çalışmaz**.

### Çözüm: Auto-Migration
Projeyi başlattığımız an (`Program.cs` içinde), kodun veritabanına gidip:
*"Tablolar var mı? Yoksa oluştur. Varsa ve modelde değişiklik yaptıysam güncelle."*
demesine **Auto-Migration** denir.

---

## 2. NoSQL (MongoDB) Farkı

Catalog Microservice'inde **MongoDB** kullanıyoruz. MongoDB **NoSQL** bir veritabanıdır ve yapısı **esnektir (Schema-less)**.

*   Önceden tablo (collection) oluşturmaya gerek yoktur.
*   Önceden kolon tanımlamaya (sabit şema) gerek yoktur.
*   Kodunuz `db.Products.Insert(...)` dediği anda, MongoDB bakar:
    *   `Products` diye bir yer var mı? Yoksa **oluşturur**.
    *   Sonra veriyi içine atar.

**Bu yüzden Catalog API'de "Migration" yapmadık.** Kod çalıştığı an veritabanı kendi kendine oluşur.

---

## 3. Seed Data (Veri Tohumlama) Nedir?

`Migration` veritabanının **iskeletini** (tabloları) kurar.
`Seed Data` ise veritabanının **içini** (başlangıç verilerini) doldurur.

Örneğin, proje ilk açıldığında "Kategoriler" tablosu boş gelirse kullanıcı kurs oluşturamaz. Bu yüzden proje başlarken:
*"Veritabanı boş mu? Boşsa şu standart kategorileri (Yazılım, İşletme vb.) ekle"* 
dediğimiz işleme **Seeding** denir.

Catalog API'de yaptığımız tam olarak buydu. Tablo oluşturmak (migration) için değil, **içine varsayılan veri koymak için** kod yazdık.

---

## 4. Projemizde Hangi Mikroservis Ne Yapıyor?

Şu an 3 farklı veritabanı teknolojisi ve yaklaşımı kullanıyoruz:

### A. Order API (SQL Server + Entity Framework Core)
**Durum:** SQL Server, tablo yapısı ister. EF Core kullanıyoruz.
**İşlem:** `Migrate()`
**Kod (`Program.cs`):**

```csharp
// Veritabanına bağlanır ve bekleyen migration'ları (tablo oluşumlarını) uygular.
using (var scope = app.Services.CreateScope())
{
    var orderDbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    orderDbContext.Database.Migrate(); // TABLO YOKSA OLUŞTURUR
}
```

### B. Discount API (PostgreSQL + Dapper)
**Durum:** PostgreSQL de tablo yapısı ister. Ama burada EF Core değil, **Dapper** (Micro ORM) kullanıyoruz. Dapper'da hazır "Migrate" komutu yoktur, SQL yazmamız gerekir.
**İşlem:** Manual SQL Script Execution
**Kod (`DbMigrationHelper.cs`):**

```csharp
// Elle SQL komutu gönderip tabloyu biz oluşturuyoruz.
var createTableSql = @"
    CREATE TABLE IF NOT EXISTS discount (
        id SERIAL PRIMARY KEY,
        code VARCHAR(50) NOT NULL,
        ...
    );
";
connection.Execute(createTableSql);
```

### C. Catalog API (MongoDB)
**Durum:** Tablo oluşturmaya gerek yok. Ama kategori listesi boş gelmesin istedik.
**İşlem:** Seed Data (Idempotent - Tekrar Eden Kontrolüyle)
**Kod (`DatabaseSeedHelper.cs`):**

```csharp
// Önce koleksiyonu al (Yoksa Mongo o an oluşturur)
var categoryCollection = database.GetCollection<Category>("categories");

// Eksik olanları kontrol et ve ekle
if (!exist)
{
    await categoryCollection.InsertOneAsync(new Category { Name = "Yazılım" });
}
```

---

## Özet Tablo

| Servis | DB | Teknoloji | Yapılan İşlem | Neden? |
| :--- | :--- | :--- | :--- | :--- |
| **Order API** | SQL Server | EF Core | `Database.Migrate()` | Tabloları oluşturmak için. |
| **Discount API** | PostgreSQL | Dapper | Manual `CREATE TABLE` | Tabloları oluşturmak için (EF olmadığı için elle yazdık). |
| **Catalog API** | MongoDB | Mongo Driver | `Insert (Seed)` | Tablo zaten otomatik oluşuyor, biz **başlangıç verisi** ekledik. |

---

## 5. Merak Edilen: ORM Kullananlar Hep İlişkisel DB mi Kullanır?

**Genellikle Evet.** 

ORM (Object Relational Mapping), adından da anlaşılacağı gibi **Nesne (Object)** ile **İlişkisel Veritabanı (Relational DB)** arasında köprü kurar.

### Projemizdeki Durum:

1.  **EF Core (Entity Framework Core)** -> **SQL Server**
    *   Bu bir ORM'dir. C# class'larını SQL tablolarına dönüştürür.
    *   İlişkisel veritabanı (RDBMS) için tasarlanmıştır.

2.  **Dapper** -> **PostgreSQL**
    *   Bu bir "Micro ORM"dir. Yine ilişkisel veritabanları (SQL) ile çalışır.
    *   SQL kodunu elle yazarız ama sonucu C# nesnesine o çevirir.

3.  **Mongo Driver** -> **MongoDB**
    *   Bu bir ORM değildir. Bu bir **Driver (Sürücü)** veya ODM (Object Document Mapper) olarak geçer.
    *   MongoDB ilişkisel (Relational) değildir. Tablolar arası "Join" mantığı SQL'deki gibi değildir.
    *   Bu yüzden buna teknik olarak "ORM" denmez, ama işlevi benzerdir: Veriyi C# nesnesine çevirir.

**Özet:** Eğer bir projede "Ben ORM kullanıyorum" deniyorsa, %99 ihtimalle arkada **SQL Server, MySQL, PostgreSQL, Oracle** gibi bir ilişkisel veritabanı vardır. NoSQL (MongoDB, Redis, Cassandra) için genellikle kendi özel sürücüleri kullanılır.

