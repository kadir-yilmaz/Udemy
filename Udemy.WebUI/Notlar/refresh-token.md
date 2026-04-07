# Refresh Token Detayları

## Refresh Token JWT mi?

**Hayır!** Refresh token **Opaque Token**'dır (rastgele karakter dizisi).

```
E4D2F8C1A3B5...7F9E (64 karakter)
```

## JWT vs Opaque Token Karşılaştırması

| Özellik | Access Token (JWT) | Refresh Token (Opaque) |
|---------|-------------------|------------------------|
| **Format** | eyJhbG...signature | Rastgele string |
| **Decode edilebilir?** | ✅ jwt.io'da açılır | ❌ Anlamsız karakterler |
| **İçerik** | Claims (sub, exp, aud) | Sadece ID referansı |
| **Doğrulama** | Signature ile (offline) | Sunucuya sorarak (online) |
| **Nerede saklanır?** | Kendini taşır (self-contained) | Veritabanında |
| **Boyut** | ~1000+ karakter | ~64 karakter |

## Neden Opaque?

1. **Güvenlik:** Çalınsa bile içinden bilgi çıkmaz
2. **Revoke:** Sunucudan anında iptal edilebilir
3. **Boyut:** Kısa ve hafif
4. **Kontrol:** Sunucu tam kontrol sahibi

---

## Refresh Token Nerede Tanımlandı?

**Dosya:** `IdentityServer/Udemy.IdentityServer/Config.cs`

```csharp
// User client (Giriş yapmış kullanıcılar - Basket, Order, vb.)
new Client
{
    ClientName = "Udemy User Client",
    ClientId = "udemy_user",
    AllowOfflineAccess = true,  // ⬅️ Refresh token'ı aktif eder!
    ClientSecrets = { new Secret("secret".Sha256()) },
    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

    AllowedScopes =
    {
        "basket_fullpermission",
        "order_fullpermission",
        // ... diğer scope'lar
        IdentityServerConstants.StandardScopes.OfflineAccess,  // ⬅️ Refresh için gerekli
    },

    // Token süreleri
    AccessTokenLifetime = 3600,  // 1 saat (saniye)
    
    // Refresh Token Ayarları
    RefreshTokenExpiration = TokenExpiration.Absolute,  // Sabit süre
    AbsoluteRefreshTokenLifetime = (int)(DateTime.Now.AddDays(60) - DateTime.Now).TotalSeconds,  // 60 gün
    RefreshTokenUsage = TokenUsage.ReUse  // Aynı token tekrar kullanılabilir
}
```

---

## Refresh Token Ayarları Açıklaması

### 1. `AllowOfflineAccess = true`
Refresh token almayı aktif eder. `false` ise refresh token verilmez.

### 2. `OfflineAccess` Scope
Client'ın `OfflineAccess` scope'unu istemesi gerekir.

### 3. `RefreshTokenExpiration`

| Değer | Açıklama |
|-------|----------|
| `Absolute` | Token oluşturulduğunda belirlenen süre sonunda kesin olarak biter |
| `Sliding` | Her kullanımda süre yenilenir |

### 4. `AbsoluteRefreshTokenLifetime`
Token'ın maksimum ömrü (saniye cinsinden):
```csharp
// 60 gün = 5,184,000 saniye
AbsoluteRefreshTokenLifetime = (int)(DateTime.Now.AddDays(60) - DateTime.Now).TotalSeconds
```

### 5. `RefreshTokenUsage`

| Değer | Açıklama |
|-------|----------|
| `ReUse` | Aynı refresh token birden fazla kez kullanılabilir |
| `OneTimeOnly` | Her kullanımda yeni refresh token üretilir (daha güvenli) |

---

## IdentityServer'da Refresh Token Saklanması

Refresh token'lar `PersistedGrants` tablosunda saklanır:

```sql
PersistedGrants tablosu:
┌─────────────────────────────────────┐
│ Key: "E4D2F8C1A3B5..."              │  ← Refresh token değeri
│ Type: "refresh_token"               │
│ SubjectId: "user-guid"              │  ← Hangi kullanıcı
│ ClientId: "udemy_user"              │  ← Hangi client
│ CreationTime: 2026-01-03            │
│ Expiration: 2026-03-04              │  ← 60 gün sonra
│ Data: "{...}"                       │  ← Serialized token data
└─────────────────────────────────────┘
```

## Development Ortamında Sorun

> ⚠️ **Önemli:** Development ortamında IdentityServer **In-Memory Database** kullanır.
> Proje yeniden başlatıldığında tüm refresh token'lar silinir!
> "invalid_grant" hatası alırsanız çıkış yapıp tekrar giriş yapın.

---

## Özet

| Ayar | Değer | Açıklama |
|------|-------|----------|
| Access Token Süresi | 1 saat | `AccessTokenLifetime = 3600` |
| Refresh Token Süresi | 60 gün | `AbsoluteRefreshTokenLifetime` |
| Refresh Token Türü | Opaque | JWT değil, veritabanında saklanır |
| Kullanım | ReUse | Aynı token birden fazla kullanılabilir |
| Expiration | Absolute | Sabit süre, sliding değil |
