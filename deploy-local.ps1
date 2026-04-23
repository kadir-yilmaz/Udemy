# Udemy Microservices - Tek Tıkla Yerel Deploy Script'i

Write-Host "`n>>> 1. Altyapı Servisleri Başlatılıyor (Docker Compose)..." -ForegroundColor Cyan
docker compose -f docker-compose-infra.yml up -d

Write-Host "`n>>> 2. Docker İmajları Build Ediliyor (Bu biraz vakit alabilir)..." -ForegroundColor Cyan
$images = @(
    @{ Name = "identityserver"; Dockerfile = "Udemy.NewIdentityServer/Dockerfile" },
    @{ Name = "gateway"; Dockerfile = "Udemy.Gateway/Dockerfile" },
    @{ Name = "catalog-api"; Dockerfile = "Catalog/Udemy.Catalog.API/Dockerfile" },
    @{ Name = "photostock-api"; Dockerfile = "PhotoStock/Udemy.PhotoStock.API/Dockerfile" },
    @{ Name = "basket-api"; Dockerfile = "Basket/Udemy.Basket.API/Dockerfile" },
    @{ Name = "discount-api"; Dockerfile = "Discount/Udemy.Discount.API/Dockerfile" },
    @{ Name = "order-api"; Dockerfile = "Order/Udemy.Order.API/Dockerfile" },
    @{ Name = "fakepayment-api"; Dockerfile = "FakePayment/Udemy.FakePayment.API/Dockerfile" },
    @{ Name = "invoice-api"; Dockerfile = "Invoice/Udemy.Invoice.API/Dockerfile" },
    @{ Name = "webui"; Dockerfile = "Udemy.WebUI/Dockerfile" }
)

foreach ($image in $images) {
    Write-Host "Building $($image.Name)..." -ForegroundColor Yellow
    docker build -t "$($image.Name):latest" -f $image.Dockerfile .
    if ($LASTEXITCODE -ne 0) {
        Write-Host "HATA: $($image.Name) build edilemedi!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

Write-Host "`n>>> 3. Kubernetes Manifestleri Uygulanıyor..." -ForegroundColor Cyan
# Önce secret ve infrastructure (bazı dosyalar dizin içinde olabilir, hata almamak için tek tek veya dizin olarak apply ediyoruz)
kubectl apply -f k8s/secrets.yaml
if (Test-Path "k8s/infrastructure") {
    kubectl apply -f k8s/infrastructure/
}

# Tüm ana manifestler
kubectl apply -f k8s/

Write-Host "`n>>> 4. Servisler Yeniden Başlatılıyor (Rollout Restart)..." -ForegroundColor Cyan
$deployments = @("identityserver", "gateway", "catalog-api", "photostock-api", "basket-api", "discount-api", "order-api", "fakepayment-api", "invoice-api", "webui")
foreach ($deploy in $deployments) {
    Write-Host "Restarting $deploy..." -ForegroundColor Gray
    kubectl rollout restart "deployment/$deploy"
}

Write-Host "`n>>> İŞLEM BAŞARIYLA TAMAMLANDI! <<<" -ForegroundColor Green
Write-Host "WebUI: http://localhost:30075" -ForegroundColor White
Write-Host "Gateway: http://localhost:30000" -ForegroundColor White
Write-Host "IdentityServer: http://localhost:30001`n" -ForegroundColor White
