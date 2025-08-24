# Order Auto-Hide Documentation

## T?ng quan
T�nh n?ng n�y t? ??ng ?n c�c ??n h�ng (Order) c� tr?ng th�i `Pending` ho?c `Cancelled` sau 3 ng�y b?ng c�ch ?�nh d?u `IsDeleted = true`. ?i?u n�y gi�p Frontend kh�ng hi?n th? c�c ??n h�ng c? qu� l�u, t?o tr?i nghi?m ng??i d�ng t?t h?n.

## C�ch ho?t ??ng

### OrderCleanupService
- **File**: `TayNinhTourApi.BusinessLogicLayer\Services\OrderCleanupService.cs`
- **Lo?i**: Background Service (IHostedService)
- **T?n su?t**: Ch?y m?i 6 gi?
- **?i?u ki?n**: ?n c�c order ?� t?n t?i h?n 3 ng�y

### Logic x? l�
1. **T�m ki?m Orders c?n ?n**:
   ```csharp
   // ?i?u ki?n:
   // - Status = Pending ho?c Cancelled
   // - CreatedAt < (Hi?n t?i - 3 ng�y)
   // - IsDeleted = false (ch?a b? ?n)
   var ordersToHide = await unitOfWork.OrderRepository.GetQueryable()
       .Where(o => (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Cancelled)
                && o.CreatedAt < cutoffDate
                && !o.IsDeleted)
       .ToListAsync();
   ```

2. **Th?c hi?n ?n Orders**:
   ```csharp
   // ?�nh d?u IsDeleted = true
   order.IsDeleted = true;
   order.UpdatedAt = DateTime.UtcNow;
   ```

3. **Transaction Safety**: S? d?ng database transaction ?? ??m b?o t�nh nh?t qu�n

## Tr?ng th�i Order ???c x? l�

### OrderStatus.Pending (0)
- ??n h�ng ?ang ch? thanh to�n
- Sau 3 ng�y s? ???c ?n t? ??ng

### OrderStatus.Cancelled (2)
- ??n h�ng ?� b? h?y (b?i kh�ch h�ng ho?c h? th?ng)
- Sau 3 ng�y s? ???c ?n t? ??ng

### OrderStatus.Paid (1)
- **KH�NG** b? ?n t? ??ng
- ??n h�ng ?� thanh to�n th�nh c�ng s? ???c gi? l?i

## C?u h�nh

### Th?i gian ch?y
```csharp
private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Ch?y m?i 6 gi?
```

### Th?i gian ?n
```csharp
private readonly TimeSpan _hideAfterDays = TimeSpan.FromDays(3); // ?n sau 3 ng�y
```

## ??ng k� Service

### Program.cs
```csharp
// ??ng k� OrderCleanupService l�m Hosted Service
builder.Services.AddHostedService<OrderCleanupService>();
```

## Logging

Service ghi log chi ti?t c�c ho?t ??ng:

### Log Levels
- **Information**: Th�ng tin t?ng quan v? qu� tr�nh cleanup
- **Debug**: Chi ti?t t?ng order ???c x? l�
- **Warning**: C?nh b�o khi kh�ng c� order n�o ???c ?n
- **Error**: L?i trong qu� tr�nh x? l�

### V� d? Log
```
[Information] OrderCleanupService started - Will hide Pending/Cancelled orders after 3 days
[Information] Found 5 orders to hide (Pending/Cancelled orders older than 3 days)
[Debug] Hidden order a1b2c3d4-e5f6-7890-abcd-ef1234567890 (PayOS: TNDT1234567890, Status: Cancelled, Age: 4 days)
[Information] Successfully hidden 5 old orders from frontend display
```

## Impact tr�n Frontend

### Tr??c khi ?n
- Order hi?n th? trong danh s�ch v?i status Pending/Cancelled
- Ng??i d�ng c� th? th?y c�c ??n h�ng c?

### Sau khi ?n
- Order c� `IsDeleted = true`
- Frontend queries v?i ?i?u ki?n `WHERE IsDeleted = false` s? kh�ng tr? v? nh?ng orders n�y
- Giao di?n s?ch s? h?n, ch? hi?n th? orders li�n quan

## L?u � k? thu?t

### Database Impact
- **Kh�ng x�a** d? li?u th?c s? (soft delete)
- Ch? ?�nh d?u `IsDeleted = true`
- D? li?u v?n t?n t?i cho m?c ?�ch audit/backup

### Performance
- Ch?y batch processing v?i transaction
- Retry mechanism v?i ExecutionStrategy
- Kh�ng ?nh h??ng ??n performance runtime

### Error Handling
- Graceful handling c?a database errors
- Continue processing n?u m?t order failed
- Detailed error logging

## T??ng t�c v?i c�c Systems kh�c

### PaymentTransactionRepository
- Kh�ng ?nh h??ng ??n payment transactions
- Payment history v?n ???c gi? nguy�n

### OrderRepository
- S? d?ng existing repository patterns
- T??ng th�ch v?i soft delete filtering

### Audit Trail
- `UpdatedAt` ???c c?p nh?t khi ?n order
- Log entries ?? track cleanup activity

## Monitoring v� Maintenance

### Health Check
- Service t? ??ng restart n?u crashed
- Log entries ?? monitor service status

### Manual Override
- Admin c� th? manually un-hide orders n?u c?n
- Database access tr?c ti?p ?? restore orders

### Configuration Changes
```csharp
// C� th? ?i?u ch?nh th?i gian trong code:
private readonly TimeSpan _hideAfterDays = TimeSpan.FromDays(5); // Thay ??i t? 3 th�nh 5 ng�y
private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(12); // Thay ??i t? 6 th�nh 12 gi?
```

## Testing

### Unit Test Scenarios
1. Test v?i orders c? h?n 3 ng�y (status Pending/Cancelled)
2. Test v?i orders m?i h?n 3 ng�y (kh�ng ???c ?n)
3. Test v?i orders status Paid (kh�ng ???c ?n)
4. Test error handling scenarios

### Integration Test
1. End-to-end test v?i database
2. Verify transaction safety
3. Check logging output

---

## K?t lu?n

OrderCleanupService cung c?p m?t gi?i ph�p t? ??ng, an to�n v� hi?u qu? ?? ?n c�c ??n h�ng c? kh�ng c?n thi?t, gi�p c?i thi?n tr?i nghi?m ng??i d�ng m� kh�ng l�m m?t d? li?u quan tr?ng.