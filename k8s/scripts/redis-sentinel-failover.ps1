$ErrorActionPreference = "Stop"

$serviceName = "basket-redis-set"

function Get-MasterAddress {
    $result = kubectl exec deploy/udemy-basket-redis-sentinel -- redis-cli -p 26379 SENTINEL get-master-addr-by-name $serviceName
    return $result | Where-Object { $_ -and $_.Trim() -ne "" }
}

function Resolve-RedisPodName([string]$masterAddress) {
    $pods = kubectl get pods -l app=udemy-basket-redis -o json | ConvertFrom-Json

    foreach ($pod in $pods.items) {
        $name = $pod.metadata.name
        $ip = $pod.status.podIP
        $fqdn = "$name.udemy-basket-redis-headless"

        if ($masterAddress -eq $ip -or $masterAddress -eq $name -or $masterAddress -eq $fqdn) {
            return $name
        }
    }

    throw "Could not map Sentinel master address '$masterAddress' to a Redis pod."
}

function Show-RedisRoles {
    $pods = kubectl get pods -l app=udemy-basket-redis -o json | ConvertFrom-Json

    foreach ($pod in $pods.items) {
        $name = $pod.metadata.name
        Write-Host "`n=== $name ===" -ForegroundColor Cyan
        kubectl exec $name -- redis-cli ROLE
    }
}

Write-Host "Current Redis master according to Sentinel:" -ForegroundColor Cyan
$before = Get-MasterAddress
$before | ForEach-Object { Write-Host $_ }

$currentMaster = $before[0]
$primaryPod = Resolve-RedisPodName $currentMaster

Write-Host "Deleting current master pod $primaryPod to trigger failover..." -ForegroundColor Yellow
kubectl delete pod $primaryPod

Write-Host "Waiting for Sentinel to elect a new master..." -ForegroundColor Yellow
$deadline = (Get-Date).AddMinutes(2)
do {
    Start-Sleep -Seconds 3
    $after = Get-MasterAddress
    $newMaster = $after[0]
} while ($newMaster -eq $currentMaster -and (Get-Date) -lt $deadline)

Write-Host "New Redis master according to Sentinel:" -ForegroundColor Green
$after | ForEach-Object { Write-Host $_ }

if ($newMaster -eq $currentMaster) {
    throw "Sentinel did not switch master within the timeout."
}

Write-Host "`nRedis roles after failover:" -ForegroundColor Cyan
Show-RedisRoles

Write-Host "`nBasket API logs:" -ForegroundColor Cyan
kubectl logs deploy/basket-api --tail=40
