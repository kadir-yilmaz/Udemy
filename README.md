# Udemy Microservices - E-Commerce Platform

Udemy Microservices, .NET 8 mimarisi üzerine inşa edilmiş, modern mikroservis yaklaşımlarını ve konteyner teknolojilerini kullanan kapsamlı bir e-ticaret platformudur.

---

## Proje Özeti ve Öne Çıkan Mimari Özellikler

Bu proje, mikroservis mimarisinin temel prensiplerini ve dağıtık sistemlerin yönetimini (service discovery, gateway, security, event-driven communication) uygulamalı olarak ele alan bir projedir.

### Event-Driven Architecture & SAGA Pattern
Mikroservisler arası iletişim, **MassTransit** ve **RabbitMQ** üzerinden asenkron olarak yönetilir. Sipariş süreçleri (Ordering), ödeme (Payment) ve fatura (Invoice) işlemleri arasındaki veri tutarlılığı, dağıtık sistemlerdeki en kritik konulardan biri olan **Event-Driven** yaklaşımları ile koordine edilir.

-

## Client

### WebUI
- **Port:** `5075`
- **Teknoloji:** ASP.NET Core MVC
- **Önemli Bilgiler:** Kullanıcı dostu arayüzü ile sepet yönetimi, ürün katalog takibi ve sipariş süreçlerini uçtan uca sunar.

---

## Gateway & Identity Management

### Ocelot Gateway
- **Port:** `5000`
- **Teknoloji:** Ocelot API Gateway
- **Önemli Bilgiler:** Merkezi giriş noktasıdır. Downstream ve Upstream servis yönlendirmelerini yönetir.

### IdentityServer (Identity Management)
- **Port:** `5001`
- **DB:** SQL Server (Port: `1445`)
- **Teknoloji:** IdentityServer4, ASP.NET Core Identity
- **Önemli Bilgiler:** OAuth2 ve OpenID Connect standartlarında kimlik doğrulama sağlar.

---

## Microservices (Port Sırasına Göre)

### Catalog API
- **Port:** `5011`
- **DB:** MongoDB (Port: `27017`)
- **Teknoloji:** MongoDB .NET Driver
- **Önemli Bilgiler:** Ürün ve kategori yönetimini sağlar. NoSQL yapısı ile esnek veri modeli sunar.

### PhotoStock API
- **Port:** `5012`
- **DB:** Local Storage / Docker Volumes
- **Teknoloji:** ASP.NET Core API
- **Önemli Bilgiler:** Ürün görsellerinin yüklenmesi ve sunulmasından sorumludur.

### Basket API
- **Port:** `5013`
- **DB:** Redis (Port: `6379`)
- **Teknoloji:** StackExchange.Redis
- **Önemli Bilgiler:** Kullanıcı sepetlerini Redis üzerinde yüksek performansla yönetir.

### Discount API
- **Port:** `5014`
- **DB:** PostgreSQL (Port: `5432`)
- **Teknoloji:** Dapper (Micro ORM)
- **Önemli Bilgiler:** İndirim kuponlarını yönetir. Hafif ve hızlı olması için Dapper tercih edilmiştir.

### Order API
- **Port:** `5015`
- **DB:** SQL Server (Port: `1444`)
- **Teknoloji:** EF Core, MassTransit, RabbitMQ
- **Önemli Bilgiler:** Onion Architecture prensiplerine göre geliştirilmiştir. Sipariş oluşturma ve geçmiş sipariş yönetimini yapar.

### FakePayment API
- **Port:** `5016`
- **DB:** Yok
- **Teknoloji:** MassTransit, RabbitMQ
- **Önemli Bilgiler:** Ödeme işlemlerini simüle eder ve asenkron olarak sonucu diğer servislere bildirir.

### Invoice API
- **Port:** `5017`
- **DB:** Yok
- **Teknoloji:** MassTransit, RabbitMQ
- **Önemli Bilgiler:** Sipariş tamamlandığında fatura bilgilerini yakalar ve süreci yönetir.

---

## Yardımcı Servisler ve Dağıtım

| Servis | Port | Görevi |
| :--- | :--- | :--- |
| **RabbitMQ** | `5673`, `15674` | Servisler arası asenkron iletişim (Message Broker). |
| **Redis** | `6379` | Dağıtık önbellekleme ve sepet yönetimi. |

### Dağıtım Yapısı
- **Docker Compose:** Geliştirme ortamını tek komutla ayağa kaldırmak için `docker-compose.yml` dosyası kullanılabilir.
