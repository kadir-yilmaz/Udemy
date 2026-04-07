# OAuth 2.0 Grant Türleri

## 1. Client Credentials Grant
**Kullanım:** Machine-to-machine, kullanıcı yok
**Refresh Token:** ❌ Yok (secret her zaman elimizde)

```
App ──► IS4: client_id + client_secret
App ◄── IS4: access_token
```

## 2. Resource Owner Password Grant
**Kullanım:** Kullanıcı login, güvenilir uygulama
**Refresh Token:** ✅ Var

```
App ──► IS4: client_id + client_secret + username + password
App ◄── IS4: access_token + refresh_token
```

## 3. Authorization Code Grant (+ PKCE)
**Kullanım:** Tarayıcı tabanlı, en güvenli
**Refresh Token:** ✅ Var

```
Browser ──► IS4: Authorization sayfası
User    ──► IS4: Login
Browser ◄── IS4: authorization_code
App     ──► IS4: code + code_verifier
App     ◄── IS4: access_token + refresh_token
```

## Karşılaştırma

| Grant | Kullanıcı | Refresh | Ne Zaman? |
|-------|-----------|---------|-----------|
| Client Credentials | ❌ | ❌ | API-to-API |
| Resource Owner Password | ✅ | ✅ | Güvenilir mobil/SPA |
| Authorization Code | ✅ | ✅ | Web uygulamaları |

## Projede Kullanılan

- **User Token:** Resource Owner Password Grant
- **Client Credential:** Client Credentials Grant

---

## JWT Claim Nedir?

**Claim** = JWT içindeki bilgi alanlarına verilen isim.

| Terim | Açıklama |
|-------|----------|
| **Claim** | JWT payload içindeki key-value çifti |
| **Registered Claims** | Standart claim'ler: `sub`, `iss`, `aud`, `exp`, `iat` |
| **Public Claims** | Herkesin kullanabileceği claim'ler: `email`, `name`, `role` |
| **Private Claims** | Uygulamaya özel claim'ler: `city`, `permission` |

**Örnek JWT Payload:**
```json
{
  "sub": "user123",        // Subject - Kullanıcı ID
  "iss": "identityserver", // Issuer - Token veren
  "aud": "api1",           // Audience - Hedef API
  "exp": 1704243600,       // Expiration - Bitiş zamanı
  "name": "Kadir Yılmaz",  // Public claim
  "role": "admin",         // Public claim
  "city": "Istanbul"       // Private claim
}
```

**Neden "Claim" Deniyor?**
Token, kullanıcı hakkında "iddia" (claim) yapar:
- "Bu kullanıcı admin'dir" (role claim)
- "Kullanıcının ID'si user123'tür" (sub claim)
- "Bu token X API'si içindir" (aud claim)
