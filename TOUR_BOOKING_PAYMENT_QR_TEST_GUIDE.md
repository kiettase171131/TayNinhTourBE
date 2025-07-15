# HƯỚNG DẪN TEST API THANH TOÁN VÀ TẠO MÃ QR CHO TOUR BOOKING

## 📋 TỔNG QUAN

Tài liệu này hướng dẫn cách test flow thanh toán và tạo mã QR cho việc booking tour details trong hệ thống TayNinhTour.

## 🔄 FLOW THANH TOÁN TOUR BOOKING

### 1. Luồng chính (Happy Path)
```
1. User tạo booking tour → Nhận PaymentUrl
2. User thanh toán qua PayOS → Scan QR code
3. PayOS callback → Backend xử lý payment success
4. Tạo QR code cho customer → Lưu vào database
5. User nhận booking confirmation + QR code
```

### 2. Các bước chi tiết

#### Bước 1: Tạo Tour Booking
```http
POST /api/user-tour-booking/create-booking
Authorization: Bearer {token}
Content-Type: application/json

{
  "tourOperationId": "75cc6ed4-1d26-41c6-82f6-6f6bcd91b2d6",
  "numberOfGuests": 3,
  "adultCount": 2,
  "childCount": 1,
  "contactName": "Nguyễn Văn A",
  "contactPhone": "0123456789",
  "contactEmail": "test@example.com",
  "customerNotes": "Yêu cầu đặc biệt"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Tạo booking thành công",
  "bookingId": "booking-guid",
  "bookingCode": "TNDT240101001",
  "paymentUrl": "https://pay.payos.vn/web/...",
  "originalPrice": 1500000,
  "discountPercent": 10,
  "finalPrice": 1350000,
  "pricingType": "EarlyBird",
  "bookingDate": "2024-01-01T10:00:00Z",
  "tourStartDate": "2024-01-15T08:00:00Z"
}
```

#### Bước 2: Thanh toán qua PayOS
- User click vào `paymentUrl` 
- Scan QR code để thanh toán
- PayOS tự động gọi callback khi thanh toán thành công

#### Bước 3: PayOS Callback (Tự động)
```http
POST /api/tour-booking-payment/payment-success
Content-Type: application/json

{
  "orderCode": "TNDT240101001"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Thanh toán thành công",
  "orderCode": "TNDT240101001"
}
```

## 🧪 CÁCH TEST API

### 1. Chuẩn bị Test Environment

#### 1.1 Lấy JWT Token
```http
POST /api/Auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "12345678h@"
}
```

#### 1.2 Kiểm tra TourOperation có sẵn
```http
GET /api/TourDetails/{tourDetailsId}
Authorization: Bearer {token}
```

### 2. Test Booking Creation

#### 2.1 Test Happy Path
```bash
curl -X POST "http://localhost:5267/api/user-tour-booking/create-booking" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "tourOperationId": "75cc6ed4-1d26-41c6-82f6-6f6bcd91b2d6",
    "numberOfGuests": 2,
    "adultCount": 2,
    "childCount": 0,
    "contactName": "Test User",
    "contactPhone": "0123456789",
    "contactEmail": "test@example.com"
  }'
```

#### 2.2 Test Validation Errors
```bash
# Test với numberOfGuests không khớp
curl -X POST "http://localhost:5267/api/user-tour-booking/create-booking" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "tourOperationId": "75cc6ed4-1d26-41c6-82f6-6f6bcd91b2d6",
    "numberOfGuests": 5,
    "adultCount": 2,
    "childCount": 1
  }'
```

#### 2.3 Test Capacity Limit
```bash
# Test với số lượng khách vượt quá capacity
curl -X POST "http://localhost:5267/api/user-tour-booking/create-booking" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "tourOperationId": "75cc6ed4-1d26-41c6-82f6-6f6bcd91b2d6",
    "numberOfGuests": 100,
    "adultCount": 100,
    "childCount": 0,
    "contactName": "Test User",
    "contactPhone": "0123456789"
  }'
```

### 3. Test Payment Callback

#### 3.1 Test Payment Success
```bash
curl -X POST "http://localhost:5267/api/tour-booking-payment/payment-success" \
  -H "Content-Type: application/json" \
  -d '{
    "orderCode": "TNDT240101001"
  }'
```

#### 3.2 Test Payment Cancel
```bash
curl -X POST "http://localhost:5267/api/tour-booking-payment/payment-cancel" \
  -H "Content-Type: application/json" \
  -d '{
    "orderCode": "TNDT240101001"
  }'
```

