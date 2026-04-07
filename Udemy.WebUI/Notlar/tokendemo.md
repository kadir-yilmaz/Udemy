# Token Demo - OAuth 2.0 & JWT Eğitim Rehberi

## JWT Yapısı (3 Parça)

```
eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.     (Header - kırmızı)
eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6...   (Payload - sarı)
SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQ... (Signature - mavi)
```

| Parça | İçerik | Açıklama |
|-------|--------|----------|
| **Header** | alg, typ | Algoritma bilgisi (RS256, HS256) |
| **Payload** | Claims | Kullanıcı bilgileri (sub, email, role) |
| **Signature** | İmza | Değiştirilmediğini doğrular |

---

## Sık Kullanılan JWT Claims

**Claim** = JWT içindeki bilgi alanlarına verilen isim (iddia).

| Claim | Açıklama | Örnek |
|-------|----------|-------|
| `sub` | Subject (Kullanıcı ID) | a1b2c3d4-... |
| `iss` | Issuer (Token veren) | https://localhost:5001 |
| `aud` | Audience (Hedef API) | resource_catalog |
| `exp` | Expiration | 1704243600 |
| `iat` | Issued At | 1704240000 |
| `scope` | İzinler | catalog_fullpermission |
| `name` | Kullanıcı adı | Kadir Yılmaz |
| `role` | Kullanıcı rolü | admin |

### Claim Türleri

| Tür | Açıklama |
|-----|----------|
| **Registered Claims** | Standart: `sub`, `iss`, `aud`, `exp`, `iat` |
| **Public Claims** | Yaygın: `email`, `name`, `role` |
| **Private Claims** | Uygulamaya özel: `city`, `permission` |

---

## JWT vs Opaque Token

| Özellik | JWT (Access Token) | Opaque (Refresh Token) |
|---------|-------------------|------------------------|
| **Format** | Base64 JSON | Rastgele string |
| **İçerik** | Header.Payload.Signature | Anlamsız karakterler |
| **Okunabilir?** | ✅ Evet (jwt.io) | ❌ Hayır |
| **Validate** | Signature ile (offline) | Sunucuya sorarak (online) |
| **Bilgi içerir?** | ✅ Claims | ❌ Sadece referans |
| **Süre** | Kısa (1 saat) | Uzun (60 gün) |

> **Neden farklı?** JWT çalınırsa içindeki bilgiler açığa çıkar. Opaque token çalınsa bile anlamsızdır, sunucu tarafında revoke edilebilir.

---

## OAuth 2.0 Grant Türleri

### 1. Resource Owner Password (ROPC)
- Kullanıcı email + şifre ile giriş yapar
- Token kullanıcıya özeldir
- `POST /connect/token → grant_type=password`

### 2. Client Credentials
- Uygulama kendi adına API çağırır
- Kullanıcı yok, makine-makine
- `POST /connect/token → grant_type=client_credentials`
- **Refresh token YOK** (secret her zaman elimizde)

### 3. Authorization Code
- Tarayıcı redirect ile login
- En güvenli yöntem (SPA, Web App)
- `GET /authorize → code → POST /token`

### 4. Refresh Token
- Mevcut refresh token ile yeni access token alır
- `POST /connect/token → grant_type=refresh_token`

---

## Token Güvenlik Kuralları

### ❌ YAPMA
- Token'ı localStorage'da tutma
- URL query string'de gönderme
- Console.log ile yazdırma (prod)
- Frontend'de decode edip güvenme

### ✅ YAP
- HttpOnly cookie kullan
- HTTPS zorunlu yap
- Access token süresini kısa tut
- Refresh token'ı sunucuda validate et

### ℹ️ BİLGİ
- JWT imzalıdır, şifreli değil
- Base64 decode ile içerik okunabilir
- Signature sayesinde değiştirilemez
- Revoke için short-lived + blacklist

---

## Cookie: Session vs Persistent

| Özellik | Session Cookie | Persistent Cookie |
|---------|----------------|-------------------|
| IsPersistent | false | true |
| Tarayıcı kapanınca | Silinir | Kalır |
| Disk'e yazılır | ❌ | ✅ |
| Expires header | Yok | Var |

```csharp
// "Beni Hatırla" seçilince IsPersistent = true olur
var authProperties = new AuthenticationProperties
{
    IsPersistent = rememberMe,
    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(60)
};
await HttpContext.SignInAsync(principal, authProperties);
```

---

## Projede Kullanılan Token'lar

| Token | Grant | Nerede? | Süre |
|-------|-------|---------|------|
| User Access Token | ROPC | Cookie içinde | 1 saat |
| Refresh Token | ROPC | Cookie içinde | 60 gün |
| Client Credential | Client Cred | Memory cache | 1 saat |

---

## Geliştirici Notu

> ⚠️ **Token Yenileme Sorunu (Development)**
> 
> IdentityServer **In-Memory Database (RAM)** kullandığı için, proje yeniden başlatıldığında tüm Refresh Token'lar silinir. 
> "invalid_grant" hatası alırsanız çıkış yapıp tekrar giriş yapın.
> Production'da gerçek veritabanı kullanılacağı için bu sorun yaşanmaz.
