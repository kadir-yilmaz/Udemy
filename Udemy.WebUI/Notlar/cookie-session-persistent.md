# Authentication Cookie ve Token Notları

## 1. Cookie: Session vs Persistent

### Session Cookie (IsPersistent = false)
- Tarayıcı kapatıldığında **otomatik silinir**
- Disk'e yazılmaz, sadece **bellekte** tutulur
- `Set-Cookie` header'ında `Expires` veya `Max-Age` **yok**
- Örnek: Bankacılık siteleri

### Persistent Cookie (IsPersistent = true)
- Tarayıcı kapansa bile **kalır**
- Disk'e yazılır
- `Set-Cookie` header'ında `Expires` veya `Max-Age` **var**
- Örnek: "Beni Hatırla" seçilen siteler

---

## 2. IsPersistent Nasıl Ayarlanır?

```csharp
// AuthController - Login metodu
var authProperties = new AuthenticationProperties
{
    IsPersistent = rememberMe,  // "Beni Hatırla" checkbox
    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(60)
};

await HttpContext.SignInAsync(principal, authProperties);
```

---

## 3. Cookie İçinde Hangi Token'lar Var?

| Token | Tip | Açıklama |
|-------|-----|----------|
| `access_token` | JWT | API'lere erişim, decode edilebilir |
| `refresh_token` | Opaque | Yeni token almak için, 64 karakter ID |
| `expires_at` | Timestamp | Access token bitiş zamanı |

---

## 4. Refresh Token Neden Kısa?

Refresh token bir **Reference Token** (opaque token):
- IdentityServer veritabanında saklanan **ID/referans**
- JWT gibi decode **edilemez**
- 64 karakter = güvenli random string
- Sunucu tarafında validate edilir (DB'den bakılır)

**Neden Opaque?**
- Güvenlik: Token ele geçirilse bile içerik görülmez
- Revoke: Sunucudan anında iptal edilebilir
- Boyut: JWT'den çok daha küçük

---

## 5. expires_at Nedir?

`expires_at` gerçek bir token **değildir**. Access token'ın ne zaman expire olacağını gösteren timestamp:

```
2026-01-03T15:22:47.5447975+03:00
```

- ISO 8601 formatında
- Cookie içinde referans olarak saklanır
- `GetTokenAsync("expires_at")` ile okunur

---

## 6. Sliding Expiration

`SlidingExpiration = true` olduğunda:
- Her HTTP isteği cookie süresini **yeniler**
- Kullanıcı aktif kaldıkça oturum **uzar**
- Pasif kalırsa süre dolunca logout olur

---

## Özet Tablo

| Özellik | Session Cookie | Persistent Cookie |
|---------|----------------|-------------------|
| IsPersistent | false | true |
| Tarayıcı kapanınca | Silinir | Kalır |
| Disk'e yazılır | ❌ | ✅ |
| Expires header | Yok | Var |
| Güvenlik seviyesi | Yüksek | Orta |

---

## Kod Referansları

- **Cookie Config:** `Program.cs` → `AddCookie()`
- **Token Süreleri:** `IdentityServer/Config.cs` → `Clients`
- **Login:** `AuthController.cs` → `SignIn()`
