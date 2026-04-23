# Local K8s Deploy - Kurulum ve Çalıştırma Rehberi

Bu döküman, projenin yerel makinede GitHub Actions ve Kubernetes (K8s) kullanılarak nasıl otomatik olarak ayağa kaldırılacağını adım adım açıklar.

---

## 1. GitHub Actions Workflow Oluşturma
Sürecin merkezi, `.github/workflows/deploy-local.yml` dosyasıdır. Bu dosya, koda her push yapıldığında tetiklenen otomasyon adımlarını içerir.

- **Dosya Yolu:** `.github/workflows/deploy-local.yml`
- **Tetikleyici:** `main` veya `master` branch'ine yapılan push işlemleri.
- **Hedef:** `runs-on: self-hosted` (Yerel makinedeki runner).

---

## 2. Self-Hosted Runner Kurulumu (Yerel Makine Tanımlama)
GitHub'ın yerel makinenize erişebilmesi için bir "Runner" kurmanız gerekir.

### Kurulum Adımları:
1. GitHub deponuzda **Settings > Actions > Runners** yolunu izleyin.
2. **New self-hosted runner** butonuna tıklayın ve "Windows" seçeneğini seçin.
3. Proje dizininizde `actions-runner-k8s` adında bir klasör oluşturun.
4. GitHub'ın verdiği indirme ve konfigürasyon komutlarını bu klasör içinde çalıştırın.
   - Örn: `config.cmd --url https://github.com/kadir-yilmaz/Udemy --token [TOKEN]`
5. Kurulum bittiğinde klasör yapınız şu şekilde olacaktır: `d:\Kadir\Projeler\Udemy\actions-runner-k8s`.

---

## 3. Altyapı ve Hazırlık (Infrastructure)
Uygulama ayağa kalkmadan önce veritabanı ve mesaj kuyruğu gibi altyapı servislerinin hazır olması gerekir.

- **Komut:** `docker compose -f docker-compose-infra.yml up -d`
- **İçerik:** MongoDB, SQL Server, Redis, RabbitMQ ve PostgreSQL.

---

## 4. Uygulamayı Yayına Alma (Deployment)
Runner aktif olduğunda, workflow şu adımları otomatik olarak gerçekleştirir:

1. **Build:** Tüm mikroservisler (`Catalog`, `Basket`, `Identity` vb.) için Docker imajları oluşturulur.
2. **Secret Injection:** Kubernetes tarafındaki hassas bilgiler (şifreler, bağlantı metinleri) `kubectl` aracılığıyla sisteme dahil edilir.
3. **K8s Apply:** `k8s/` klasöründeki manifest dosyaları (`deployment.yaml`, `service.yaml`) Kubernetes kümesine uygulanır.
4. **Rollout Restart:** Değişikliklerin anında yansıması için deployment'lar yeniden başlatılır.

---

## 5. Sistemi Çalıştırma (Kritik Komutlar)

Projenin yerelinizde GitHub Actions ile senkronize çalışması için runner'ın açık olması gerekir.

### Runner'ı Başlatma:
Terminali açın ve şu komutları çalıştırın:
```powershell
cd d:\Kadir\Projeler\Udemy\actions-runner-k8s
./run.cmd
```
*Bu komut çalıştıktan sonra GitHub'daki workflow "Waiting for a runner..." durumundan "Running" durumuna geçecektir.*

### Kubernetes Durumunu Kontrol Etme:
Her şeyin yolunda olduğunu doğrulamak için:
```powershell
kubectl get pods
kubectl get services
```

### Tarayıcı Erişimi (K8s):
- **WebUI:** `http://localhost:30075`
- **Gateway:** `http://localhost:30000`
- **IdentityServer:** `http://localhost:30001`

---

## 6. Önemli Notlar
- **Docker Desktop:** Kubernetes özelliğinin aktif olduğundan emin olun.
- **K8s Secrets:** `invoice-api-secrets` gibi gerekli secret'ların kümede tanımlı olması gerekir.
- **Persistence:** Kubernetes ayarlarını veritabanında tutar; cluster'ı kapatsanız bile tekrar açtığınızda uygulamalar otomatik olarak eski halleriyle ayağa kalkar.
