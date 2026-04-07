# 🎓 .NET 8 Nullable Hatası ve Veritabanı Etkileşimi: Neden Patladık?

Bu doküman, projede yaşadığımız **"City alanı yüzünden alınan 500 Hatası"** üzerinden, .NET'in yeni nullable yapısını ve veritabanı kısıtlamalarını anlamak için hazırlanmıştır.

---

## 🛑 Yaşadığımız Sorun Neydi?

**Senaryo:** `SignUp` (Kayıt Ol) sayfamızda Ad, Soyad, Email ve Şifre istiyoruz. Ancak arka plandaki `ApplicationUser` modelimizde `City` (Şehir) adında bir alan vardı ve formda bu alan yoktu.

**Sonuç:** Kayıt ol butonuna bastığımızda uygulama **"Internal Server Error"** vererek patladı.

### 🔍 Teknik Sebep: .NET 8 ve "Nullable Reference Types"

Eskiden (örn. .NET Core 3.1), `string` veri tipi varsayılan olarak null değer alabilirdi. Ancak .NET 6 ve sonrası (özellikle .NET 8) ile gelen projelerde `.csproj` dosyasında şu ayar açık gelir:

```xml
<Nullable>enable</Nullable>
```

Bu ayar açıkken:

1.  **`public string City { get; set; }`**  
    👉 "Bu alan kesinlikle dolu olmalı, NULL OLAMAZ" demektir.
    
2.  **`public string? City { get; set; }`**  
    👉 "Bu alan dolu olabilir de olmayabilir de (Nullable)" demektir.

---

## 🛠️ Entity Framework (Migration) Nasıl Davrandı?

Entity Framework Core, yazdığınız C# modeline bakarak SQL tablosunu oluşturur.

### Hatalı Durum (İlk Halimiz)

Kodumuz şuydu:
```csharp
public string City { get; set; } // Nullable değil!
```

EF Core bunu görünce migration dosyasını şöyle oluşturdu:
```csharp
table: "AspNetUsers",
nullable: false // <--- DİKKAT!
```

**SQL Veritabanındaki Karşılığı:**
```sql
City NVARCHAR(MAX) NOT NULL
```

### Sorun Anı 💥
Kullanıcı formu doldurduğunda `SignupDto` içinde `City` yoktu. Dolayısıyla `ApplicationUser` oluşturulurken `City`'ye değer atanmadı.
Modelde `City` null kaldı. EF Core bunu veritabanına `INSERT` etmeye çalıştığında, veritabanı **"Dur! Bu kolon NOT NULL (boş olamaz) ama sen bana NULL gönderiyorsun!"** diyerek işlemi reddetti.

---

## ✅ Çözüm: Ne Yaptık?

Modelimizde küçük ama kritik bir değişiklik yaptık:

```diff
- public string City { get; set; }
+ public string? City { get; set; } // Soru işareti (?) ekledik
```

Bunun üzerine yeni bir migration aldığımızda EF Core şunu anladı: *"Tamam, geliştirici bu alanın boş olabileceğine izin veriyor."*

**Yeni Migration Çıktısı:**
```csharp
table: "AspNetUsers",
nullable: true // <--- Artık sorun yok
```

**SQL Veritabanındaki Karşılığı:**
```sql
City NVARCHAR(MAX) NULL
```

Artık `City` göndermesek bile veritabanı bunu kabul ediyor ve o hücreyi boş (`NULL`) bırakıyor.

---

## 💡 Kıssadan Hisse: Dikkat Edilmesi Gerekenler

Bir backend geliştiricisi olarak model tasarlarken şu soruları sormalısınız:

1.  **Bu Veri Zorunlu mu?**  
    *   Kullanıcı bunu girmek **zorunda** mı? (Örn: Email, Şifre)
    *   Formda bu alan var mı?
    *   Eğer zorunlu ise: `string` (soru işaretsiz) tanımlayın ve DTO/Form tarafında da zorunlu tutun.

2.  **DTO ile Entity Uyumu**  
    *   DTO (Data Transfer Object), kullanıcının girdiği veridir. Entity ise veritabanı tablosudur.
    *   Eğer Entity'de zorunlu bir alan varsa (`NOT NULL`), DTO'da da mutlaka olmalı ve map edilmelidir.
    *   Eğer DTO'da yoksa (bizim `City` örneği gibi), Entity tarafında o alan **mutlaka nullable (`?`)** olmalı veya bir **varsayılan değer (`default value`)** atanmalıdır.

3.  **Migration Kontrolü**  
    *   `dotnet ef migrations add` dedikten sonra oluşan dosyaya göz atın.
    *   Özellikle `nullable: false` olan kolonları inceleyin. "Ben bu veriyi her zaman sağlayabilecek miyim?" diye düşünün.

---

### Özet Tablo

| C# Tanımı | Anlamı | SQL Karşılığı | Tehlike Seviyesi |
| :--- | :--- | :--- | :--- |
| `string Name` | Kesinlikle dolu olmalı | `NOT NULL` | 🔴 Yüksek (Veri gelmezse patlar) |
| `string? City` | Boş olabilir | `NULL` | 🟢 Güvenli (Opsiyonel alanlar için) |
| `string Role = "User"` | Varsayılan değeri var | `NOT NULL` | 🟢 Güvenli (Gelmezse varsayılanı yazar) |
