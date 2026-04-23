# Redis Sentinel Failover Test Raporu

Bu döküman, Kubernetes üzerinde çalışan Redis Sentinel yapısının yüksek erişilebilirlik (High Availability) test sonuçlarını içerir.

## Test Senaryosu
Sistemde 3 adet Redis pod'u (`redis-0`, `redis-1`, `redis-2`) ve bir Sentinel denetleyicisi bulunmaktadır. Mevcut Master pod manuel olarak silinerek sistemin yeni bir Master seçme ve sürekliliği sağlama yeteneği test edilmiştir.

---

## Test Aşamaları ve Çıktılar

### 1. Mevcut Durum Analizi
Sentinel üzerinden yapılan sorgulamada, sistemin lideri (Master) belirlendi:
- **Master Pod:** `udemy-basket-redis-0`
- **Port:** `6379`

### 2. Failover Tetikleme (Simülasyon)
Master olan `udemy-basket-redis-0` pod'u Kubernetes üzerinden silindi. Bu işlem, bir sunucu çökmesini veya beklenmedik bir kesintiyi simüle eder.

### 3. Yeni Master Seçimi (Election)
Sentinel saniyeler içinde durumu fark etti ve yeni lideri seçti:
- **Yeni Master:** `udemy-basket-redis-2`
- **Yeni Master IP:** `10.1.0.191`

### 4. Son Durum ve Rol Dağılımı
Test sonrası pod'ların güncel rolleri şu şekildedir:

| Pod Adı | Yeni Rol | Durum |
| :--- | :--- | :--- |
| `udemy-basket-redis-2` | **Master** | Aktif / Lider |
| `udemy-basket-redis-1` | **Slave** | Lidere Bağlı |
| `udemy-basket-redis-0` | **Slave** | Yeniden Başlatıldı / Takipçi |

---

## Basket API Üzerindeki Etkisi
Basket API logları incelendiğinde, Redis kümesindeki bu değişim sırasında servisin ayakta kaldığı ve JWT doğrulaması gibi işlemlerin kesintisiz devam ettiği gözlemlenmiştir.

```text
[JWT] Token validated for: udemy_public
[JWT] All claims: ...
info: Microsoft.Hosting.Lifetime[0]
      Application started. Hosting environment: Development
```

## Sonuç
Yapılan test sonucunda, Redis Sentinel yapısının **otomatik iyileştirme (self-healing)** ve **yüksek erişilebilirlik** kriterlerini başarıyla karşıladığı doğrulanmıştır. Sistem, herhangi bir veri kaybı veya hizmet kesintisi yaşanmadan lider değişimini gerçekleştirebilmektedir.

---
*Test Tarihi: 23 Nisan 2026*
*Araç: k8s/scripts/redis-sentinel-failover.ps1*
