
$baseUrl = "http://localhost"

# Map service names to ports
$services = @{
    "Auth API" = "5001"
    "Apps API" = "5002"
    "Users API" = "5003"
    "Notifications API" = "5004"
    "Media API" = "5005"
    "Chat API" = "5006"
    "Payments API" = "5007"
    "Gateway API" = "7032"
    "Audit API" = "5008"
    "Search API" = "5009"
    "Scheduler API" = "5010"
    "Geo API" = "5011"
    "Recommendation API" = "5012"
}

$frontends = @{
    "Global Admin" = "3000"
    "FitIT Admin" = "3001"
    "Wissler Admin" = "3002"
}

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url
    )
    
    Write-Host "Testing $Name... " -NoNewline
    try {
        $response = Invoke-RestMethod -Uri $Url -Method Get -ErrorAction Stop -TimeoutSec 5
        Write-Host "OK" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "FAILED" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)"
        return $false
    }
}

Write-Host "--- Verifying MS Platform Services ---"
$successCount = 0
$totalCount = 0

foreach ($serviceName in $services.Keys) {
    $port = $services[$serviceName]
    $url = "${baseUrl}:${port}/health"
    $result = Test-Endpoint -Name $serviceName -Url $url
    if ($result) { $successCount++ }
    $totalCount++
}

Write-Host "`n--- Verifying Frontends ---"
foreach ($feName in $frontends.Keys) {
    $port = $frontends[$feName]
    $url = "${baseUrl}:${port}"
    $result = Test-Endpoint -Name $feName -Url $url
    if ($result) { $successCount++ }
    $totalCount++
}

Write-Host "`n--- Verification Summary ---"
Write-Host "$successCount / $totalCount services are reachable."

if ($successCount -eq $totalCount) {
    exit 0
} else {
    exit 1
}
