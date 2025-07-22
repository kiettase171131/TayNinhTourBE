# HƯỚNG DẪN CẤU HÌNH PAYOS WEBHOOK CHO TOUR BOOKING
# ========================================================

## 🚨 VẤN ĐỀ ĐÃ ĐƯỢC KHẮC PHỤC

**Vấn đề trước đây:** Tour booking chỉ dựa vào frontend callback, dẫn đến mất thanh toán khi có lỗi network.

**Giải pháp:** Đã thêm PayOS webhook server-to-server cho tour booking.

## 📋 CÁC ENDPOINT WEBHOOK MỚI CHO TOUR BOOKING

### A. PayOS Webhook Endpoints (Server-to-Server) - ✅ ĐỒNG NHẤT VỚI PRODUCT PAYMENT
```
POST /api/tour-booking-payment/webhook/paid/{orderCode}
- Mô tả: PayOS tự động gọi khi thanh toán tour booking thành công
- Chức năng: Cập nhật booking status = Confirmed + Tạo QR code + Thêm revenue
- Headers: Không cần Authorization (giống product payment)
- Format: Đơn giản, không cần signature verification
- Security: ✅ Tương tự product payment đang hoạt động

POST /api/tour-booking-payment/webhook/cancelled/{orderCode}
- Mô tả: PayOS tự động gọi khi thanh toán tour booking bị hủy
- Chức năng: Cập nhật booking status = CancelledByCustomer + Release capacity
- Headers: Không cần Authorization (giống product payment)
- Format: Đơn giản, không cần signature verification
- Security: ✅ Tương tự product payment đang hoạt động
```

### B. Frontend Callback Endpoints (Client-to-Server) - Đã có sẵn
```
POST /api/tour-booking-payment/payment-success
- Mô tả: Frontend gọi để xử lý UI sau khi thanh toán
- Chức năng: Backup cho webhook, xử lý UI response

POST /api/tour-booking-payment/payment-cancel
- Mô tả: Frontend gọi để xử lý UI khi hủy thanh toán
- Chức năng: Backup cho webhook, xử lý UI response
```

## 🔧 CẤU HÌNH PAYOS DASHBOARD

### Bước 1: Đăng nhập PayOS Dashboard
1. Truy cập: https://business.payos.vn/
2. Đăng nhập bằng tài khoản PayOS
3. Vào mục "Cấu hình" > "Webhook"

### Bước 2: Thêm Webhook URLs cho Tour Booking

**Server URLs (giống format product payment):**
```
Thanh toán thành công:
https://localhost:7205/api/tour-booking-payment/webhook/paid/{orderCode}

Thanh toán bị hủy:
https://localhost:7205/api/tour-booking-payment/webhook/cancelled/{orderCode}
```

**Cấu hình PayOS Dashboard (nếu cần):**
```
Nếu PayOS đã hoạt động với product payment thì có thể:
1. Sử dụng cùng domain/server
2. Chỉ cần thêm tour booking endpoints
3. Không cần ngrok nếu server đã public

Thanh toán thành công:
https://yourdomain.com/api/tour-booking-payment/webhook/paid/{orderCode}

Thanh toán bị hủy:
https://yourdomain.com/api/tour-booking-payment/webhook/cancelled/{orderCode}
```

### Bước 3: Cấu hình Events
Tick chọn các event sau:
- [x] Payment Success (PAID)
- [x] Payment Cancelled (CANCELLED)
- [x] Payment Failed (nếu cần)

### Bước 4: Security Settings
- **Authentication:** Không cần Authorization header (giống product payment)
- **Signature Verification:** Không cần (đơn giản như product payment)
- **HTTPS:** Bắt buộc phải dùng HTTPS (nếu production)
- **IP Whitelist:** Cho phép PayOS IPs (nếu cần)
- **Format:** Đơn giản, chỉ cần orderCode trong URL path

## 🔄 FLOW HOẠT ĐỘNG MỚI

### Khi thanh toán thành công:
```
1. User thanh toán thành công trên PayOS
2. PayOS gửi webhook: POST /api/tour-booking-payment/webhook/paid/{orderCode}
   - Đơn giản như product payment
   - Không cần body phức tạp
3. Backend xử lý webhook:
   - Tìm booking bằng orderCode
   - Cập nhật status = Confirmed
   - Tạo QR code cho customer
   - Thêm tiền vào revenue hold
4. PayOS redirect user về frontend
5. Frontend gọi: POST /api/tour-booking-payment/payment-success (backup)
6. Hiển thị trang success với thông tin booking
```

