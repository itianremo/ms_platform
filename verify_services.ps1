
# verify_services.ps1

$baseUrl = "http://localhost"
$geoPort = "5011"
$recPort = "5012"
$chatPort = "5006"
$mediaPort = "5005"
$auditPort = "5008"

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Method = "GET",
        [string]$Body = ""
    )
    
    Write-Host "Testing $Name... " -NoNewline
    try {
        if ($Method -eq "POST") {
            $response = Invoke-RestMethod -Uri $Url -Method Post -Body $Body -ContentType "application/json" -ErrorAction Stop
        }
        else {
            $response = Invoke-RestMethod -Uri $Url -Method Get -ErrorAction Stop
        }
        Write-Host "OK" -ForegroundColor Green
        return $response
    }
    catch {
        Write-Host "FAILED" -ForegroundColor Red
        Write-Host $_.Exception.Message
        return $null
    }
}

Write-Host "--- Verifying MS Platform Services ---"

# 1. Geo Service
Test-Endpoint -Name "Geo Service Health" -Url "${baseUrl}:${geoPort}/health"

# 2. Recommendation Service
Test-Endpoint -Name "Recommendation Service Health" -Url "${baseUrl}:${recPort}/health"

# 3. Chat Service
Test-Endpoint -Name "Chat Service Health" -Url "${baseUrl}:${chatPort}/health"

# 4. Media Service
Test-Endpoint -Name "Media Service Health" -Url "${baseUrl}:${mediaPort}/health"

# 5. Audit Service
Test-Endpoint -Name "Audit Service Health" -Url "${baseUrl}:${auditPort}/health"

Write-Host "`n--- Verification Complete ---"
