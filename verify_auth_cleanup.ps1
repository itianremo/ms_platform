
param(
    [string]$Email = "admin@example.com",
    [string]$Password = "Password123!",
    [string]$BaseUrl = "http://localhost:5001"
)

$loginUrl = "$BaseUrl/api/auth/login"
$logoutUrl = "$BaseUrl/api/auth/logout"

Write-Host "1. Logging in as $Email..."
$body = @{
    email    = $Email
    password = $Password
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $body -ContentType "application/json" -SessionVariable "Session"
    Write-Host "Login Successful." -ForegroundColor Green
    
    $token = $loginResponse.accessToken
    Write-Host "Access Token: $token"
}
catch {
    Write-Host "Login Failed: $($_.Exception.Message)" -ForegroundColor Red
    exit
}

Write-Host "`n2. Logging out..."
try {
    # Include the cookie in the session
    Invoke-RestMethod -Uri $logoutUrl -Method Post -WebSession $Session -Headers @{ "Authorization" = "Bearer $token" }
    Write-Host "Logout Successful." -ForegroundColor Green
}
catch {
    Write-Host "Logout Failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n3. Verifying Session Cleanup (Manual Step)..."
Write-Host "Please check the database (UserSessions table) to confirm the session has been removed."
