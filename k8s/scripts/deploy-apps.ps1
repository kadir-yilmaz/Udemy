# Udemy Microservices - Uygulama Deployment Script'i
# Bu script mikroservisleri build eder ve K8s üzerine deploy eder.

$rootDir = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

Write-Host "`n>>> [BUILD] Docker İmajları Build Ediliyor..." -ForegroundColor Cyan
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
    docker build -t "$($image.Name):latest" -f "$rootDir/$($image.Dockerfile)" "$rootDir"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "HATA: $($image.Name) build edilemedi!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

Write-Host "`n>>> [K8S] Uygulama Manifestleri Uygulanıyor..." -ForegroundColor Cyan
# Sadece ana dizindeki servisleri apply et (infrastructure'ı start-infra scripti hallediyor)
Get-ChildItem -Path "$rootDir/k8s/*.yaml" | ForEach-Object {
    kubectl apply -f $_.FullName
}

Write-Host "`n>>> [K8S] Servisler Yeniden Başlatılıyor..." -ForegroundColor Cyan
$deployments = @("identityserver", "gateway", "catalog-api", "photostock-api", "basket-api", "discount-api", "order-api", "fakepayment-api", "invoice-api", "webui")
foreach ($deploy in $deployments) {
    kubectl rollout restart "deployment/$deploy"
}

Write-Host "`n>>> Uygulamalar Yayına Alındı! <<<" -ForegroundColor Green
Write-Host "WebUI: http://localhost:30075`n" -ForegroundColor White
