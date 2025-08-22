# Order Auto-Hide Documentation

## T?ng quan
Tính n?ng này t? ??ng ?n các ??n hàng (Order) có tr?ng thái `Pending` ho?c `Cancelled` sau 3 ngày b?ng cách ?ánh d?u `IsDeleted = true`. ?i?u này giúp Frontend không hi?n th? các ??n hàng c? quá lâu, t?o tr?i nghi?m ng??i dùng t?t h?n.

## Cách ho?t ??ng

### OrderCleanupService
- **File**: `TayNinhTourApi.BusinessLogicLayer\Services\OrderCleanupService.cs`
- **Lo?i**: Background Service (IHostedService)
- **T?n su?t**: Ch?y m?i 6 gi?
- **?i?u ki?n**: ?n các order ?ã t?n t?i h?n 3 ngày

### Logic x? lý
1. **Tìm ki?m Orders c?n ?n**:
   ```csharp
   // ?i?u ki?n:
   // - Status = Pending ho?c Cancelled
   // - CreatedAt < (Hi?n t?i - 3 ngày)
   // - IsDeleted = false (ch?a b? ?n)
   var ordersToHide = await unitOfWork.OrderRepository.GetQueryable()
       .Where(o => (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Cancelled)
                && o.CreatedAt < cutoffDate
                && !o.IsDeleted)
       .ToListAsync();
   ```

2. **Th?c hi?n ?n Orders**:
   ```csharp
   // ?ánh d?u IsDeleted = true
   order.IsDeleted = true;
   order.UpdatedAt = DateTime.UtcNow;
   ```

3. **Transaction Safety**: S? d?ng database transaction ?? ??m b?o tính nh?t quán

## Tr?ng thái Order ???c x? lý

### OrderStatus.Pending (0)
- ??n hàng ?ang ch? thanh toán
- Sau 3 ngày s? ???c ?n t? ??ng

### OrderStatus.Cancelled (2)
- ??n hàng ?ã b? h?y (b?i khách hàng ho?c h? th?ng)
- Sau 3 ngày s? ???c ?n t? ??ng

### OrderStatus.Paid (1)
- **KHÔNG** b? ?n t? ??ng
- ??n hàng ?ã thanh toán thành công s? ???c gi? l?i

## C?u hình

### Th?i gian ch?y
```csharp
private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Ch?y m?i 6 gi?
```

### Th?i gian ?n
```csharp
private readonly TimeSpan _hideAfterDays = TimeSpan.FromDays(3); // ?n sau 3 ngày
```

## ??ng ký Service

### Program.cs
```csharp
// ??ng ký OrderCleanupService làm Hosted Service
builder.Services.AddHostedService<OrderCleanupService>();
```

## Logging

Service ghi log chi ti?t các ho?t ??ng:

### Log Levels
- **Information**: Thông tin t?ng quan v? quá trình cleanup
- **Debug**: Chi ti?t t?ng order ???c x? lý
- **Warning**: C?nh báo khi không có order nào ???c ?n
- **Error**: L?i trong quá trình x? lý

### Ví d? Log
```
[Information] OrderCleanupService started - Will hide Pending/Cancelled orders after 3 days
[Information] Found 5 orders to hide (Pending/Cancelled orders older than 3 days)
[Debug] Hidden order a1b2c3d4-e5f6-7890-abcd-ef1234567890 (PayOS: TNDT1234567890, Status: Cancelled, Age: 4 days)
[Information] Successfully hidden 5 old orders from frontend display
```

## Impact trên Frontend

### Tr??c khi ?n
- Order hi?n th? trong danh sách v?i status Pending/Cancelled
- Ng??i dùng có th? th?y các ??n hàng c?

### Sau khi ?n
- Order có `IsDeleted = true`
- Frontend queries v?i ?i?u ki?n `WHERE IsDeleted = false` s? không tr? v? nh?ng orders này
- Giao di?n s?ch s? h?n, ch? hi?n th? orders liên quan

## L?u ý k? thu?t

### Database Impact
- **Không xóa** d? li?u th?c s? (soft delete)
- Ch? ?ánh d?u `IsDeleted = true`
- D? li?u v?n t?n t?i cho m?c ?ích audit/backup

### Performance
- Ch?y batch processing v?i transaction
- Retry mechanism v?i ExecutionStrategy
- Không ?nh h??ng ??n performance runtime

### Error Handling
- Graceful handling c?a database errors
- Continue processing n?u m?t order failed
- Detailed error logging

## T??ng tác v?i các Systems khác

### PaymentTransactionRepository
- Không ?nh h??ng ??n payment transactions
- Payment history v?n ???c gi? nguyên

### OrderRepository
- S? d?ng existing repository patterns
- T??ng thích v?i soft delete filtering

### Audit Trail
- `UpdatedAt` ???c c?p nh?t khi ?n order
- Log entries ?? track cleanup activity

## Monitoring và Maintenance

### Health Check
- Service t? ??ng restart n?u crashed
- Log entries ?? monitor service status

### Manual Override
- Admin có th? manually un-hide orders n?u c?n
- Database access tr?c ti?p ?? restore orders

### Configuration Changes
```csharp
// Có th? ?i?u ch?nh th?i gian trong code:
private readonly TimeSpan _hideAfterDays = TimeSpan.FromDays(5); // Thay ??i t? 3 thành 5 ngày
private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(12); // Thay ??i t? 6 thành 12 gi?
```

## Testing

### Unit Test Scenarios
1. Test v?i orders c? h?n 3 ngày (status Pending/Cancelled)
2. Test v?i orders m?i h?n 3 ngày (không ???c ?n)
3. Test v?i orders status Paid (không ???c ?n)
4. Test error handling scenarios

### Integration Test
1. End-to-end test v?i database
2. Verify transaction safety
3. Check logging output

---

## K?t lu?n

OrderCleanupService cung c?p m?t gi?i pháp t? ??ng, an toàn và hi?u qu? ?? ?n các ??n hàng c? không c?n thi?t, giúp c?i thi?n tr?i nghi?m ng??i dùng mà không làm m?t d? li?u quan tr?ng.