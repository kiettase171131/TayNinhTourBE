# TourBooking Auto-Hide Documentation

## T?ng quan
T�nh n?ng n�y m? r?ng `TourBookingCleanupService` ?? t? ??ng ?n c�c TourBooking c� tr?ng th�i `Pending`, `CancelledByCustomer`, ho?c `CancelledByCompany` sau 3 ng�y b?ng c�ch ?�nh d?u `IsDeleted = true`. ??ng th?i c?ng ?n t?t c? `TourBookingGuest` li�n quan ?? ??m b?o t�nh nh?t qu�n d? li?u.

## C�ch ho?t ??ng

### TourBookingCleanupService (M? r?ng)
- **File**: `TayNinhTourApi.BusinessLogicLayer\Services\TourBookingCleanupService.cs`
- **Lo?i**: Background Service (IHostedService)
- **Ch?c n?ng k�p**:
  1. **Cleanup expired bookings**: Ch?y m?i 5 ph�t
  2. **Hide old bookings**: Ch?y m?i 6 gi?

### Logic x? l� cho Hide Old Bookings

#### 1. T�m ki?m TourBookings c?n ?n
```csharp
// ?i?u ki?n:
// - Status = Pending ho?c CancelledByCustomer ho?c CancelledByCompany
// - CreatedAt < (Hi?n t?i - 3 ng�y)
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

#### 2. ?n TourBooking v� TourBookingGuest
```csharp
// ?n TourBooking
booking.IsDeleted = true;
booking.UpdatedAt = DateTime.UtcNow;
await unitOfWork.TourBookingRepository.UpdateAsync(booking);

// ?n t?t c? TourBookingGuest li�n quan
foreach (var guest in booking.Guests.Where(g => !g.IsDeleted))
{
    guest.IsDeleted = true;
    guest.UpdatedAt = DateTime.UtcNow;
    unitOfWork.Context.Update(guest);
}
```

#### 3. Transaction Safety
- S? d?ng database transaction ?? ??m b?o t�nh nh?t qu�n
- ExecutionStrategy v?i retry logic
- Rollback n?u c� l?i x?y ra

## Tr?ng th�i TourBooking ???c x? l�

### BookingStatus.Pending (0)
- Booking ?ang ch? thanh to�n
- Sau 3 ng�y s? ???c ?n t? ??ng

### BookingStatus.CancelledByCustomer (2)
- Booking ?� b? h?y b?i kh�ch h�ng
- Sau 3 ng�y s? ???c ?n t? ??ng

### BookingStatus.CancelledByCompany (3)
- Booking ?� b? h?y b?i tour company
- Sau 3 ng�y s? ???c ?n t? ??ng

### Kh�ng ???c ?n
- **BookingStatus.Confirmed (1)**: Booking ?� ???c x�c nh?n
- **BookingStatus.Completed (4)**: Tour ?� ho�n th�nh
- **BookingStatus.NoShow (5)**: Kh�ch kh�ng xu?t hi?n
- **BookingStatus.Refunded (6)**: ?� ho�n ti?n

## C?u h�nh

### Th?i gian ch?y
```csharp
private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Cleanup expired bookings
private readonly TimeSpan _hideOldBookingsInterval = TimeSpan.FromHours(6); // Hide old bookings
```

### Th?i gian ?n
```csharp
private readonly TimeSpan _hideAfterDays = TimeSpan.FromDays(3); // ?n sau 3 ng�y
```

### Dual Job Structure
```csharp
// Job 1: Cleanup expired bookings (m?i 5 ph�t)
await CleanupExpiredBookingsAsync();

