# PowerShell script để kiểm tra webhook endpoints hiện tại
# Kiểm tra xem product payment và tour booking webhooks có hoạt động không

$baseUrl = "http://localhost:5267"
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

Write-Host "🔍 KIỂM TRA WEBHOOK ENDPOINTS HIỆN TẠI" -ForegroundColor Green
Write-Host "Server: $baseUrl" -ForegroundColor Yellow
Write-Host ""

# Test orderCode
$testOrderCode = "TNDT1234567890"

Write-Host "📋 Danh sách endpoints sẽ test:" -ForegroundColor Cyan
Write-Host "1. Product Payment - PAID: /api/payment-callback/paid/$testOrderCode" -ForegroundColor White
Write-Host "2. Product Payment - CANCELLED: /api/payment-callback/cancelled/$testOrderCode" -ForegroundColor White
Write-Host "3. Tour Booking - PAID: /api/tour-booking-payment/webhook/paid/$testOrderCode" -ForegroundColor White
Write-Host "4. Tour Booking - CANCELLED: /api/tour-booking-payment/webhook/cancelled/$testOrderCode" -ForegroundColor White
Write-Host "5. Tour Booking - Frontend Success: /api/tour-booking-payment/payment-success" -ForegroundColor White
Write-Host ""

# Test 1: Product Payment PAID
Write-Host "1️⃣ Testing Product Payment PAID Webhook..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/payment-callback/paid/$testOrderCode" -Method POST -Headers $headers -SkipCertificateCheck
    Write-Host "✅ Product Payment PAID Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 2
} catch {
    Write-Host "❌ Product Payment PAID Failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""

# Test 2: Product Payment CANCELLED
Write-Host "2️⃣ Testing Product Payment CANCELLED Webhook..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/payment-callback/cancelled/$testOrderCode" -Method POST -Headers $headers -SkipCertificateCheck
    Write-Host "✅ Product Payment CANCELLED Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 2
} catch {
    Write-Host "❌ Product Payment CANCELLED Failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""

# Test 3: Tour Booking PAID
Write-Host "3️⃣ Testing Tour Booking PAID Webhook..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/tour-booking-payment/webhook/paid/$testOrderCode" -Method POST -Headers $headers -SkipCertificateCheck
    Write-Host "✅ Tour Booking PAID Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 2
} catch {
    Write-Host "❌ Tour Booking PAID Failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""

# Test 4: Tour Booking CANCELLED
Write-Host "4️⃣ Testing Tour Booking CANCELLED Webhook..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/tour-booking-payment/webhook/cancelled/$testOrderCode" -Method POST -Headers $headers -SkipCertificateCheck
    Write-Host "✅ Tour Booking CANCELLED Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 2
} catch {
    Write-Host "❌ Tour Booking CANCELLED Failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""

# Test 5: Tour Booking Frontend Success
Write-Host "5️⃣ Testing Tour Booking Frontend Success..." -ForegroundColor Cyan
$frontendCallbackData = @{
    orderCode = $testOrderCode
    status = "PAID"
    transactionDateTime = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/api/tour-booking-payment/payment-success" -Method POST -Headers $headers -Body $frontendCallbackData -SkipCertificateCheck
    Write-Host "✅ Tour Booking Frontend Success Response:" -ForegroundColor Green
    $response | ConvertTo-Json -Depth 2
} catch {
    Write-Host "❌ Tour Booking Frontend Success Failed:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
}

Write-Host ""
Write-Host "🏁 Test completed!" -ForegroundColor Green
Write-Host ""

# Summary
Write-Host "📊 SUMMARY & NEXT STEPS:" -ForegroundColor Yellow
Write-Host ""
Write-Host "✅ Nếu Product Payment webhooks hoạt động:" -ForegroundColor Green
Write-Host "   → PayOS đã được cấu hình đúng" -ForegroundColor White
Write-Host "   → Server đã accessible từ PayOS" -ForegroundColor White
Write-Host "   → Chỉ cần thêm tour booking URLs vào PayOS Dashboard" -ForegroundColor White
Write-Host ""
Write-Host "❌ Nếu Product Payment webhooks không hoạt động:" -ForegroundColor Red
Write-Host "   → Cần kiểm tra PayOS Dashboard configuration" -ForegroundColor White
Write-Host "   → Có thể cần setup ngrok hoặc public domain" -ForegroundColor White
Write-Host "   → Cần cấu hình webhook URLs từ đầu" -ForegroundColor White
Write-Host ""
Write-Host "🔄 Nếu Tour Booking webhooks không hoạt động:" -ForegroundColor Yellow
Write-Host "   → Endpoints đã được tạo nhưng chưa được cấu hình trong PayOS" -ForegroundColor White
Write-Host "   → Cần thêm URLs vào PayOS Dashboard" -ForegroundColor White
Write-Host ""
Write-Host "📝 Cần kiểm tra:" -ForegroundColor Cyan
Write-Host "1. PayOS Dashboard: https://business.payos.vn/" -ForegroundColor White
Write-Host "2. Webhook URLs hiện tại đã cấu hình" -ForegroundColor White
Write-Host "3. Production server domain" -ForegroundColor White
Write-Host "4. Product payment có hoạt động trong thực tế không" -ForegroundColor White
