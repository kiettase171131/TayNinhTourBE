# PowerShell script để test Tour Booking PayOS Webhook
# Chạy script này để test webhook endpoints (đơn giản như product payment)

$baseUrl = "https://localhost:7205"
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

Write-Host "🧪 Testing Tour Booking PayOS Webhook Endpoints" -ForegroundColor Green
Write-Host "Server: $baseUrl" -ForegroundColor Yellow
Write-Host "Format: Đơn giản như product payment (không cần signature)" -ForegroundColor Cyan
Write-Host ""

# Test orderCode (giống format PayOS)
$testOrderCode = "TNDT1234567890"

# Test Success Webhook
Write-Host "1️⃣ Testing Success Webhook..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/tour-booking-payment/webhook/paid/$testOrderCode" -Method POST -Headers $headers -SkipCertificateCheck
    Write-Host "✅ Success Webhook Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 2
} catch {
    Write-Host "❌ Success Webhook Failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Yellow
    }
}

Write-Host ""

# Test Cancel Webhook
Write-Host "2️⃣ Testing Cancel Webhook..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/tour-booking-payment/webhook/cancelled/$testOrderCode" -Method POST -Headers $headers -SkipCertificateCheck
    Write-Host "✅ Cancel Webhook Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 2
} catch {
    Write-Host "❌ Cancel Webhook Failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        $responseBody = $reader.ReadToEnd()
        Write-Host "Response Body: $responseBody" -ForegroundColor Yellow
    }
}

Write-Host ""

# Test Frontend Callback (existing endpoint)
Write-Host "3️⃣ Testing Frontend Callback..." -ForegroundColor Cyan
$frontendCallbackData = @{
    orderCode = $testOrderCode
    status = "PAID"
    transactionDateTime = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/tour-booking-payment/payment-success" -Method POST -Headers $headers -Body $frontendCallbackData -SkipCertificateCheck
    Write-Host "✅ Frontend Callback Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 2
} catch {
    Write-Host "❌ Frontend Callback Failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""
Write-Host "🏁 Test completed!" -ForegroundColor Green
Write-Host ""
Write-Host "📝 Notes:" -ForegroundColor Yellow
Write-Host "- Webhook endpoints đơn giản như product payment" -ForegroundColor Yellow
Write-Host "- Không cần signature verification" -ForegroundColor Yellow
Write-Host "- PayOS sẽ gọi với orderCode trong URL path" -ForegroundColor Yellow
Write-Host "- Đảm bảo server đang chạy trên https://localhost:7205" -ForegroundColor Yellow
Write-Host "- Cần có booking với orderCode '$testOrderCode' để test thành công" -ForegroundColor Yellow
