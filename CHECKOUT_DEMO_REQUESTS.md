# ?? DEMO REQUESTS CHO CHECKOUT (C� V� KH�NG VOUCHER)

## ?? **CHECKOUT KH�NG S? D?NG VOUCHER**

### **Request 1: Ch? c� cartItemIds (Recommended)**
```json
{
    "cartItemIds": [
        "c3b5c3c1-4d6a-4b2f-8f9e-1a2b3c4d5e6f"
    ]
}
```

### **Request 2: Explicit null values**
```json
{
    "cartItemIds": [
        "c3b5c3c1-4d6a-4b2f-8f9e-1a2b3c4d5e6f"
    ],
    "voucherCode": null,
    "myVoucherCodeId": null
}
```

### **Request 3: Empty string cho voucherCode**
```json
{
    "cartItemIds": [
        "c3b5c3c1-4d6a-4b2f-8f9e-1a2b3c4d5e6f"
    ],
    "voucherCode": "",
    "myVoucherCodeId": null
}
```

---

## ?? **CHECKOUT V?I VOUCHER**

### **Option A: S? d?ng voucher t? kho c� nh�n**
```json
{
    "cartItemIds": [
        "c3b5c3c1-4d6a-4b2f-8f9e-1a2b3c4d5e6f"
    ],
    "myVoucherCodeId": "a1b2c3d4-5e6f-7g8h-9i0j-k1l2m3n4o5p6"
}
```

### **Option B: S? d?ng voucher code tr?c ti?p**
```json
{
    "cartItemIds": [
        "c3b5c3c1-4d6a-4b2f-8f9e-1a2b3c4d5e6f"
    ],
    "voucherCode": "TETN-ABC1-2345"
}
```

### **Option C: C? hai (system ?u ti�n myVoucherCodeId)**
```json
{
    "cartItemIds": [
        "c3b5c3c1-4d6a-4b2f-8f9e-1a2b3c4d5e6f"
    ],
    "voucherCode": "BACKUP-CODE-123",
    "myVoucherCodeId": "a1b2c3d4-5e6f-7g8h-9i0j-k1l2m3n4o5p6"
}
```

---

## ? **EXPECTED RESPONSES**

### **Th�nh c�ng kh�ng voucher:**
```json
{
    "checkoutUrl": "https://payos.com/checkout/...",
    "orderId": "order-guid-here",
    "totalOriginal": 500000,
    "discountAmount": 0,
    "totalAfterDiscount": 500000
}
```

### **Th�nh c�ng c� voucher:**
```json
{
    "checkoutUrl": "https://payos.com/checkout/...",
    "orderId": "order-guid-here", 
    "totalOriginal": 500000,
    "discountAmount": 125000,
    "totalAfterDiscount": 375000
}
```

### **L?i validation:**
```json
{
    "message": "D? li?u kh�ng h?p l?",
    "errors": [
        "Danh s�ch s?n ph?m kh�ng ???c ?? tr?ng"
    ]
}
```

---

## ?? **LOGIC X? L� TRONG H? TH?NG**

### **1. Validation Order:**
```csharp
// 1. Validate CartItemIds (Required)
if (!cartItemIds.Any()) -> Error

// 2. Process Voucher (Optional)
if (myVoucherCodeId.HasValue) -> Use claimed voucher
else if (!string.IsNullOrEmpty(voucherCode)) -> Use direct code  
else -> No voucher (OK)

// 3. Calculate totals
totalAfterDiscount = total - discountAmount
```

### **2. Voucher Processing Logic:**
```csharp
// ?u ti�n myVoucherCodeId
if (myVoucherCodeId.HasValue)
{
    var result = await ApplyMyVoucherForCartAsync(myVoucherCodeId.Value, ...);
    // Apply discount t? kho c� nh�n
}
else if (!string.IsNullOrEmpty(voucherCode))
{
    var result = await ApplyVoucherForCartAsync(voucherCode, ...);
    // Apply discount tr?c ti?p
}
// N?u kh�ng c� voucher -> ti?p t?c v?i gi� g?c
```

---

## ?? **TEST CASES**

### **Test 1: Checkout th�nh c�ng kh�ng voucher**
```bash
curl -X POST "https://api.domain.com/api/Product/checkout" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "cartItemIds": ["cart-item-id-here"]
  }'
```

**Expected:** Status 200, kh�ng c� discount

### **Test 2: Checkout v?i invalid cartItemIds**
```json
{
    "cartItemIds": []
}
```

**Expected:** Status 400, error message v? cartItemIds

### **Test 3: Checkout v?i voucher kh�ng t?n t?i**
```json
{
    "cartItemIds": ["valid-cart-item-id"],
    "voucherCode": "INVALID-CODE-999"
}
```

**Expected:** Status 400, error v? voucher kh�ng t?n t?i

### **Test 4: Checkout v?i voucher ?� s? d?ng**
```json
{
    "cartItemIds": ["valid-cart-item-id"],
    "myVoucherCodeId": "used-voucher-id"
}
```

**Expected:** Status 400, error v? voucher ?� s? d?ng

---

## ?? **RECOMMENDATION**

### **C�ch t?t nh?t ?? test:**

**1. Kh�ng voucher (Simplest):**
```json
{
    "cartItemIds": ["your-cart-item-id"]
}
```

**2. C� voucher (t? kho c� nh�n):**
```json
{
    "cartItemIds": ["your-cart-item-id"],
    "myVoucherCodeId": "your-claimed-voucher-id"
}
```

**3. Voucher code tr?c ti?p:**
```json
{
    "cartItemIds": ["your-cart-item-id"],
    "voucherCode": "DIRECT-CODE-123"
}
```

---

## ?? **L?U � QUAN TR?NG**

1. **cartItemIds** l� field duy nh?t b?t bu?c
2. **voucherCode** v� **myVoucherCodeId** ??u optional
3. N?u c? hai voucher fields ??u null/empty -> checkout b�nh th??ng
4. System s? validate voucher ch? khi c� value
5. Priority: myVoucherCodeId > voucherCode

B�y gi? checkout kh�ng voucher s? ho?t ??ng b�nh th??ng! ??