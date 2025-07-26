# API H?y Tour Slot C�ng Khai

## T?ng quan

API n�y cho ph�p tour company h?y m?t tour slot ?� ???c assign tour details v� ?ang ? tr?ng th�i **Public**. Khi h?y tour, h? th?ng s?:

1. ? ??i `IsActive = false` v� `Status = Cancelled` cho slot
2. ? H?y t?t c? booking li�n quan (`Status = CancelledByCompany`)
3. ? G?i email th�ng b�o xin l?i cho t?t c? kh�ch h�ng ?� ??t
4. ? Th�ng b�o nh�n vi�n s? ti?n h�nh ho�n ti?n
5. ? Release capacity t? TourOperation
6. ? G?i email x�c nh?n cho tour company

## Business Rules

### ? ?i?u ki?n ?? h?y tour:
- Slot ph?i c� `TourDetailsId` (?� ???c assign tour details)
- TourDetails ph?i ? tr?ng th�i `Public` 
- Ch? tour company s? h?u tour m?i c� th? h?y
- Slot ph?i ?ang `IsActive = true`

### ? Kh�ng th? h?y khi:
- Slot ch?a c� tour details assigned
- Tour details ch?a public
- Kh�ng ph?i ch? s? h?u tour
- Slot ?� b? h?y tr??c ?�

---

## API Endpoint

### **POST** `/api/TourSlot/{slotId}/cancel-public`

**Authorization:** `Bearer Token` (Role: "Tour Company")

#### Request Body

```json
{
  "reason": "Do th?i ti?t kh�ng thu?n l?i, ch�ng t�i bu?c ph?i h?y tour ?? ??m b?o an to�n cho kh�ch h�ng",
  "additionalMessage": "Ch�ng t�i s? m? l?i tour v?i l?ch tr�nh m?i trong th?i gian s?m nh?t"
}
```

#### Request Schema

| Field | Type | Required | Validation | Description |
|-------|------|----------|------------|-------------|
| `reason` | string | ? Yes | 10-1000 chars | L� do h?y tour (s? hi?n th? cho kh�ch h�ng) |
| `additionalMessage` | string | ? No | max 500 chars | Th�ng ?i?p b? sung (t�y ch?n) |

#### Response - Success (200)

```json
{
  "success": true,
  "message": "H?y tour th�nh c�ng. ?� th�ng b�o cho 5 kh�ch h�ng v� x? l� 3 booking.",
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
  "message": "Ch? c� th? h?y tour ?ang ? tr?ng th�i Public"
}
```

#### Response - Unauthorized (401)

```json
{
  "success": false,
  "message": "Kh�ng th? x�c th?c ng??i d�ng"
}
```

#### Response - Server Error (500)

```json
{
  "success": false,
  "message": "C� l?i x?y ra khi h?y tour",
  "error": "Internal server error details"
}
```

---

## Email Templates

### ?? Email g?i cho Kh�ch h�ng

**Subject:** `?? Th�ng b�o h?y tour: [T�n Tour]`

**?u ti�n g?i email:**
1. **ContactEmail** t? b?ng TourBooking (?u ti�n cao nh?t)
2. **User.Email** n?u ContactEmail tr?ng
3. **Validation** email h?p l? tr??c khi g?i

**N?i dung:** 
- ?? Th�ng b�o h?y tour v?i l� do c? th?
- ?? Th�ng tin booking (m�, s? kh�ch, s? ti?n)
- ?? Cam k?t ho�n ti?n ??y ?? trong 3-5 ng�y
- ?? **Nh�n vi�n s? li�n h? tr?c ti?p h? tr? ho�n ti?n**
- ?? Voucher b� ??p cho l?n ??t tour ti?p theo
- ?? G?i � tour kh�c, li�n h? h? tr?
- ?? L?i xin l?i ch�n th�nh v?i cam k?t h? tr?

### ?? Email g?i cho Tour Company

**Subject:** `? X�c nh?n h?y tour: [T�n Tour]`

**N?i dung:**
- ? X�c nh?n h?y tour th�nh c�ng
- ?? Th?ng k� (s? booking, l� do, th?i gian)
- ?? Checklist nh?ng g� ?� x? l�
- ?? C?m ?n s? d?ng h? th?ng c� tr�ch nhi?m

---

## Usage Examples

### C# Client

