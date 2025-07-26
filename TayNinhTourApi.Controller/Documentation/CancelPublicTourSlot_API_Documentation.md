# API H?y Tour Slot Công Khai

## T?ng quan

API này cho phép tour company h?y m?t tour slot ?ã ???c assign tour details và ?ang ? tr?ng thái **Public**. Khi h?y tour, h? th?ng s?:

1. ? ??i `IsActive = false` và `Status = Cancelled` cho slot
2. ? H?y t?t c? booking liên quan (`Status = CancelledByCompany`)
3. ? G?i email thông báo xin l?i cho t?t c? khách hàng ?ã ??t
4. ? Thông báo nhân viên s? ti?n hành hoàn ti?n
5. ? Release capacity t? TourOperation
6. ? G?i email xác nh?n cho tour company

## Business Rules

### ? ?i?u ki?n ?? h?y tour:
- Slot ph?i có `TourDetailsId` (?ã ???c assign tour details)
- TourDetails ph?i ? tr?ng thái `Public` 
- Ch? tour company s? h?u tour m?i có th? h?y
- Slot ph?i ?ang `IsActive = true`

### ? Không th? h?y khi:
- Slot ch?a có tour details assigned
- Tour details ch?a public
- Không ph?i ch? s? h?u tour
- Slot ?ã b? h?y tr??c ?ó

---

## API Endpoint

### **POST** `/api/TourSlot/{slotId}/cancel-public`

**Authorization:** `Bearer Token` (Role: "Tour Company")

#### Request Body

```json
{
  "reason": "Do th?i ti?t không thu?n l?i, chúng tôi bu?c ph?i h?y tour ?? ??m b?o an toàn cho khách hàng",
  "additionalMessage": "Chúng tôi s? m? l?i tour v?i l?ch trình m?i trong th?i gian s?m nh?t"
}
```

#### Request Schema

| Field | Type | Required | Validation | Description |
|-------|------|----------|------------|-------------|
| `reason` | string | ? Yes | 10-1000 chars | Lý do h?y tour (s? hi?n th? cho khách hàng) |
| `additionalMessage` | string | ? No | max 500 chars | Thông ?i?p b? sung (tùy ch?n) |

#### Response - Success (200)

```json
{
  "success": true,
  "message": "H?y tour thành công. ?ã thông báo cho 5 khách hàng và x? lý 3 booking.",
  "customersNotified": 5,
  "affectedBookings": 3,
  "totalRefundAmount": 1500000,
  "affectedCustomers": [
    {
      "bookingId": "123e4567-e89b-12d3-a456-426614174000",
      "bookingCode": "TNT2025001",
      "customerName": "Nguy?n V?n A",
      "customerEmail": "nguyenvana@email.com",
      "numberOfGuests": 2,
      "refundAmount": 500000,
      "emailSent": true
    }
  ]
}
```

#### Response - Error (400)

```json
{
  "success": false,
  "message": "Ch? có th? h?y tour ?ang ? tr?ng thái Public"
}
```

#### Response - Unauthorized (401)

```json
{
  "success": false,
  "message": "Không th? xác th?c ng??i dùng"
}
```

#### Response - Server Error (500)

```json
{
  "success": false,
  "message": "Có l?i x?y ra khi h?y tour",
  "error": "Internal server error details"
}
```

---

## Email Templates

### ?? Email g?i cho Khách hàng

**Subject:** `?? Thông báo h?y tour: [Tên Tour]`

**?u tiên g?i email:**
1. **ContactEmail** t? b?ng TourBooking (?u tiên cao nh?t)
2. **User.Email** n?u ContactEmail tr?ng
3. **Validation** email h?p l? tr??c khi g?i

**N?i dung:** 
- ?? Thông báo h?y tour v?i lý do c? th?
- ?? Thông tin booking (mã, s? khách, s? ti?n)
- ?? Cam k?t hoàn ti?n ??y ?? trong 3-5 ngày
- ?? **Nhân viên s? liên h? tr?c ti?p h? tr? hoàn ti?n**
- ?? Voucher bù ??p cho l?n ??t tour ti?p theo
- ?? G?i ý tour khác, liên h? h? tr?
- ?? L?i xin l?i chân thành v?i cam k?t h? tr?

### ?? Email g?i cho Tour Company

**Subject:** `? Xác nh?n h?y tour: [Tên Tour]`

**N?i dung:**
- ? Xác nh?n h?y tour thành công
- ?? Th?ng kê (s? booking, lý do, th?i gian)
- ?? Checklist nh?ng gì ?ã x? lý
- ?? C?m ?n s? d?ng h? th?ng có trách nhi?m

---

## Usage Examples

### C# Client

```csharp
var request = new CancelPublicTourSlotDto
{
    Reason = "Do bão l?n, chúng tôi bu?c ph?i h?y tour ?? ??m b?o an toàn",
    AdditionalMessage = "Tour s? ???c m? l?i khi th?i ti?t ?n ??nh"
};

var response = await httpClient.PostAsJsonAsync(
    $"/api/TourSlot/{slotId}/cancel-public", 
    request);

if (response.IsSuccessStatusCode)
{
    var result = await response.Content.ReadFromJsonAsync<CancelTourSlotResultDto>();
    Console.WriteLine($"?ã thông báo cho {result.CustomersNotified} khách hàng");
}
```

### JavaScript/Fetch

