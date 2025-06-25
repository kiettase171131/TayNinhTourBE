# Test script for TayNinhTour APIs
# Test Tour Guide Registration and Specialty Shop Registration APIs

$baseUrl = "http://localhost:5267"
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

Write-Host "=== TayNinhTour API Testing Script ===" -ForegroundColor Green
Write-Host "Base URL: $baseUrl" -ForegroundColor Yellow

# Function to make HTTP requests with error handling
function Invoke-ApiRequest {
    param(
        [string]$Method,
        [string]$Uri,
        [hashtable]$Headers,
        [object]$Body = $null,
        [string]$ContentType = "application/json"
    )
    
    try {
        $params = @{
            Method = $Method
            Uri = $Uri
            Headers = $Headers
        }
        
        if ($Body) {
            if ($ContentType -eq "application/json") {
                $params.Body = $Body | ConvertTo-Json -Depth 10
            } else {
                $params.Body = $Body
                $params.ContentType = $ContentType
            }
        }
        
        $response = Invoke-RestMethod @params
        return @{
            Success = $true
            Data = $response
            StatusCode = 200
        }
    }
    catch {
        $errorDetails = $_.Exception.Response
        $statusCode = if ($errorDetails) { $errorDetails.StatusCode.value__ } else { 500 }
        
        Write-Host "API Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()
            Write-Host "Response Body: $responseBody" -ForegroundColor Red
        }
        
        return @{
            Success = $false
            Error = $_.Exception.Message
            StatusCode = $statusCode
            ResponseBody = $responseBody
        }
    }
}

# Step 1: Register a test user
Write-Host "`n1. Registering test user..." -ForegroundColor Cyan

$registerData = @{
    name = "Test User for API Testing"
    email = "testuser.api@example.com"
    password = "TestPassword123!"
    phoneNumber = "0123456789"
    avatar = "https://example.com/default-avatar.jpg"
}

$registerResult = Invoke-ApiRequest -Method "POST" -Uri "$baseUrl/api/Authentication/register" -Headers $headers -Body $registerData

if ($registerResult.Success) {
    Write-Host "✓ User registration successful" -ForegroundColor Green
    Write-Host "Response: $($registerResult.Data | ConvertTo-Json -Depth 3)" -ForegroundColor Gray
} else {
    Write-Host "✗ User registration failed" -ForegroundColor Red
    Write-Host "Error: $($registerResult.Error)" -ForegroundColor Red
}

# Step 2: Login to get JWT token
Write-Host "`n2. Logging in to get JWT token..." -ForegroundColor Cyan

$loginData = @{
    email = "testuser.api@example.com"
    password = "TestPassword123!"
}

$loginResult = Invoke-ApiRequest -Method "POST" -Uri "$baseUrl/api/Authentication/login" -Headers $headers -Body $loginData

if ($loginResult.Success -and $loginResult.Data.token) {
    $jwtToken = $loginResult.Data.token
    Write-Host "✓ Login successful" -ForegroundColor Green
    Write-Host "JWT Token: $($jwtToken.Substring(0, 50))..." -ForegroundColor Gray
    
    # Update headers with JWT token
    $authHeaders = @{
        "Content-Type" = "application/json"
        "Accept" = "application/json"
        "Authorization" = "Bearer $jwtToken"
    }
} else {
    Write-Host "✗ Login failed" -ForegroundColor Red
    Write-Host "Error: $($loginResult.Error)" -ForegroundColor Red
    exit 1
}

# Step 3: Test Tour Guide Registration API (JSON version first)
Write-Host "`n3. Testing Tour Guide Registration API (JSON)..." -ForegroundColor Cyan

$tourGuideData = @{
    fullName = "Nguyen Van Test Guide"
    phoneNumber = "0987654321"
    email = "testguide@example.com"
    experience = 3
    languages = "Vietnamese, English, Chinese"
}

$tourGuideResult = Invoke-ApiRequest -Method "POST" -Uri "$baseUrl/api/Account/tourguide-application" -Headers $authHeaders -Body $tourGuideData

if ($tourGuideResult.Success) {
    Write-Host "✓ Tour Guide JSON registration successful" -ForegroundColor Green
    Write-Host "Response: $($tourGuideResult.Data | ConvertTo-Json -Depth 3)" -ForegroundColor Gray
} else {
    Write-Host "✗ Tour Guide JSON registration failed" -ForegroundColor Red
    Write-Host "Error: $($tourGuideResult.Error)" -ForegroundColor Red
}

# Step 4: Test Specialty Shop Registration API
Write-Host "`n4. Testing Specialty Shop Registration API..." -ForegroundColor Cyan

$shopData = @{
    shopName = "Test Specialty Shop"
    shopDescription = "A test specialty shop for API testing"
    businessLicense = "BL123456789"
    location = "123 Shop Street, Tay Ninh"
    phoneNumber = "0123456789"
    email = "testshop@example.com"
    website = "https://testshop.example.com"
    shopType = "Handicrafts"
    openingHours = "8:00 AM - 6:00 PM"
    representativeName = "Nguyen Van Shop Owner"
}

$shopResult = Invoke-ApiRequest -Method "POST" -Uri "$baseUrl/api/Account/specialty-shop-application" -Headers $authHeaders -Body $shopData

if ($shopResult.Success) {
    Write-Host "✓ Specialty Shop registration successful" -ForegroundColor Green
    Write-Host "Response: $($shopResult.Data | ConvertTo-Json -Depth 3)" -ForegroundColor Gray
    $shopApplicationId = $shopResult.Data.applicationId
} else {
    Write-Host "✗ Specialty Shop registration failed" -ForegroundColor Red
    Write-Host "Error: $($shopResult.Error)" -ForegroundColor Red
}

Write-Host "`n=== API Testing Completed ===" -ForegroundColor Green
Write-Host "Check the results above for any issues." -ForegroundColor Yellow
