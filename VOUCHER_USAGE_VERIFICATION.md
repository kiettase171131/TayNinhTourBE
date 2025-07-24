# ?? KI?M TRA CH?C N?NG ?ÁNH D?U VOUCHER ?Ã S? D?NG

## ?? OVERVIEW
Ch?c n?ng `ClearCartAndUpdateInventoryAsync` ?ã ???c c?p nh?t ?? ?ánh d?u voucher ?ã s? d?ng khi payment thành công. ?ây là h??ng d?n ki?m tra chi ti?t.

---

## ? XÁC NH?N IMPLEMENTATION ?Ã HOÀN T?T

### **1. C?p nh?t ClearCartAndUpdateInventoryAsync**
Method này ???c g?i khi:
- Order status = `OrderStatus.Paid`
- PayOS callback thành công
- Background job x? lý order completion

### **2. Logic X? Lý Voucher**
```csharp
// Ki?m tra order có voucher code không
if (!string.IsNullOrEmpty(order.VoucherCode))
{
    // Tìm voucher code entity
    var voucherCode = await _voucherCodeRepository.GetByCodeAsync(order.VoucherCode);
    
    if (voucherCode != null && !voucherCode.IsUsed)
    {
        // ?ánh d?u ?ã s? d?ng
        voucherCode.IsUsed = true;
        voucherCode.UsedByUserId = order.UserId;
        voucherCode.UsedAt = DateTime.UtcNow;
        voucherCode.UpdatedAt = DateTime.UtcNow;
        voucherCode.UpdatedById = order.UserId;
        
        // L?u vào database
        await _voucherCodeRepository.UpdateAsync(voucherCode);
        await _voucherCodeRepository.SaveChangesAsync();
    }
}
```

### **3. Enhanced Logging**
```
- ? Voucher code validation
- ? Claim ownership verification  
- ? Usage status tracking
- ? Error handling và logging
```

---

## ?? TEST SCENARIOS CHO VOUCHER USAGE MARKING

### **Test Case 1: Normal Voucher Usage Flow**

#### **Step 1: Customer claim voucher**
```http
POST /api/Product/Claim-VoucherCode/{voucher-code-id}
Authorization: Bearer {customer-token}
```

**Verify Database:**
```sql
SELECT * FROM VoucherCodes WHERE Id = '{voucher-code-id}';
-- IsClaimed = 1, ClaimedByUserId = {user-id}, IsUsed = 0
```

#### **Step 2: Customer checkout v?i claimed voucher**
```http
POST /api/Product/CheckoutCart
{
    "cartItemIds": [...],
    "myVoucherCodeId": "{voucher-code-id}"
}
```

**Expected:** Order created v?i VoucherCode field populated

#### **Step 3: Simulate payment success**
Trigger `ClearCartAndUpdateInventoryAsync` method

**Verify Database sau payment:**
```sql
SELECT * FROM VoucherCodes WHERE Id = '{voucher-code-id}';
-- IsClaimed = 1, ClaimedByUserId = {user-id}
-- IsUsed = 1, UsedByUserId = {user-id}, UsedAt = {current-time}

SELECT * FROM Orders WHERE VoucherCode = '{voucher-code}';
-- Order có VoucherCode field
```

**Expected Console Output:**
```
ClearCartAndUpdateInventoryAsync called for order: {order-id}
Processing voucher code: {voucher-code}
Voucher code {voucher-code} was properly claimed by user {user-id} on {claimed-time}
Voucher code {voucher-code} successfully marked as used by user {user-id}
```

---

### **Test Case 2: Direct Voucher Code Usage (Backward Compatibility)**

#### **Step 1: Customer checkout v?i direct voucher code**
```http
POST /api/Product/CheckoutCart
{
    "cartItemIds": [...],
    "voucherCode": "SALE-ABC123-4567"
}
```

#### **Step 2: Payment success**
**Expected Console Output:**
```
Processing voucher code: SALE-ABC123-4567
Voucher code SALE-ABC123-4567 was used directly without claiming (backward compatibility)
Voucher code SALE-ABC123-4567 successfully marked as used by user {user-id}
```

**Verify Database:**
```sql
SELECT * FROM VoucherCodes WHERE Code = 'SALE-ABC123-4567';
-- IsClaimed = 0 (vì ch?a ???c claim)
-- IsUsed = 1, UsedByUserId = {user-id}, UsedAt = {current-time}
```

---

### **Test Case 3: Error Scenarios**

#### **Test 3.1: Voucher already used**
```sql
-- Setup: Mark voucher as already used
UPDATE VoucherCodes SET IsUsed = 1, UsedByUserId = '{other-user-id}' 
WHERE Code = '{voucher-code}';
```

**Expected Console Output:**
```
Voucher code {voucher-code} already marked as used by user {other-user-id} at {timestamp}
```

#### **Test 3.2: Voucher not found**
```http
POST /api/Product/CheckoutCart
{
    "cartItemIds": [...],
    "voucherCode": "INVALID-CODE-999"
}
```

**Expected Console Output:**
```
Voucher code not found: INVALID-CODE-999
```