```javascript
const cancelTour = async (slotId, reason) => {
  try {
    const response = await fetch(`/api/TourSlot/${slotId}/cancel-public`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
      },
      body: JSON.stringify({
        reason: reason,
        additionalMessage: "Chúng tôi s? liên h? ?? h? tr? b?n ch?n tour m?i"
      })
    });

    const result = await response.json();
    
    if (result.success) {
      alert(`H?y tour thành công! ?ã thông báo ${result.customersNotified} khách hàng`);
    } else {
      alert(`L?i: ${result.message}`);
    }
  } catch (error) {
    console.error('Error cancelling tour:', error);
  }
};
```

### cURL

```bash
curl -X POST "https://api.tayninhour.com/api/TourSlot/123e4567-e89b-12d3-a456-426614174000/cancel-public" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "reason": "Do th?i ti?t x?u, chúng tôi bu?c ph?i h?y tour",
    "additionalMessage": "Xin l?i vì s? b?t ti?n này"
  }'
```

---

## Database Changes

### TourSlot Table
```sql
UPDATE TourSlots 
SET IsActive = 0, 
    Status = 3, -- Cancelled
    UpdatedAt = GETUTCDATE()
WHERE Id = @SlotId;
```

### TourBooking Table  
```sql
UPDATE TourBookings 
SET Status = 4, -- CancelledByCompany
    CancelledDate = GETUTCDATE(),
    CancellationReason = @Reason,
    UpdatedAt = GETUTCDATE()
WHERE TourSlotId = @SlotId 
  AND Status IN (1, 2); -- Confirmed, Pending
```

### TourOperation Table
```sql
UPDATE TourOperations 
SET CurrentBookings = CurrentBookings - @ReleasedGuests
WHERE Id = @TourOperationId;
```

---

## Error Handling

### Common Error Scenarios

1. **Slot không t?n t?i**
   - Code: 400
   - Message: "Không tìm th?y tour slot"

2. **Slot ch?a có tour details**
   - Code: 400  
   - Message: "Slot này ch?a ???c assign tour details, không th? h?y"

3. **Tour ch?a public**
   - Code: 400
   - Message: "Ch? có th? h?y tour ?ang ? tr?ng thái Public"

4. **Không có quy?n**
   - Code: 400
   - Message: "B?n không có quy?n h?y tour này"

5. **?ã b? h?y tr??c ?ó**
   - Code: 400
   - Message: "Tour slot ?ã b? h?y tr??c ?ó"

### Error Logging

```csharp
_logger.LogError(ex, "Error cancelling tour slot: {SlotId}", slotId);
_logger.LogWarning("Failed to send cancellation email to customer {CustomerEmail}", email);
_logger.LogInformation("Tour slot {SlotId} cancelled successfully. Affected bookings: {BookingCount}", slotId, bookingCount);
```

---

## Security Considerations

- ? **Authorization Required:** Ch? Tour Company có th? g?i API
- ? **Ownership Check:** Ch? ch? s? h?u tour m?i có th? h?y
- ? **Input Validation:** Validate reason length và format
- ? **Transaction Safety:** S? d?ng database transaction ?? ??m b?o consistency
- ? **Error Handling:** Không expose sensitive information trong error messages

---

## Testing

### Unit Test Example

```csharp
[Test]
public async Task CancelPublicTourSlot_ValidRequest_ShouldReturnSuccess()
{
    // Arrange
    var slotId = Guid.NewGuid();
    var reason = "Weather conditions";
    var tourCompanyId = Guid.NewGuid();
    
    // Act
    var result = await _tourSlotService.CancelPublicTourSlotAsync(slotId, reason, tourCompanyId);
    
    // Assert
    Assert.IsTrue(result.Success);
    Assert.Greater(result.CustomersNotified, 0);
}
```

### Integration Test

```csharp
[Test]
public async Task POST_CancelPublicTourSlot_WithValidData_ShouldReturn200()
{
    // Arrange
    var request = new CancelPublicTourSlotDto { Reason = "Test reason" };
    
    // Act
    var response = await _client.PostAsJsonAsync($"/api/TourSlot/{_slotId}/cancel-public", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

## Workflow Ho?t ??ng

### ?? **Email Priority Logic**

H? th?ng s? g?i email theo th? t? ?u tiên:
1. **?u tiên ??u tiên**: `ContactEmail` t? b?ng TourBooking
2. **D? phóng**: `User.Email` n?u ContactEmail tr?ng
3. **Validation**: Ki?m tra email h?p l? tr??c khi g?i

```csharp
// Logic ?u tiên email
var customerEmail = !string.IsNullOrEmpty(booking.ContactEmail) 
    ? booking.ContactEmail 
    : booking.User?.Email ?? "";
    
var customerName = !string.IsNullOrEmpty(booking.ContactName) 
    ? booking.ContactName 
    : booking.User?.Name ?? "Khách hàng";
```

### ?? **Quy trình ho?t ??ng:**

1. **Validate**: Ki?m tra quy?n và business rules
2. **Cancel Slot**: Set `IsActive = false`, `Status = Cancelled` 
3. **Cancel Bookings**: Update t?t c? booking thành `CancelledByCompany`
4. **Release Capacity**: Gi?m `CurrentBookings` trong TourOperation
5. **Send Customer Emails**: G?i email cho khách hàng (?u tiên ContactEmail)
6. **Send Company Email**: G?i email xác nh?n cho tour company
7. **Return Result**: Tr? v? s? l??ng khách hàng ???c thông báo

### ?? **Database Priority**

| Priority | Field | Source | Fallback |
|----------|-------|--------|-----------|
| 1 | `ContactEmail` | TourBooking | User.Email |
| 2 | `ContactName` | TourBooking | User.Name |
| 3 | `ContactPhone` | TourBooking | User.PhoneNumber |

---