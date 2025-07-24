# ?? VOUCHER SYSTEM V2 - CH? S? D?NG VOUCHER ?Ã CLAIM

## ?? **OVERVIEW**
H? th?ng voucher ?ã ???c c?p nh?t ?? **lo?i b? kh? n?ng nh?p mã voucher tr?c ti?p** khi checkout. User ch? có th? s? d?ng voucher t? kho cá nhân ?ã claim tr??c ?ó.

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

### **3. User xem voucher có s?n**
```http
GET /api/Product/GetAvailable-VoucherCodes
```

### **4. User claim voucher**
```http
POST /api/Product/Claim-VoucherCode/{voucherCodeId}
```

### **5. User checkout v?i voucher ?ã claim**
```http
POST /api/Product/checkout
{
    "cartItemIds": ["cart-item-id"],
    "myVoucherCodeId": "claimed-voucher-id"  // Ch? field này
}
```

---

## ?? **NEW CHECKOUT FORMAT**

### **? Checkout không voucher:**
```json
{
    "cartItemIds": [
        "550e8400-e29b-41d4-a716-446655440000"
    ]
}
```

### **? Checkout v?i voucher ?ã claim:**
```json
{
    "cartItemIds": [
        "550e8400-e29b-41d4-a716-446655440000"
    ],
    "myVoucherCodeId": "a1b2c3d4-5e6f-7g8h-9i0j-k1l2m3n4o5p6"
}
```

### **? KHÔNG ???C PHÉP (?ã lo?i b?):**
```json
{
    "cartItemIds": [...],
    "voucherCode": "BLAC-A7B2-1234"  // ? Field này ?ã b? xóa
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
    
    // ? ?ã xóa: public string? VoucherCode { get; set; }
    
    /// <summary>
    /// Ch? có th? s? d?ng voucher ?ã claim trong kho cá nhân
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
- ? Không th? s? d?ng voucher "?n c?p" t? ng??i khác
- ? T?t c? voucher ph?i ???c claim chính th?c
- ? Audit trail ??y ?? cho vi?c s? d?ng voucher

### **2. User Experience**
- ? Clear ownership: "Voucher c?a tôi"
- ? Không c?n nh?/nh?p mã code ph?c t?p
- ? Interface ??n gi?n h?n

### **3. Business Logic**
- ? Prevent voucher sharing/leaking
- ? Better tracking và analytics
- ? Consistent v?i claim system

---

## ?? **TESTING SCENARIOS**

### **Test Case 1: Checkout thành công không voucher**
```http
POST /api/Product/checkout
{
    "cartItemIds": ["valid-cart-item-id"]
}
```
**Expected:** Status 200, `discountAmount: 0`

### **Test Case 2: Checkout v?i voucher h?p l?**
**Setup:** User ?ã claim voucher
```http
POST /api/Product/checkout
{
    "cartItemIds": ["valid-cart-item-id"],
    "myVoucherCodeId": "user-claimed-voucher-id"
}
```
**Expected:** Status 200, có discount

### **Test Case 3: Voucher không thu?c user**
```http
POST /api/Product/checkout
{
    "cartItemIds": ["valid-cart-item-id"],
    "myVoucherCodeId": "other-user-voucher-id"
}
```
**Expected:** Status 400, "Không tìm th?y mã voucher trong kho voucher c?a b?n"

### **Test Case 4: Voucher ?ã s? d?ng**
```http
POST /api/Product/checkout
{
    "cartItemIds": ["valid-cart-item-id"],
    "myVoucherCodeId": "used-voucher-id"
}
```
**Expected:** Status 400, "Mã voucher này ?ã ???c s? d?ng"

---

## ?? **API DOCUMENTATION**

### **GET /api/Product/My-Vouchers**
L?y danh sách voucher trong kho cá nhân:
```json
{
    "data": [
        {
            "voucherCodeId": "guid-here",
            "code": "BLAC-A7B2-1234",
            "voucherName": "Black Friday 2024",
            "discountPercent": 25,
            "status": "Có th? s? d?ng",
            "isActive": true
        }
    ]
}
```

### **POST /api/Product/checkout**
Checkout v?i voucher t? kho cá nhân:
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
- User ch? có th? dùng voucher ?ã claim
- Checkout workflow ??n gi?n và secure
- Better ownership tracking
- Prevent voucher abuse

### **?? Next Steps:**
1. Update frontend ?? remove voucher code input
2. Enhance My-Vouchers UI ?? d? select
3. Add voucher preview trong checkout flow
4. Monitor usage analytics

**H? th?ng voucher gi? ?ây an toàn và user-friendly h?n!** ??