// Job 2: Hide old bookings (m?i 6 gi?)
var now = DateTime.UtcNow;
if (now - _lastHideOldBookingsRun >= _hideOldBookingsInterval)
{
    await HideOldBookingsAsync();
    _lastHideOldBookingsRun = now;
}
```

## Entities ???c x? l�

### TourBooking
- **Primary entity** ???c ?n
- C�c fields ???c c?p nh?t:
  - `IsDeleted = true`
  - `UpdatedAt = DateTime.UtcNow`

### TourBookingGuest
- **Related entity** ???c ?n c�ng l�c
- M?i quan h?: 1 TourBooking c� nhi?u TourBookingGuest
- C�c fields ???c c?p nh?t:
  - `IsDeleted = true`
  - `UpdatedAt = DateTime.UtcNow`

## Logging

Service ghi log chi ti?t cho c? hai ch?c n?ng:

### Hide Old Bookings Log Levels
- **Information**: Th�ng tin t?ng quan v? qu� tr�nh ?n bookings
- **Debug**: Chi ti?t t?ng booking v� guest ???c ?n
- **Warning**: C?nh b�o khi kh�ng c� booking n�o ???c ?n
- **Error**: L?i trong qu� tr�nh x? l�

### V� d? Log
```
[Information] TourBookingCleanupService started - Cleanup expired bookings (5min) + Hide old bookings (6h)
[Information] Found 8 old tour bookings to hide (Pending/Cancelled bookings older than 3 days)
[Debug] Hidden booking a1b2c3d4-e5f6-7890-abcd-ef1234567890 (Code: TB20250103001, Status: CancelledByCustomer, Age: 4 days) with 2 guests
[Information] Successfully hidden 8 old tour bookings and 15 guests from frontend display
```

## Impact tr�n Frontend

### TourBooking Queries
Frontend queries s? t? ??ng l?c c�c bookings ?� ?n:
```sql
WHERE IsDeleted = false
```

### TourBookingGuest Queries  
Guest queries c?ng s? t? ??ng l?c:
```sql
WHERE IsDeleted = false
```

### Soft Delete Filter
C? hai entities ??u c� soft delete query filter:
```csharp
// T? ??ng �p d?ng trong EF Core
builder.HasQueryFilter(entity => !entity.IsDeleted);
```

## Database Schema Impact

### TourBooking Table
| Column | Type | Description |
|--------|------|-------------|
| `IsDeleted` | bit | ?�nh d?u booking ?� ?n |
| `UpdatedAt` | datetime2 | Th?i gian c?p nh?t cu?i |

### TourBookingGuest Table  
| Column | Type | Description |
|--------|------|-------------|
| `IsDeleted` | bit | ?�nh d?u guest ?� ?n |
| `UpdatedAt` | datetime2 | Th?i gian c?p nh?t cu?i |

## Performance Considerations

### Query Optimization
```csharp
// Include guests trong single query ?? tr�nh N+1 problem
.Include(b => b.Guests.Where(g => !g.IsDeleted))
```

### Batch Processing
- X? l� nhi?u bookings trong m?t transaction
- Update guests qua DbContext.Update() cho performance

### Index Usage
C�c indexes h? tr? performance:
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
- Transaction rollback n?u to�n b? batch failed

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

## Monitoring v� Maintenance

### Metrics Tracking
- S? l??ng bookings ???c ?n
- S? l??ng guests ???c ?n  
- Th?i gian x? l�
- Error rates

### Health Monitoring
```csharp
// Log sample
[Information] Successfully hidden 8 old tour bookings and 15 guests from frontend display
```

### Manual Recovery
Admin c� th? manually restore bookings:
```sql
-- Restore booking v� guests
UPDATE TourBookings SET IsDeleted = 0 WHERE Id = @bookingId;
UPDATE TourBookingGuests SET IsDeleted = 0 WHERE TourBookingId = @bookingId;
```

## Integration v?i Existing Features

### TourBookingRepository
- GetUserBookingsWithFilterAsync() t? ??ng l?c IsDeleted = false
- C�c methods kh�c c?ng respect soft delete filter

### QR Code System
- QR codes c?a guests b? ?n s? kh�ng accessible
- Check-in system s? reject QR c?a guests ?� ?n

### Payment System
- Payment transactions kh�ng b? ?nh h??ng
- Refund system v?n ho?t ??ng b�nh th??ng

## Testing Strategy

### Unit Tests
1. Test ?n bookings v?i status Pending/Cancelled
2. Test kh�ng ?n bookings v?i status kh�c
3. Test ?n guests li�n quan
4. Test transaction rollback khi c� l?i

### Integration Tests  
1. End-to-end test v?i database
2. Verify soft delete filters
3. Check frontend queries kh�ng tr? v? hidden bookings

### Load Testing
1. Performance v?i large dataset
2. Transaction timeout handling
3. Memory usage optimization

---

## K?t lu?n

TourBookingCleanupService (m? r?ng) cung c?p m?t gi?i ph�p to�n di?n ??:

1. **Cleanup expired bookings** - X? l� bookings h?t h?n thanh to�n
2. **Hide old bookings** - ?n bookings c? kh�ng c?n thi?t
3. **Maintain data consistency** - ??m b?o c? booking v� guests ???c ?n c�ng l�c
4. **Preserve audit trail** - Soft delete, kh�ng x�a d? li?u th?c s?

T�nh n?ng n�y gi�p c?i thi?n tr?i nghi?m ng??i d�ng b?ng c�ch ?n c�c bookings c? kh�ng li�n quan, ??ng th?i v?n b?o to�n t�nh to�n v?n v� kh? n?ng audit c?a d? li?u.