### 4. Test Booking Lookup

#### 4.1 Lookup by PayOS Order Code
```bash
curl -X GET "http://localhost:5267/api/tour-booking-payment/lookup/TNDT240101001" \
  -H "Content-Type: application/json"
```

#### 4.2 Get User Bookings
```bash
curl -X GET "http://localhost:5267/api/user-tour-booking/my-bookings?pageIndex=1&pageSize=10" \
  -H "Authorization: Bearer {token}"
```

## 🎯 QR CODE GENERATION

### 1. QR Code được tạo tự động
- Sau khi thanh toán thành công
- QR code chứa thông tin booking
- Được lưu vào database và trả về URL

### 2. QR Code Data Structure
```json
{
  "BookingId": "booking-guid",
  "BookingCode": "TNDT240101001",
  "UserId": "user-guid",
  "TourOperationId": "operation-guid",
  "NumberOfGuests": 3,
  "TotalPrice": 1350000,
  "BookingDate": "2024-01-01T10:00:00Z",
  "Status": "Confirmed"
}
```

### 3. Test QR Code Generation
```bash
# QR code được tạo tự động trong payment success callback
# Kiểm tra trong response của my-bookings
curl -X GET "http://localhost:5267/api/user-tour-booking/my-bookings" \
  -H "Authorization: Bearer {token}"
```

## 🔍 DEBUGGING & TROUBLESHOOTING

### 1. Common Issues

#### 1.1 "Tour đã hết chỗ"
- Kiểm tra `currentBookings` vs `maxGuests` trong TourOperation
- Test với tour operation khác có capacity

#### 1.2 "Không tìm thấy booking"
- Kiểm tra PayOsOrderCode có đúng format
- Verify booking tồn tại trong database

#### 1.3 "Token expired"
- Lấy token mới từ login API
- Kiểm tra token format trong Authorization header

### 2. Database Queries for Debugging

```sql
-- Kiểm tra booking status
SELECT Id, BookingCode, PayOsOrderCode, Status, TotalPrice 
FROM TourBookings 
WHERE PayOsOrderCode = 'TNDT240101001';

-- Kiểm tra tour operation capacity
SELECT Id, MaxGuests, CurrentBookings, (MaxGuests - CurrentBookings) as Available
FROM TourOperations 
WHERE Id = '75cc6ed4-1d26-41c6-82f6-6f6bcd91b2d6';

-- Kiểm tra QR code data
SELECT Id, BookingCode, QRCodeData, CreatedAt
FROM TourBookings 
WHERE QRCodeData IS NOT NULL;
```

### 3. Log Monitoring
- Kiểm tra console logs trong PaymentController
- Monitor logs trong UserTourBookingService
- Verify PayOS callback logs

## 📝 TEST CHECKLIST

### ✅ Booking Creation Tests
- [ ] Valid booking request
- [ ] Invalid numberOfGuests validation
- [ ] Capacity limit validation
- [ ] Missing required fields
- [ ] Invalid tourOperationId
- [ ] Unauthorized access

### ✅ Payment Tests
- [ ] Payment success callback
- [ ] Payment cancel callback
- [ ] Duplicate payment handling
- [ ] Invalid orderCode handling

### ✅ QR Code Tests
- [ ] QR code generation after payment
- [ ] QR code data structure
- [ ] QR code URL accessibility

### ✅ Integration Tests
- [ ] End-to-end booking flow
- [ ] Concurrent booking handling
- [ ] Payment timeout scenarios
- [ ] Database consistency checks

## 📚 API ENDPOINTS REFERENCE

### Core Booking APIs
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/user-tour-booking/create-booking` | Tạo booking mới | ✅ |
| `GET` | `/api/user-tour-booking/my-bookings` | Lấy bookings của user | ✅ |
| `GET` | `/api/user-tour-booking/check-availability` | Kiểm tra availability | ✅ |

### Payment APIs
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/tour-booking-payment/payment-success` | PayOS success callback | ❌ |
| `POST` | `/api/tour-booking-payment/payment-cancel` | PayOS cancel callback | ❌ |
| `GET` | `/api/tour-booking-payment/lookup/{orderCode}` | Lookup booking info | ❌ |

