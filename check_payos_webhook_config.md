# KIỂM TRA CẤU HÌNH PAYOS WEBHOOK HIỆN TẠI
# ==========================================

## 🔍 THÔNG TIN PAYOS ĐÃ CÓ

### PayOS Configuration (từ appsettings.json):
```json
{
  "PayOS": {
    "ClientId": "918be0b9-be53-4935-aa8b-4f84d482259a",
    "ApiKey": "6dcc7ef9-f1ce-4c69-b902-08c46f346456", 
    "ChecksumKey": "280912f5532e5b76bda2e245f4c8643bcae79f19fa6498e33447a675afd6a181",
    "ReturnUrl": "https://localhost:7000/payment/return",
    "CancelUrl": "https://localhost:7000/payment/cancel"
  }
}
```

## 📋 WEBHOOK ENDPOINTS ĐÃ CÓ

### A. Product Payment Webhooks (đã hoạt động):
```
✅ POST /api/payment-callback/paid/{orderCode}
✅ POST /api/payment-callback/cancelled/{orderCode}
```

### B. Tour Booking Webhooks (mới tạo):
```
🆕 POST /api/tour-booking-payment/webhook/paid/{orderCode}
🆕 POST /api/tour-booking-payment/webhook/cancelled/{orderCode}
```

## ❓ CẦN KIỂM TRA

### 1. PayOS Dashboard Configuration
Cần kiểm tra trong PayOS Dashboard xem đã có webhook URLs nào được cấu hình:

**Cách kiểm tra:**
1. Đăng nhập: https://business.payos.vn/
2. Vào mục "Cấu hình" > "Webhook"
3. Xem danh sách webhook URLs hiện tại

**Có thể có các trường hợp:**

#### Trường hợp 1: Chỉ có Product Payment URLs
```
✅ https://yourdomain.com/api/payment-callback/paid/{orderCode}
✅ https://yourdomain.com/api/payment-callback/cancelled/{orderCode}
❌ Chưa có tour booking URLs
```

#### Trường hợp 2: Đã có cả Tour Booking URLs
```
✅ https://yourdomain.com/api/payment-callback/paid/{orderCode}
✅ https://yourdomain.com/api/payment-callback/cancelled/{orderCode}
✅ https://yourdomain.com/api/tour-booking-payment/webhook/paid/{orderCode}
✅ https://yourdomain.com/api/tour-booking-payment/webhook/cancelled/{orderCode}
```

#### Trường hợp 3: Chưa có webhook nào
```
❌ Chưa cấu hình webhook URLs
```

### 2. Server Domain/URL hiện tại
Cần xác định server đang chạy trên domain nào:
- Local: `https://localhost:7205`
- Production: `https://yourdomain.com`
- Development: `https://abc123.ngrok.io`

### 3. Product Payment có hoạt động không?
Nếu product payment đang hoạt động tốt thì:
- PayOS đã được cấu hình đúng
- Server đã accessible từ PayOS
- Chỉ cần thêm tour booking URLs

## 🎯 HÀNH ĐỘNG TIẾP THEO

### Nếu Product Payment đang hoạt động:
```
1. ✅ PayOS đã cấu hình sẵn
2. ✅ Server đã accessible
3. 🔄 Chỉ cần thêm tour booking webhook URLs vào PayOS Dashboard:
   - https://yourdomain.com/api/tour-booking-payment/webhook/paid/{orderCode}
   - https://yourdomain.com/api/tour-booking-payment/webhook/cancelled/{orderCode}
```

### Nếu Product Payment chưa hoạt động:
```
1. 🔄 Cần cấu hình PayOS Dashboard từ đầu
2. 🔄 Cần setup ngrok hoặc public domain
3. 🔄 Cần thêm cả product và tour booking URLs
```

## 🧪 CÁCH TEST

### Test Product Payment Webhook:
```bash
curl -X POST "https://localhost:7205/api/payment-callback/paid/TNDT1234567890" \
  -H "Content-Type: application/json"
```

### Test Tour Booking Webhook:
```bash
curl -X POST "https://localhost:7205/api/tour-booking-payment/webhook/paid/TNDT1234567890" \
  -H "Content-Type: application/json"
```

## 📞 LIÊN HỆ TEAM

**Cần hỏi team:**
1. Product payment có đang hoạt động tốt không?
2. PayOS Dashboard đã cấu hình webhook URLs nào?
3. Server production đang chạy trên domain nào?
4. Có cần setup ngrok cho development không?

**Thông tin cần có:**
- PayOS Dashboard login credentials
- Production server domain
- Current webhook URLs (nếu có)
