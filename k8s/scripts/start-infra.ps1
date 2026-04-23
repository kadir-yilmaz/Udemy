# Udemy Microservices - Altyapı Başlatma Script'i
# Bu script veritabanları, mesaj kuyrukları ve secret'ları hazırlar.

$rootDir = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

Write-Host "`n>>> [INFRA] Altyapı Servisleri Başlatılıyor (Docker Compose)..." -ForegroundColor Cyan
docker compose -f "$rootDir/docker-compose-infra.yml" up -d

Write-Host "`n>>> [K8S] Secret ve Altyapı Manifestleri Uygulanıyor..." -ForegroundColor Cyan
kubectl apply -f "$rootDir/k8s/secrets.yaml"

if (Test-Path "$rootDir/k8s/infrastructure") {
    kubectl apply -f "$rootDir/k8s/infrastructure/"
}

Write-Host "`n>>> Altyapı Hazır! <<<`n" -ForegroundColor Green