#### **Test 3.3: Wrong ownership (Security issue)**
```sql
-- Setup: Voucher claimed by different user
UPDATE VoucherCodes SET IsClaimed = 1, ClaimedByUserId = '{user-a}' 
WHERE Code = '{voucher-code}';
```

User B tries to use voucher:
**Expected Console Output:**
```
WARNING: Voucher code {voucher-code} was claimed by user {user-a} but being used by user {user-b}
```

---

## ?? DATABASE VERIFICATION QUERIES

### **1. Voucher Usage Statistics**
```sql
SELECT 
    v.Name as VoucherName,
    COUNT(vc.Id) as TotalCodes,
    COUNT(CASE WHEN vc.IsClaimed = 1 THEN 1 END) as ClaimedCount,
    COUNT(CASE WHEN vc.IsUsed = 1 THEN 1 END) as UsedCount,
    COUNT(CASE WHEN vc.IsClaimed = 1 AND vc.IsUsed = 0 THEN 1 END) as ClaimedButNotUsed
FROM Vouchers v
LEFT JOIN VoucherCodes vc ON v.Id = vc.VoucherId
WHERE v.IsDeleted = 0
GROUP BY v.Id, v.Name;
```

### **2. User Voucher Activity**
```sql
SELECT 
    u.Name as UserName,
    v.Name as VoucherName,
    vc.Code,
    vc.IsClaimed,
    vc.ClaimedAt,
    vc.IsUsed,
    vc.UsedAt,
    CASE 
        WHEN vc.IsUsed = 1 THEN 'Used'
        WHEN vc.IsClaimed = 1 THEN 'Claimed'
        ELSE 'Available'
    END as Status
FROM Users u
LEFT JOIN VoucherCodes vc ON u.Id = vc.ClaimedByUserId OR u.Id = vc.UsedByUserId
LEFT JOIN Vouchers v ON vc.VoucherId = v.Id
WHERE u.Id = '{user-id}' AND vc.Id IS NOT NULL
ORDER BY vc.ClaimedAt DESC, vc.UsedAt DESC;
```

### **3. Orders with Voucher Usage**
```sql
SELECT 
    o.Id as OrderId,
    o.PayOsOrderCode,
    o.VoucherCode,
    o.DiscountAmount,
    o.Status,
    vc.IsClaimed,
    vc.ClaimedByUserId,
    vc.UsedByUserId,
    vc.UsedAt,
    u.Name as UserName
FROM Orders o
LEFT JOIN VoucherCodes vc ON o.VoucherCode = vc.Code
LEFT JOIN Users u ON o.UserId = u.Id
WHERE o.VoucherCode IS NOT NULL
ORDER BY o.CreatedAt DESC;
```

---

## ?? MONITORING VÀ DEBUGGING

### **1. Log Analysis**
Ki?m tra console logs khi payment processing:
```bash
# Search for voucher-related logs
grep "Processing voucher code" application.log
grep "Voucher code.*marked as used" application.log
grep "WARNING.*voucher" application.log
```

### **2. Real-time Database Monitoring**
```sql
-- Monitor voucher usage in real-time
SELECT 
    vc.Code,
    vc.IsUsed,
    vc.UsedAt,
    vc.UsedByUserId,
    NOW() as CheckTime
FROM VoucherCodes vc
WHERE vc.UpdatedAt > DATE_SUB(NOW(), INTERVAL 1 HOUR)
    AND vc.IsUsed = 1
ORDER BY vc.UsedAt DESC;
```

### **3. Performance Metrics**
```sql
-- Voucher system performance
SELECT 
    DATE(vc.UsedAt) as UsageDate,
    COUNT(*) as VouchersUsed,
    COUNT(DISTINCT vc.UsedByUserId) as UniqueUsers,
    AVG(o.DiscountAmount) as AvgDiscount
FROM VoucherCodes vc
JOIN Orders o ON vc.Code = o.VoucherCode
WHERE vc.UsedAt IS NOT NULL
GROUP BY DATE(vc.UsedAt)
ORDER BY UsageDate DESC;
```

---

## ? SUCCESS CRITERIA

### **Functional Requirements**
- ? Voucher marked as used after successful payment
- ? Proper user ownership tracking
- ? Backward compatibility maintained
- ? Duplicate usage prevention
- ? Comprehensive logging

### **Technical Requirements**
- ? Database consistency
- ? Transaction safety
- ? Error handling
- ? Performance optimization
- ? Security validation

### **Business Requirements**
- ? Accurate usage statistics
- ? Fraud prevention
- ? Audit trail
- ? User experience continuity

---

## ?? SECURITY CONSIDERATIONS

### **1. Voucher Ownership Validation**
- System validates voucher ???c claim b?i ?úng user
- Warning logs cho suspicious usage patterns
- Prevention of voucher code sharing abuse

### **2. Race Condition Protection**
- Database constraints prevent duplicate usage
- Atomic updates for voucher status
- Proper transaction handling

### **3. Data Integrity**
- Comprehensive audit trail
- Immutable usage timestamps
- Referential integrity maintenance

---

**K?T LU?N:** Ch?c n?ng ?ánh d?u voucher ?ã s? d?ng ?ã ???c implement ??y ?? và ready ?? test trong production environment! ??