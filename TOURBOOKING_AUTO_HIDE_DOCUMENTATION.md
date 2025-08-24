# TourBooking Auto-Hide Documentation

## T?ng quan
Tính n?ng này m? r?ng `TourBookingCleanupService` ?? t? ??ng ?n các TourBooking có tr?ng thái `Pending`, `CancelledByCustomer`, ho?c `CancelledByCompany` sau 3 ngày b?ng cách ?ánh d?u `IsDeleted = true`. ??ng th?i c?ng ?n t?t c? `TourBookingGuest` liên quan ?? ??m b?o tính nh?t quán d? li?u.

## Cách ho?t ??ng

### TourBookingCleanupService (M? r?ng)
- **File**: `TayNinhTourApi.BusinessLogicLayer\Services\TourBookingCleanupService.cs`
- **Lo?i**: Background Service (IHostedService)
- **Ch?c n?ng kép**:
  1. **Cleanup expired bookings**: Ch?y m?i 5 phút
  2. **Hide old bookings**: Ch?y m?i 6 gi?

### Logic x? lý cho Hide Old Bookings

#### 1. Tìm ki?m TourBookings c?n ?n
```csharp
// ?i?u ki?n:
// - Status = Pending ho?c CancelledByCustomer ho?c CancelledByCompany
// - CreatedAt < (Hi?n t?i - 3 ngày)
// - IsDeleted = false (ch?a b? ?n)
var bookingsToHide = await unitOfWork.TourBookingRepository.GetQueryable()
    .Where(b => (b.Status == BookingStatus.Pending || 
                b.Status == BookingStatus.CancelledByCustomer || 
                b.Status == BookingStatus.CancelledByCompany)
             && b.CreatedAt < cutoffDate
             && !b.IsDeleted)
    .Include(b => b.Guests.Where(g => !g.IsDeleted))
    .ToListAsync();
```

#### 2. ?n TourBooking và TourBookingGuest
```csharp
// ?n TourBooking
booking.IsDeleted = true;
booking.UpdatedAt = DateTime.UtcNow;
await unitOfWork.TourBookingRepository.UpdateAsync(booking);

// ?n t?t c? TourBookingGuest liên quan
foreach (var guest in booking.Guests.Where(g => !g.IsDeleted))
{
    guest.IsDeleted = true;
    guest.UpdatedAt = DateTime.UtcNow;
    unitOfWork.Context.Update(guest);
}
```

#### 3. Transaction Safety
- S? d?ng database transaction ?? ??m b?o tính nh?t quán
- ExecutionStrategy v?i retry logic
- Rollback n?u có l?i x?y ra

## Tr?ng thái TourBooking ???c x? lý

### BookingStatus.Pending (0)
- Booking ?ang ch? thanh toán
- Sau 3 ngày s? ???c ?n t? ??ng

### BookingStatus.CancelledByCustomer (2)
- Booking ?ã b? h?y b?i khách hàng
- Sau 3 ngày s? ???c ?n t? ??ng

### BookingStatus.CancelledByCompany (3)
- Booking ?ã b? h?y b?i tour company
- Sau 3 ngày s? ???c ?n t? ??ng

### Không ???c ?n
- **BookingStatus.Confirmed (1)**: Booking ?ã ???c xác nh?n
- **BookingStatus.Completed (4)**: Tour ?ã hoàn thành
- **BookingStatus.NoShow (5)**: Khách không xu?t hi?n
- **BookingStatus.Refunded (6)**: ?ã hoàn ti?n

## C?u hình

### Th?i gian ch?y
```csharp
private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Cleanup expired bookings
private readonly TimeSpan _hideOldBookingsInterval = TimeSpan.FromHours(6); // Hide old bookings
```

### Th?i gian ?n
```csharp
private readonly TimeSpan _hideAfterDays = TimeSpan.FromDays(3); // ?n sau 3 ngày
```

### Dual Job Structure
```csharp
// Job 1: Cleanup expired bookings (m?i 5 phút)
await CleanupExpiredBookingsAsync();

// Job 2: Hide old bookings (m?i 6 gi?)
var now = DateTime.UtcNow;
if (now - _lastHideOldBookingsRun >= _hideOldBookingsInterval)
{
    await HideOldBookingsAsync();
    _lastHideOldBookingsRun = now;
}
```

## Entities ???c x? lý

### TourBooking
- **Primary entity** ???c ?n
- Các fields ???c c?p nh?t:
  - `IsDeleted = true`
  - `UpdatedAt = DateTime.UtcNow`

### TourBookingGuest
- **Related entity** ???c ?n cùng lúc
- M?i quan h?: 1 TourBooking có nhi?u TourBookingGuest
- Các fields ???c c?p nh?t:
  - `IsDeleted = true`
  - `UpdatedAt = DateTime.UtcNow`

## Logging

Service ghi log chi ti?t cho c? hai ch?c n?ng:

### Hide Old Bookings Log Levels
- **Information**: Thông tin t?ng quan v? quá trình ?n bookings
- **Debug**: Chi ti?t t?ng booking và guest ???c ?n
- **Warning**: C?nh báo khi không có booking nào ???c ?n
- **Error**: L?i trong quá trình x? lý