### Legacy Product Payment APIs (for reference)
| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/Product/checkout` | Product checkout | ✅ |
| `POST` | `/api/payment-callback/paid/{orderCode}` | Product payment success | ❌ |
| `POST` | `/api/payment-callback/cancelled/{orderCode}` | Product payment cancel | ❌ |

## 🔧 POSTMAN COLLECTION EXAMPLES

### 1. Environment Variables
```json
{
  "baseUrl": "http://localhost:5267",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tourOperationId": "75cc6ed4-1d26-41c6-82f6-6f6bcd91b2d6"
}
```

### 2. Pre-request Script (for auth)
```javascript
// Set authorization header
pm.request.headers.add({
    key: 'Authorization',
    value: 'Bearer ' + pm.environment.get('token')
});
```

### 3. Test Scripts Examples
```javascript
// Test for successful booking creation
pm.test("Booking created successfully", function () {
    pm.response.to.have.status(200);
    const response = pm.response.json();
    pm.expect(response.success).to.be.true;
    pm.expect(response.bookingId).to.exist;
    pm.expect(response.paymentUrl).to.exist;

    // Save for next requests
    pm.environment.set("bookingId", response.bookingId);
    pm.environment.set("paymentUrl", response.paymentUrl);
});

// Test for payment callback
pm.test("Payment processed successfully", function () {
    pm.response.to.have.status(200);
    const response = pm.response.json();
    pm.expect(response.success).to.be.true;
    pm.expect(response.message).to.include("thành công");
});
```

## 🎯 ADVANCED TESTING SCENARIOS

### 1. Concurrency Testing
```bash
# Test multiple bookings simultaneously
for i in {1..5}; do
  curl -X POST "http://localhost:5267/api/user-tour-booking/create-booking" \
    -H "Authorization: Bearer {token}" \
    -H "Content-Type: application/json" \
    -d '{
      "tourOperationId": "75cc6ed4-1d26-41c6-82f6-6f6bcd91b2d6",
      "numberOfGuests": 2,
      "adultCount": 2,
      "childCount": 0,
      "contactName": "Concurrent Test '$i'",
      "contactPhone": "012345678'$i'"
    }' &
done
wait
```

### 2. Load Testing với Apache Bench
```bash
# Test booking endpoint performance
ab -n 100 -c 10 -H "Authorization: Bearer {token}" \
   -H "Content-Type: application/json" \
   -p booking_payload.json \
   http://localhost:5267/api/user-tour-booking/create-booking
```

### 3. Error Handling Tests
```bash
# Test với invalid JSON
curl -X POST "http://localhost:5267/api/user-tour-booking/create-booking" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{"invalid": json}'

# Test với missing Authorization
curl -X POST "http://localhost:5267/api/user-tour-booking/create-booking" \
  -H "Content-Type: application/json" \
  -d '{
    "tourOperationId": "75cc6ed4-1d26-41c6-82f6-6f6bcd91b2d6",
    "numberOfGuests": 2
  }'
```

## 🔐 SECURITY TESTING

### 1. Authentication Tests
```bash
# Test với expired token
curl -X POST "http://localhost:5267/api/user-tour-booking/create-booking" \
  -H "Authorization: Bearer expired.token.here" \
  -H "Content-Type: application/json" \
  -d '{...}'

# Test với malformed token
curl -X POST "http://localhost:5267/api/user-tour-booking/create-booking" \
  -H "Authorization: Bearer invalid-token" \
  -H "Content-Type: application/json" \
  -d '{...}'
```

### 2. Input Validation Tests
```bash
# Test SQL injection attempt
curl -X POST "http://localhost:5267/api/user-tour-booking/create-booking" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "tourOperationId": "75cc6ed4-1d26-41c6-82f6-6f6bcd91b2d6",
    "contactName": "Robert\"; DROP TABLE TourBookings; --",
    "numberOfGuests": 1
  }'

# Test XSS attempt
curl -X POST "http://localhost:5267/api/user-tour-booking/create-booking" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "tourOperationId": "75cc6ed4-1d26-41c6-82f6-6f6bcd91b2d6",
    "contactName": "<script>alert(\"XSS\")</script>",
    "numberOfGuests": 1
  }'
```

## 🚀 NEXT STEPS

1. **Frontend Integration**: Implement payment flow trong React app
2. **QR Code Scanner**: Tạo QR scanner cho tour guide app
3. **Notification System**: Thêm email/SMS notification
4. **Analytics**: Track booking success rate và payment metrics
5. **Mobile App**: Tích hợp booking flow vào mobile app
6. **Real-time Updates**: WebSocket cho real-time booking status
