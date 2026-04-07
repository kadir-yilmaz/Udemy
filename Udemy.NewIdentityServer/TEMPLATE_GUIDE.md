# Duende IdentityServer Şablon Yapısı Rehberi

> **Proje**: `Udemy.NewIdentityServer`  
> **Şablon**: Duende IdentityServer with ASP.NET Core Identity (`isaspid`)  
> **Varsayılan Port**: `https://localhost:5001`

---

## 📁 Proje Yapısı Genel Bakış

```
Udemy.NewIdentityServer/
├── Data/                    # Entity Framework yapılandırması
├── Models/                  # ApplicationUser vb. modeller
├── Pages/                   # Razor Pages (Login, Register, Consent, vb.)
├── Properties/              # launchSettings.json
├── wwwroot/                 # Statik dosyalar (CSS, JS, images)
├── Config.cs                # Client, Scope, Resource tanımları
├── HostingExtensions.cs     # Service ve Pipeline konfigürasyonu
├── Program.cs               # Uygulama giriş noktası
├── SeedData.cs              # Veritabanı başlangıç verileri
└── appsettings.json         # Uygulama ayarları
```

---

## 🔑 Ana Dosyalar

### 1. `Config.cs` — OAuth2/OIDC Yapılandırması

**Amaç**: API kaynaklarını, scope'ları ve client uygulamaları tanımlar.

| Property | Açıklama |
|----------|----------|
| `IdentityResources` | OpenID Connect claim'leri (openid, profile, email) |
| `ApiScopes` | API erişim izinleri (scope1, scope2, vb.) |
| `Clients` | Token alacak uygulamalar (WebUI, Mobile, vb.) |

> [!IMPORTANT]
> **Mevcut projenizdeki `Config.cs`'i buraya taşıyacaksınız!**  
> `ApiResources` ve `Clients` tanımlarınız burada olacak.

---

### 2. `HostingExtensions.cs` — Servis Konfigürasyonu

**Amaç**: DI container ve middleware pipeline yapılandırması.

**İki ana metod:**

```csharp
ConfigureServices()     // AddIdentityServer, AddDbContext, AddIdentity
ConfigurePipeline()     // UseIdentityServer, UseAuthorization, MapRazorPages
```

**Önemli Servisler:**
- `AddDbContext<ApplicationDbContext>` — SQLite (varsayılan)
- `AddIdentity<ApplicationUser, IdentityRole>` — ASP.NET Identity
- `AddIdentityServer()` — Duende IdentityServer çekirdeği
- `AddInMemoryClients(Config.Clients)` — Config.cs'den client'ları yükler

---

### 3. `Program.cs` — Uygulama Giriş Noktası

**Önemli Özellikler:**
- Serilog ile loglama
- `/seed` parametresi ile veritabanı seed

```powershell
# Veritabanını seed etmek için:
dotnet run /seed
```

---

### 4. `SeedData.cs` — Başlangıç Verileri

**Amaç**: İlk çalıştırmada örnek kullanıcı oluşturur.

| Kullanıcı | Email | Şifre |
|-----------|-------|-------|
| `alice` | AliceSmith@email.com | Pass123$ |
| `bob` | BobSmith@email.com | Pass123$ |

> [!NOTE]
> Production'da bu dosyayı devre dışı bırakın veya kendi kullanıcı seed mantığınızı yazın.

---

## 📂 Pages Klasörü (Razor Pages)

**Neden MVC değil Razor Pages?**  
Duende, Login/Logout gibi tek sayfalık işlemler için Razor Pages tercih ediyor — daha basit, daha az boilerplate.

### Alt Klasörler ve Amaçları

| Klasör | Açıklama | Sayfalar |
|--------|----------|----------|
| **Account/** | Giriş, çıkış, kayıt işlemleri | Login, Logout, AccessDenied |
| **Consent/** | OAuth2 izin onay ekranı | Index (izin ver/reddet) |
| **Device/** | Device Flow desteği | UserCode, Callback |
| **Diagnostics/** | Hata ayıklama araçları | Index (token bilgileri) |
| **ExternalLogin/** | Harici sağlayıcılar (Google, vb.) | Challenge, Callback |
| **Grants/** | Verilen izinlerin yönetimi | Index (token iptal) |
| **Home/** | Ana sayfa | Index, Error |
| **Redirect/** | Token sonrası yönlendirme | Index |
| **ServerSideSessions/** | Sunucu taraflı oturum yönetimi | Index |
| **Shared/** | Ortak layout dosyaları | _Layout.cshtml |
| **Ciba/** | Client-Initiated Backchannel Auth | (İleri seviye) |

---

## 📂 Data Klasörü

| Dosya | Açıklama |
|-------|----------|
| `ApplicationDbContext.cs` | EF Core DbContext (Identity tabloları) |
| `Migrations/` | Veritabanı migration dosyaları |

**Varsayılan**: SQLite kullanıyor. SQL Server'a geçiş için `appsettings.json` ve `HostingExtensions.cs` düzenle.

---

## 📂 wwwroot Klasörü

Statik dosyalar:
- `css/` — Bootstrap temalı stiller
- `js/` — Signin-redirect.js vb.
- `lib/` — Bootstrap, jQuery kütüphaneleri

---

## 🔄 Eski Proje vs Yeni Proje Karşılaştırması

| Özellik | Eski (IdentityServer4) | Yeni (Duende) |
|---------|------------------------|---------------|
| **Framework** | .NET Core 3.1 | .NET 8.0 |
| **Namespace** | `IdentityServer4` | `Duende.IdentityServer` |
| **UI** | MVC Controllers | Razor Pages |
| **Konfigürasyon** | `Startup.cs` | `HostingExtensions.cs` |
| **Veritabanı** | SQL Server | SQLite (değiştirilebilir) |

---

## 🚀 Sonraki Adımlar

1. **Port değiştir**: `launchSettings.json` → 5005 veya başka bir port
2. **Config.cs güncelle**: Mevcut `ApiResources`, `ApiScopes`, `Clients` tanımlarını taşı
3. **Veritabanını değiştir**: SQLite → SQL Server
4. **Migration çalıştır**: `dotnet ef database update`
5. **Seed et**: `dotnet run /seed`

---

## 📚 Faydalı Komutlar

```powershell
# Projeyi çalıştır
dotnet run

# Seed ile çalıştır (ilk kurulumda)
dotnet run /seed

# Migration ekle
dotnet ef migrations add InitialCreate

# Veritabanını güncelle
dotnet ef database update
```