```csharp
var request = new CancelPublicTourSlotDto
{
    Reason = "Do b�o l?n, ch�ng t�i bu?c ph?i h?y tour ?? ??m b?o an to�n",
    AdditionalMessage = "Tour s? ???c m? l?i khi th?i ti?t ?n ??nh"
};

var response = await httpClient.PostAsJsonAsync(
    $"/api/TourSlot/{slotId}/cancel-public", 
    request);

if (response.IsSuccessStatusCode)
{
    var result = await response.Content.ReadFromJsonAsync<CancelTourSlotResultDto>();
    Console.WriteLine($"?� th�ng b�o cho {result.CustomersNotified} kh�ch h�ng");
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
        additionalMessage: "Ch�ng t�i s? li�n h? ?? h? tr? b?n ch?n tour m?i"
      })
    });

    const result = await response.json();
    
    if (result.success) {
      alert(`H?y tour th�nh c�ng! ?� th�ng b�o ${result.customersNotified} kh�ch h�ng`);
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
    "reason": "Do th?i ti?t x?u, ch�ng t�i bu?c ph?i h?y tour",
    "additionalMessage": "Xin l?i v� s? b?t ti?n n�y"
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

1. **Slot kh�ng t?n t?i**
   - Code: 400
   - Message: "Kh�ng t�m th?y tour slot"

2. **Slot ch?a c� tour details**
   - Code: 400  
   - Message: "Slot n�y ch?a ???c assign tour details, kh�ng th? h?y"

3. **Tour ch?a public**
   - Code: 400
   - Message: "Ch? c� th? h?y tour ?ang ? tr?ng th�i Public"

4. **Kh�ng c� quy?n**
   - Code: 400
   - Message: "B?n kh�ng c� quy?n h?y tour n�y"

5. **?� b? h?y tr??c ?�**
   - Code: 400
   - Message: "Tour slot ?� b? h?y tr??c ?�"

### Error Logging

```csharp
_logger.LogError(ex, "Error cancelling tour slot: {SlotId}", slotId);
_logger.LogWarning("Failed to send cancellation email to customer {CustomerEmail}", email);
_logger.LogInformation("Tour slot {SlotId} cancelled successfully. Affected bookings: {BookingCount}", slotId, bookingCount);
```

---

## Security Considerations

- ? **Authorization Required:** Ch? Tour Company c� th? g?i API
- ? **Ownership Check:** Ch? ch? s? h?u tour m?i c� th? h?y
- ? **Input Validation:** Validate reason length v� format
- ? **Transaction Safety:** S? d?ng database transaction ?? ??m b?o consistency
- ? **Error Handling:** Kh�ng expose sensitive information trong error messages

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

H? th?ng s? g?i email theo th? t? ?u ti�n:
1. **?u ti�n ??u ti�n**: `ContactEmail` t? b?ng TourBooking
2. **D? ph�ng**: `User.Email` n?u ContactEmail tr?ng
3. **Validation**: Ki?m tra email h?p l? tr??c khi g?i

```csharp
// Logic ?u ti�n email
var customerEmail = !string.IsNullOrEmpty(booking.ContactEmail) 
    ? booking.ContactEmail 
    : booking.User?.Email ?? "";
    
var customerName = !string.IsNullOrEmpty(booking.ContactName) 
    ? booking.ContactName 
    : booking.User?.Name ?? "Kh�ch h�ng";
```

### ?? **Quy tr�nh ho?t ??ng:**

1. **Validate**: Ki?m tra quy?n v� business rules
2. **Cancel Slot**: Set `IsActive = false`, `Status = Cancelled` 
3. **Cancel Bookings**: Update t?t c? booking th�nh `CancelledByCompany`
4. **Release Capacity**: Gi?m `CurrentBookings` trong TourOperation
5. **Send Customer Emails**: G?i email cho kh�ch h�ng (?u ti�n ContactEmail)
6. **Send Company Email**: G?i email x�c nh?n cho tour company
7. **Return Result**: Tr? v? s? l??ng kh�ch h�ng ???c th�ng b�o

### ?? **Database Priority**

| Priority | Field | Source | Fallback |
|----------|-------|--------|-----------|
| 1 | `ContactEmail` | TourBooking | User.Email |
| 2 | `ContactName` | TourBooking | User.Name |
| 3 | `ContactPhone` | TourBooking | User.PhoneNumber |

---