### Khi thanh toán bị hủy:
```
1. User hủy thanh toán trên PayOS
2. PayOS gửi webhook: POST /api/tour-booking-payment/webhook/cancelled/{orderCode}
   - Đơn giản như product payment
   - Không cần body phức tạp
3. Backend xử lý webhook:
   - Tìm booking bằng orderCode
   - Cập nhật status = CancelledByCustomer
   - Release capacity cho tour
4. PayOS redirect user về frontend
5. Frontend gọi: POST /api/tour-booking-payment/payment-cancel (backup)
6. Hiển thị trang cancel
```

## ✅ LỢI ÍCH CỦA GIẢI PHÁP MỚI

1. **Độ tin cậy cao:** Webhook server-to-server đảm bảo thanh toán được xử lý
2. **Backup mechanism:** Frontend callback vẫn hoạt động như backup
3. **Không mất thanh toán:** Ngay cả khi frontend gặp lỗi, webhook vẫn xử lý
4. **Consistent với product payment:** Cùng pattern với product payment system

## 🧪 TESTING

### A. Test webhook với server hiện tại + ngrok
1. Đảm bảo server đang chạy trên `https://localhost:7205`
2. Cài đặt ngrok: https://ngrok.com/
3. Chạy ngrok: `ngrok http https://localhost:7205`
4. Lấy public URL từ ngrok (vd: https://abc123.ngrok.io)
5. Cấu hình webhook URL trong PayOS dashboard với ngrok URL

### B. Test với Postman (cần signature hợp lệ)
```http
POST /api/tour-booking-payment/webhook/paid
Content-Type: application/json

{
  "code": "00",
  "desc": "success",
  "success": true,
  "data": {
    "orderCode": 1234567890,
    "amount": 100000,
    "description": "Tour booking payment",
    "accountNumber": "12345678",
    "reference": "TF230204212323",
    "transactionDateTime": "2023-02-04 18:25:00",
    "currency": "VND",
    "paymentLinkId": "124c33293c43417ab7879e14c8d9eb18",
    "code": "00",
    "desc": "Thành công",
    "counterAccountBankId": "",
    "counterAccountBankName": "",
    "counterAccountName": "",
    "counterAccountNumber": "",
    "virtualAccountName": "",
    "virtualAccountNumber": ""
  },
  "signature": "8d8640d802576397a1ce45ebda7f835055768ac7ad2e0bfb77f9b8f12cca4c7f"
}
```

**Lưu ý:** Signature phải được tính toán đúng theo thuật toán HMAC-SHA256 của PayOS.

## 📝 NOTES QUAN TRỌNG

1. **Webhook URLs phải khác với product payment:**
   - Product: `/api/payment-callback/paid/{orderCode}`
   - Tour Booking: `/api/tour-booking-payment/webhook/paid/{orderCode}`

2. **Frontend URLs vẫn giữ nguyên:**
   - Success: `https://tndt.netlify.app/payment-success?orderId={orderCode}&orderCode={orderCode}`
   - Cancel: `https://tndt.netlify.app/payment-cancel?orderId={orderCode}&orderCode={orderCode}`

3. **Monitoring:** Theo dõi logs để đảm bảo webhook hoạt động đúng

4. **Fallback:** Nếu webhook thất bại, frontend callback vẫn có thể xử lý

## 🔒 SECURITY BEST PRACTICES

### 1. Signature Verification
- ✅ **Luôn verify signature** từ PayOS webhook
- ✅ **Sử dụng PayOS SDK** để verify (đã implement)
- ✅ **Reject invalid signatures** với HTTP 401
- ✅ **Log security events** để monitoring

### 2. HTTPS Requirements
- ✅ **Chỉ accept HTTPS** webhook URLs
- ✅ **Valid SSL certificate** cho production
- ✅ **TLS 1.2+** minimum

### 3. Rate Limiting & Monitoring
- ⚠️ **Implement rate limiting** cho webhook endpoints
- ⚠️ **Monitor failed signature verifications**
- ⚠️ **Alert on suspicious activities**

### 4. Configuration Security
- ✅ **Store PayOS keys** trong appsettings.json hoặc environment variables
- ✅ **Không hardcode** sensitive information
- ✅ **Rotate keys** định kỳ

### 5. Error Handling
- ✅ **Không expose** internal errors trong response
- ✅ **Log detailed errors** cho debugging
- ✅ **Return appropriate HTTP status codes**