### Ví d? Log
```
[Information] TourBookingCleanupService started - Cleanup expired bookings (5min) + Hide old bookings (6h)
[Information] Found 8 old tour bookings to hide (Pending/Cancelled bookings older than 3 days)
[Debug] Hidden booking a1b2c3d4-e5f6-7890-abcd-ef1234567890 (Code: TB20250103001, Status: CancelledByCustomer, Age: 4 days) with 2 guests
[Information] Successfully hidden 8 old tour bookings and 15 guests from frontend display
```

## Impact trên Frontend

### TourBooking Queries
Frontend queries s? t? ??ng l?c các bookings ?ã ?n:
```sql
WHERE IsDeleted = false
```

### TourBookingGuest Queries  
Guest queries c?ng s? t? ??ng l?c:
```sql
WHERE IsDeleted = false
```

### Soft Delete Filter
C? hai entities ??u có soft delete query filter:
```csharp
// T? ??ng áp d?ng trong EF Core
builder.HasQueryFilter(entity => !entity.IsDeleted);
```

## Database Schema Impact

### TourBooking Table
| Column | Type | Description |
|--------|------|-------------|
| `IsDeleted` | bit | ?ánh d?u booking ?ã ?n |
| `UpdatedAt` | datetime2 | Th?i gian c?p nh?t cu?i |

### TourBookingGuest Table  
| Column | Type | Description |
|--------|------|-------------|
| `IsDeleted` | bit | ?ánh d?u guest ?ã ?n |
| `UpdatedAt` | datetime2 | Th?i gian c?p nh?t cu?i |

## Performance Considerations

### Query Optimization
```csharp
// Include guests trong single query ?? tránh N+1 problem
.Include(b => b.Guests.Where(g => !g.IsDeleted))
```

### Batch Processing
- X? lý nhi?u bookings trong m?t transaction
- Update guests qua DbContext.Update() cho performance

### Index Usage
Các indexes h? tr? performance:
```sql
-- TourBooking indexes
IX_TourBookings_Status_CreatedAt_IsDeleted
IX_TourBookings_IsDeleted

-- TourBookingGuest indexes  
IX_TourBookingGuests_TourBookingId (v?i filter IsDeleted = 0)
IX_TourBookingGuests_IsDeleted
```

## Error Handling

### Graceful Degradation
- Continue processing n?u m?t booking failed
- Detailed error logging cho t?ng booking
- Transaction rollback n?u toàn b? batch failed

### Exception Types
```csharp
try
{
    // Hide booking and guests
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error hiding tour booking {BookingId}", booking.Id);
    // Continue v?i booking ti?p theo
}
```

## Monitoring và Maintenance

### Metrics Tracking
- S? l??ng bookings ???c ?n
- S? l??ng guests ???c ?n  
- Th?i gian x? lý
- Error rates

### Health Monitoring
```csharp
// Log sample
[Information] Successfully hidden 8 old tour bookings and 15 guests from frontend display
```

### Manual Recovery
Admin có th? manually restore bookings:
```sql
-- Restore booking và guests
UPDATE TourBookings SET IsDeleted = 0 WHERE Id = @bookingId;
UPDATE TourBookingGuests SET IsDeleted = 0 WHERE TourBookingId = @bookingId;
```

## Integration v?i Existing Features

### TourBookingRepository
- GetUserBookingsWithFilterAsync() t? ??ng l?c IsDeleted = false
- Các methods khác c?ng respect soft delete filter

### QR Code System
- QR codes c?a guests b? ?n s? không accessible
- Check-in system s? reject QR c?a guests ?ã ?n

### Payment System
- Payment transactions không b? ?nh h??ng
- Refund system v?n ho?t ??ng bình th??ng

## Testing Strategy

### Unit Tests
1. Test ?n bookings v?i status Pending/Cancelled
2. Test không ?n bookings v?i status khác
3. Test ?n guests liên quan
4. Test transaction rollback khi có l?i

### Integration Tests  
1. End-to-end test v?i database
2. Verify soft delete filters
3. Check frontend queries không tr? v? hidden bookings

### Load Testing
1. Performance v?i large dataset
2. Transaction timeout handling
3. Memory usage optimization

---

## K?t lu?n

TourBookingCleanupService (m? r?ng) cung c?p m?t gi?i pháp toàn di?n ??:

1. **Cleanup expired bookings** - X? lý bookings h?t h?n thanh toán
2. **Hide old bookings** - ?n bookings c? không c?n thi?t
3. **Maintain data consistency** - ??m b?o c? booking và guests ???c ?n cùng lúc
4. **Preserve audit trail** - Soft delete, không xóa d? li?u th?c s?

Tính n?ng này giúp c?i thi?n tr?i nghi?m ng??i dùng b?ng cách ?n các bookings c? không liên quan, ??ng th?i v?n b?o toàn tính toàn v?n và kh? n?ng audit c?a d? li?u.