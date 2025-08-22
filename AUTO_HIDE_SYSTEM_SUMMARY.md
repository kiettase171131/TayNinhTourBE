# Auto-Hide System Summary

## T?ng quan
H? th?ng t? ??ng ?n (Auto-Hide System) bao g?m hai background services ?? t? ??ng ?n c�c records c? sau 3 ng�y, gi�p Frontend c� giao di?n s?ch s? v� kh�ng hi?n th? d? li?u kh�ng c?n thi?t qu� l�u.

## Components

### 1. OrderCleanupService
**File**: `TayNinhTourApi.BusinessLogicLayer\Services\OrderCleanupService.cs`

#### Ch?c n?ng
- T? ??ng ?n **Product Orders** c� status `Pending` ho?c `Cancelled` sau 3 ng�y
- Ch?y m?i 6 gi?

#### Entities x? l�
- **Order**: ??n h�ng s?n ph?m
- Kh�ng c� b?ng con li�n quan

#### Status ???c ?n
- `OrderStatus.Pending` (0)
- `OrderStatus.Cancelled` (2)

#### Status kh�ng b? ?n
- `OrderStatus.Paid` (1)

---

### 2. TourBookingCleanupService (Extended)
**File**: `TayNinhTourApi.BusinessLogicLayer\Services\TourBookingCleanupService.cs`

#### Ch?c n?ng k�p
1. **Cleanup expired bookings**: H?y bookings h?t h?n thanh to�n (m?i 5 ph�t)
2. **Hide old bookings**: ?n tour bookings c? (m?i 6 gi?)

#### Entities x? l�
- **TourBooking**: Booking tour ch�nh  
- **TourBookingGuest**: Kh�ch h�ng trong booking (b?ng con)

#### Status ???c ?n
- `BookingStatus.Pending` (0)
- `BookingStatus.CancelledByCustomer` (2)  
- `BookingStatus.CancelledByCompany` (3)

#### Status kh�ng b? ?n
- `BookingStatus.Confirmed` (1)
- `BookingStatus.Completed` (4)
- `BookingStatus.NoShow` (5)
- `BookingStatus.Refunded` (6)

---

## C?u h�nh chung

### Th?i gian ?n
```csharp
private readonly TimeSpan _hideAfterDays = TimeSpan.FromDays(3); // 3 ng�y cho c? hai services
```

### T?n su?t ch?y
```csharp
// OrderCleanupService
private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6);

// TourBookingCleanupService  
private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5); // Cleanup expired
private readonly TimeSpan _hideOldBookingsInterval = TimeSpan.FromHours(6); // Hide old
```

## ??ng k� Services

### Program.cs
```csharp
// Register Background Job Services as Hosted Services
builder.Services.AddHostedService<BackgroundJobService>();
builder.Services.AddHostedService<TourAutoCancelService>();
builder.Services.AddHostedService<TourBookingCleanupService>(); // Extended v?i hide old bookings
builder.Services.AddHostedService<OrderCleanupService>(); // NEW: Auto-hide old orders
builder.Services.AddHostedService<TourRevenueTransferService>();
builder.Services.AddHostedService<TourReminderService>();
```

## Database Impact

### Soft Delete Pattern
C? hai services ??u s? d?ng soft delete:
```csharp
entity.IsDeleted = true;
entity.UpdatedAt = DateTime.UtcNow;
```

### Query Filters
EF Core t? ??ng �p d?ng soft delete filters:
```csharp
builder.HasQueryFilter(entity => !entity.IsDeleted);
```

## Frontend Impact

### Before Auto-Hide
- Users th?y t?t c? orders/bookings c?
- Giao di?n l?n x?n v?i d? li?u kh�ng li�n quan
- Performance impact khi load qu� nhi?u records

### After Auto-Hide  
- Ch? hi?n th? orders/bookings li�n quan
- Giao di?n s?ch s?, t?p trung v�o d? li?u quan tr?ng
- Better UX v?i less clutter

## Logging Strategy

### Log Levels s? d?ng
- **Information**: T?ng quan operations
- **Debug**: Chi ti?t t?ng record ???c x? l�  
- **Warning**: Kh�ng c� records ?? x? l�
- **Error**: L?i trong qu� tr�nh x? l�

### Sample Log Output
```
[Information] OrderCleanupService started - Will hide Pending/Cancelled orders after 3 days
[Information] TourBookingCleanupService started - Cleanup expired bookings (5min) + Hide old bookings (6h)
[Information] Found 5 orders to hide (Pending/Cancelled orders older than 3 days)
[Information] Found 8 old tour bookings to hide (Pending/Cancelled bookings older than 3 days)
[Information] Successfully hidden 5 old orders from frontend display
[Information] Successfully hidden 8 old tour bookings and 15 guests from frontend display
```

## Performance Considerations

### Batch Processing
- X? l� multiple records trong m?t transaction
- ExecutionStrategy v?i retry logic
- Graceful error handling

### Query Optimization
```csharp
// Efficient querying v?i proper includes
.Include(b => b.Guests.Where(g => !g.IsDeleted))
```

### Memory Management
- Using statement cho scope management
- Proper disposal c?a resources
- Avoiding memory leaks trong long-running services

## Monitoring & Maintenance

### Health Checks
- Services t? ??ng restart n?u crashed
- Log entries ?? track service status
- Exception handling ?? prevent service shutdown

### Manual Override Options
```sql
-- Restore hidden order
UPDATE Orders SET IsDeleted = 0 WHERE Id = @orderId;

-- Restore hidden booking v� guests
UPDATE TourBookings SET IsDeleted = 0 WHERE Id = @bookingId;
UPDATE TourBookingGuests SET IsDeleted = 0 WHERE TourBookingId = @bookingId;
```

### Configuration Tuning
Admin c� th? adjust timing n?u c?n:
```csharp
// Thay ??i t? 3 ng�y th�nh 5 ng�y
private readonly TimeSpan _hideAfterDays = TimeSpan.FromDays(5);

// Thay ??i t?n su?t ch?y
private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(12);
```

## Testing Coverage

### Unit Tests Required
1. Test hide logic cho t?ng service
2. Test timing conditions  
3. Test error handling scenarios
4. Test transaction rollback

### Integration Tests Required
1. End-to-end database operations
2. Service startup/shutdown cycles
3. Concurrent operation handling
4. Performance under load

## Benefits Summary

### For End Users
- ? Cleaner interface without old irrelevant data
- ? Faster page loads v?i less data
- ? Better focus on current/relevant orders & bookings

### For Developers  
- ? Automated maintenance, no manual intervention needed
- ? Consistent data management across both systems
- ? Preserved audit trail v?i soft delete
- ? Easy to monitor and troubleshoot

### For System Performance
- ? Reduced query result sets
- ? Better database performance v?i filtered queries  
- ? Improved caching efficiency
- ? Reduced memory usage trong Frontend applications

---

## Future Enhancements

### Potential Improvements
1. **Configurable timing**: Move intervals to appsettings.json
2. **Admin dashboard**: UI ?? monitor v� control services  
3. **Bulk restore**: Tools ?? restore hidden records if needed
4. **Analytics**: Track patterns trong hidden data
5. **Notifications**: Alert admins khi large amounts are hidden

### Extension Points
```csharp
// C� th? extend cho other entities
public interface IAutoHideService<T> where T : BaseEntity
{
    Task HideOldRecordsAsync(TimeSpan olderThan, params object[] statusesToHide);
}
```

---

**Auto-Hide System ?� ???c tri?n khai th�nh c�ng v� s?n s�ng ho?t ??ng!** ??