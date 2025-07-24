# ?? DEBUG GUIDE CHO CHECKOUT ISSUE

## ?? **V?N ?? HI?N T?I:**
- URL: `/api/Product/checkout` 
- Error: "The Code field is required"
- Request: `{ "cartItemIds": [...], "voucherCode": null, "myVoucherCodeId": null }`

---

## ?? **TEST ENDPOINTS ?Ã THÊM:**

### **1. Test Endpoint (Debug Raw Request)**
```http
POST /api/Product/test-checkout
Authorization: Bearer {token}
Content-Type: application/json

{
    "cartItemIds": ["test-id"],
    "voucherCode": null,
    "myVoucherCodeId": null
}
```

**M?c ?ích:** Debug raw request ?? xem ModelState validation

### **2. Simple Checkout (Bypass Validation)**
```http
POST /api/Product/simple-checkout
Authorization: Bearer {token}
Content-Type: application/json

{
    "cartItemIds": ["real-cart-item-id"],
    "voucherCode": null,
    "myVoucherCodeId": null
}
```

**M?c ?ích:** Test checkout logic mà không có complex validation

### **3. Original Checkout (Fixed)**
```http
POST /api/Product/checkout
Authorization: Bearer {token}
Content-Type: application/json

{
    "cartItemIds": ["real-cart-item-id"]
}
```

---

## ?? **DEBUGGING STEPS:**

### **Step 1: Test v?i test-checkout**
1. G?i `/api/Product/test-checkout` v?i request c?a b?n
2. Xem response ?? check:
   - ModelState validation errors
   - Raw request data
   - Field mapping issues

### **Step 2: Test v?i simple-checkout**
1. Thay th? real cart item ID
2. G?i `/api/Product/simple-checkout`
3. Xem có bypass ???c validation issue không

### **Step 3: So sánh results**
- N?u `simple-checkout` work ? issue ? validation
- N?u c? 2 ??u fail ? issue ? business logic
- N?u `test-checkout` show l?i khác ? issue ? model binding

---

## ?? **POSSIBLE ROOT CAUSES:**

### **1. Model Binding Issue**
```csharp
// Có th? có attribute validation ?n ? ?âu ?ó
[Required] 
public string Code { get; set; } // ? Field này ko nên có!
```

### **2. Route Conflict** 
```csharp
// Có th? có controller khác v?i endpoint t??ng t?
[HttpPost("checkout")]
public async Task<IActionResult> AnotherCheckout([FromBody] SomeOtherDto dto)
```

### **3. Model Validation Pipeline**
```csharp
// Global validation filters có th? ?ang check field "Code"
```

### **4. JSON Serialization Issue**
```json
// Request có th? b? serialize sai format
{
    "cartItemIds": [...],
    "voucherCode": null, // ? này có th? b? interpret as required
    "myVoucherCodeId": null
}
```

---

## ?? **WORKAROUND SOLUTIONS:**

### **Solution 1: S? d?ng simple-checkout**
T?m th?i dùng endpoint này thay vì checkout chính:
```http
POST /api/Product/simple-checkout
```

### **Solution 2: B? null fields**
```json
{
    "cartItemIds": ["cart-item-id"]
    // Không truy?n voucherCode và myVoucherCodeId
}
```

### **Solution 3: Explicit empty values**
```json
{
    "cartItemIds": ["cart-item-id"],
    "voucherCode": "",
    "myVoucherCodeId": "00000000-0000-0000-0000-000000000000"
}
```

---

## ?? **TESTING MATRIX:**

| Test Case | Endpoint | VoucherCode | MyVoucherCodeId | Expected Result |
|-----------|----------|-------------|-----------------|-----------------|
| 1 | test-checkout | null | null | Debug info |
| 2 | test-checkout | "" | null | Debug info |
| 3 | test-checkout | omitted | omitted | Debug info |
| 4 | simple-checkout | null | null | Success/Clear error |
| 5 | simple-checkout | omitted | omitted | Success/Clear error |
| 6 | checkout | null | null | Should work |
| 7 | checkout | omitted | omitted | Should work |

---

## ?? **ACTION PLAN:**

### **Immediate (Test now):**
1. G?i `test-checkout` v?i request hi?n t?i
2. Xem debug output trong response
3. Identify exact validation error

### **Short-term (If debug shows issue):**
1. S? d?ng `simple-checkout` ?? test business logic
2. Fix validation issue based on debug info
3. Update original `checkout` endpoint

### **Long-term (Prevention):**
1. Add comprehensive unit tests cho validation
2. Add integration tests cho checkout flow
3. Document proper request formats

---

## ?? **DEBUG COMMANDS:**

### **cURL Examples:**
```bash
# Test endpoint
curl -X POST "https://localhost:7268/api/Product/test-checkout" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "cartItemIds": ["test-id"],
    "voucherCode": null,
    "myVoucherCodeId": null
  }'

# Simple checkout
curl -X POST "https://localhost:7268/api/Product/simple-checkout" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "cartItemIds": ["real-cart-item-id"]
  }'
```

Hãy test các endpoints này ?? tìm ra root cause! ??????