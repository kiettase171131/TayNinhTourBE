# 🚀 TOUR BOOKING SYSTEM - TASK LIST

## 📋 **TỔNG QUAN**
Hệ thống Tour Booking với Early Bird Pricing đã được implement hoàn chỉnh và build thành công. Cần thực hiện các bước cuối để deploy và test.

---

## ⏳ **TASKS CẦN THỰC HIỆN**

### 🗄️ **1. DATABASE MIGRATION** (Ưu tiên cao)
**Mục tiêu**: Apply database schema cho Tour Booking System

#### **1.1 Apply Migration Script**
- [ ] Mở MySQL Workbench hoặc phpMyAdmin
- [ ] Connect tới database `tayninhtourapidb`
- [ ] Chạy script: `TayNinhTourBE/migration_tour_booking_system.sql`
- [ ] Verify tables được tạo:
  - `TourCompanies` table
  - `TourBookings` có thêm fields: `DiscountPercent`, `OriginalPrice`, `PayOsOrderCode`, `QRCodeData`, `RowVersion`
  - `TourDetails` có thêm field: `ImageUrl`

#### **1.2 Verify Migration**
- [ ] Check `__EFMigrationsHistory` table có 4 records:
  - `20250711163032_Init`
  - `20250713125113_AddImageUrlToTourDetails`
  - `20250713140159_AddTourBookingSystemEntities`
  - `20250713144940_AddTourBookingSystem`

---

### ⚙️ **2. CONFIGURATION** (Ưu tiên cao)
**Mục tiêu**: Cấu hình các settings cần thiết

#### **2.1 PayOS Configuration**
- [ ] Mở `TayNinhTourApi.Controller/appsettings.json`
- [ ] Thêm PayOS settings:
```json
{
  "PayOS": {
    "ClientId": "your-payos-client-id",
    "ApiKey": "your-payos-api-key",
    "ChecksumKey": "your-payos-checksum-key",
    "ReturnUrl": "https://your-domain.com/payment/return",
    "CancelUrl": "https://your-domain.com/payment/cancel"
  }
}
```

#### **2.2 Email Configuration**
- [ ] Verify Email settings trong `appsettings.json`:
```json
{
  "EmailSettings": {
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "TayNinh Tour",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

#### **2.3 Hangfire Configuration**
- [ ] Verify Hangfire connection string trong `appsettings.json`
- [ ] Ensure background jobs được enable

---

### 🧪 **3. API TESTING** (Ưu tiên trung bình)
**Mục tiêu**: Test các APIs mới được tạo

#### **3.1 Setup Test Data**
- [ ] Tạo test TourCompany:
```sql
INSERT INTO TourCompanies (Id, UserId, CompanyName, Wallet, RevenueHold, IsActive, IsDeleted, CreatedById, CreatedAt)
VALUES (UUID(), 'existing-user-id', 'Test Tour Company', 0, 0, 1, 0, 'existing-user-id', NOW());
```

#### **3.2 Test User Tour Booking APIs**
- [ ] **GET** `/api/user-tour-booking/available-tours` - Lấy danh sách tours
- [ ] **GET** `/api/user-tour-booking/tour-details/{id}` - Chi tiết tour
- [ ] **POST** `/api/user-tour-booking/calculate-price` - Tính giá với early bird
- [ ] **POST** `/api/user-tour-booking/book` - Đặt tour
- [ ] **GET** `/api/user-tour-booking/my-bookings` - Lịch sử booking

#### **3.3 Test Payment APIs**
- [ ] **POST** `/api/tour-booking-payment/create-payment` - Tạo payment
- [ ] **POST** `/api/tour-booking-payment/payos-webhook` - Test webhook
- [ ] **GET** `/api/tour-booking-payment/payment-status/{orderCode}` - Check status

---

### 🔧 **4. BUSINESS LOGIC TESTING** (Ưu tiên trung bình)
**Mục tiêu**: Verify business rules hoạt động đúng

#### **4.1 Early Bird Pricing Test**
- [ ] Tạo tour khởi hành sau 30 ngày
- [ ] Book trong 15 ngày đầu → Verify giảm 25%
- [ ] Book sau 15 ngày → Verify giá gốc

#### **4.2 Revenue Hold System Test**
- [ ] Complete payment → Verify tiền vào `RevenueHold`
- [ ] Wait 3 days → Verify tiền chuyển sang `Wallet`

#### **4.3 Auto Cancel Test**
- [ ] Tạo tour với capacity 10
- [ ] Book chỉ 4 slots (< 50%)
- [ ] Set tour date = 2 days from now
- [ ] Verify tour bị auto-cancel

---

### 📚 **5. DOCUMENTATION** (Ưu tiên thấp)
**Mục tiêu**: Document hệ thống cho team

#### **5.1 API Documentation**
- [ ] Update Swagger comments
- [ ] Add example requests/responses
- [ ] Document error codes

#### **5.2 Business Logic Documentation**
- [ ] Document early bird pricing rules
- [ ] Document revenue hold process
- [ ] Document auto-cancel logic

---

### 🐛 **6. BUG FIXES & IMPROVEMENTS** (Nếu cần)
**Mục tiêu**: Fix issues phát hiện trong testing

#### **6.1 Potential Issues**
- [ ] Foreign key constraints (nếu có lỗi)
- [ ] PayOS integration errors
- [ ] Email sending failures
- [ ] Background job failures

#### **6.2 Performance Optimization**
- [ ] Add database indexes nếu cần
- [ ] Optimize queries
- [ ] Add caching nếu cần

---

## 🎯 **PRIORITY ORDER**

1. **Database Migration** (Bắt buộc) - 15 phút
2. **Configuration** (Bắt buộc) - 10 phút  
3. **API Testing** (Quan trọng) - 30 phút
4. **Business Logic Testing** (Quan trọng) - 20 phút
5. **Documentation** (Tùy chọn) - 15 phút
6. **Bug Fixes** (Nếu cần) - Tùy theo issue

---

## 📞 **SUPPORT COMMANDS**

### Database Commands:
```bash
# Check migration status
dotnet ef migrations list --project TayNinhTourApi.DataAccessLayer --startup-project TayNinhTourApi.Controller

# Generate new migration (if needed)
dotnet ef migrations add MigrationName --project TayNinhTourApi.DataAccessLayer --startup-project TayNinhTourApi.Controller
```

### Build Commands:
```bash
# Build solution
dotnet build

# Run application
dotnet run --project TayNinhTourApi.Controller
```

### Test Commands:
```bash
# Test specific endpoint
curl -X GET "https://localhost:7000/api/user-tour-booking/available-tours"
```

---

## ✅ **COMPLETION CHECKLIST**

- [ ] Database schema applied successfully
- [ ] All configurations set
- [ ] APIs responding correctly
- [ ] Business logic working as expected
- [ ] No critical bugs found
- [ ] Documentation updated

**🎉 Khi hoàn thành tất cả, hệ thống Tour Booking sẽ sẵn sàng production!**
