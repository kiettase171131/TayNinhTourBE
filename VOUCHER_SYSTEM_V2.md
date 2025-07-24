# ?? VOUCHER SYSTEM V2 - CH? S? D?NG VOUCHER ?� CLAIM

## ?? **OVERVIEW**
H? th?ng voucher ?� ???c c?p nh?t ?? **lo?i b? kh? n?ng nh?p m� voucher tr?c ti?p** khi checkout. User ch? c� th? s? d?ng voucher t? kho c� nh�n ?� claim tr??c ?�.

---

## ?? **VOUCHER WORKFLOW M?I**

### **1. Admin t?o voucher**
```http
POST /api/Product/Create-Voucher
{
    "name": "Black Friday 2024",
    "quantity": 100,
    "discountPercent": 25,
    "startDate": "2024-11-25T00:00:00Z",
    "endDate": "2024-11-30T23:59:59Z"
}
```

### **2. H? th?ng auto-generate codes**
```
? BLAC-A7B2-1234
? BLAC-C9D4-5678
? BLAC-E2F6-9012
... (97 codes n?a)
```

### **3. User xem voucher c� s?n**
```http
GET /api/Product/GetAvailable-VoucherCodes
```

### **4. User claim voucher**
```http
POST /api/Product/Claim-VoucherCode/{voucherCodeId}
```

### **5. User checkout v?i voucher ?� claim**
```http
POST /api/Product/checkout
{
    "cartItemIds": ["cart-item-id"],
    "myVoucherCodeId": "claimed-voucher-id"  // Ch? field n�y
}
```

---

## ?? **NEW CHECKOUT FORMAT**

### **? Checkout kh�ng voucher:**
```json
{
    "cartItemIds": [
        "550e8400-e29b-41d4-a716-446655440000"
    ]
}
```

### **? Checkout v?i voucher ?� claim:**
```json
{
    "cartItemIds": [
        "550e8400-e29b-41d4-a716-446655440000"
    ],
    "myVoucherCodeId": "a1b2c3d4-5e6f-7g8h-9i0j-k1l2m3n4o5p6"
}
```

### **? KH�NG ???C PH�P (?� lo?i b?):**
```json
{
    "cartItemIds": [...],
    "voucherCode": "BLAC-A7B2-1234"  // ? Field n�y ?� b? x�a
}
```

---

## ?? **TECHNICAL CHANGES**

### **1. CheckoutSelectedCartItemsDto**
```csharp
public class CheckoutSelectedCartItemsDto
{
    [Required]
    public List<Guid> CartItemIds { get; set; }
    
    // ? ?� x�a: public string? VoucherCode { get; set; }
    
    /// <summary>
    /// Ch? c� th? s? d?ng voucher ?� claim trong kho c� nh�n
    /// </summary>
    public Guid? MyVoucherCodeId { get; set; }
}
```

### **2. ProductController.Checkout**
```csharp
// OLD: CheckoutCartAsync(dto.CartItemIds, currentUser, dto.VoucherCode, dto.MyVoucherCodeId)
// NEW: CheckoutCartAsync(dto.CartItemIds, currentUser, dto.MyVoucherCodeId)
```

### **3. IProductService.CheckoutCartAsync**
```csharp
// OLD: Task<CheckoutResultDto?> CheckoutCartAsync(List<Guid> cartItemIds, CurrentUserObject currentUser, string? voucherCode = null, Guid? myVoucherCodeId = null);
// NEW: Task<CheckoutResultDto?> CheckoutCartAsync(List<Guid> cartItemIds, CurrentUserObject currentUser, Guid? myVoucherCodeId = null);
```

---

## ?? **BUSINESS BENEFITS**

### **1. Security Enhancement**
- ? Kh�ng th? s? d?ng voucher "?n c?p" t? ng??i kh�c
- ? T?t c? voucher ph?i ???c claim ch�nh th?c
- ? Audit trail ??y ?? cho vi?c s? d?ng voucher

### **2. User Experience**
- ? Clear ownership: "Voucher c?a t�i"
- ? Kh�ng c?n nh?/nh?p m� code ph?c t?p
- ? Interface ??n gi?n h?n

### **3. Business Logic**
- ? Prevent voucher sharing/leaking
- ? Better tracking v� analytics
- ? Consistent v?i claim system

---

## ?? **TESTING SCENARIOS**

### **Test Case 1: Checkout th�nh c�ng kh�ng voucher**
```http
POST /api/Product/checkout
{
    "cartItemIds": ["valid-cart-item-id"]
}
```
**Expected:** Status 200, `discountAmount: 0`

### **Test Case 2: Checkout v?i voucher h?p l?**
**Setup:** User ?� claim voucher
```http
POST /api/Product/checkout
{
    "cartItemIds": ["valid-cart-item-id"],
    "myVoucherCodeId": "user-claimed-voucher-id"
}
```
**Expected:** Status 200, c� discount

### **Test Case 3: Voucher kh�ng thu?c user**
```http
POST /api/Product/checkout
{
    "cartItemIds": ["valid-cart-item-id"],
    "myVoucherCodeId": "other-user-voucher-id"
}
```
**Expected:** Status 400, "Kh�ng t�m th?y m� voucher trong kho voucher c?a b?n"

### **Test Case 4: Voucher ?� s? d?ng**
```http
POST /api/Product/checkout
{
    "cartItemIds": ["valid-cart-item-id"],
    "myVoucherCodeId": "used-voucher-id"
}
```
**Expected:** Status 400, "M� voucher n�y ?� ???c s? d?ng"

---

## ?? **API DOCUMENTATION**

### **GET /api/Product/My-Vouchers**
L?y danh s�ch voucher trong kho c� nh�n:
```json
{
    "data": [
        {
            "voucherCodeId": "guid-here",
            "code": "BLAC-A7B2-1234",
            "voucherName": "Black Friday 2024",
            "discountPercent": 25,
            "status": "C� th? s? d?ng",
            "isActive": true
        }
    ]
}
```

### **POST /api/Product/checkout**
Checkout v?i voucher t? kho c� nh�n:
```json
// Request
{
    "cartItemIds": ["cart-item-id"],
    "myVoucherCodeId": "voucher-code-id"  // Optional
}

// Response
{
    "checkoutUrl": "https://pay.payos.vn/...",
    "orderId": "order-guid",
    "totalOriginal": 500000,
    "discountAmount": 125000,
    "totalAfterDiscount": 375000
}
```

---

## ?? **SUMMARY**

### **? What's Working:**
- User ch? c� th? d�ng voucher ?� claim
- Checkout workflow ??n gi?n v� secure
- Better ownership tracking
- Prevent voucher abuse

### **?? Next Steps:**
1. Update frontend ?? remove voucher code input
2. Enhance My-Vouchers UI ?? d? select
3. Add voucher preview trong checkout flow
4. Monitor usage analytics

**H? th?ng voucher gi? ?�y an to�n v� user-friendly h?n!** ??