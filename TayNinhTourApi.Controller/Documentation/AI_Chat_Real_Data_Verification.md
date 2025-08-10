# H??ng D?n Ki?m Tra AI Chat System - S? D?ng D? Li?u Th?c

## T�m T?t Thay ??i
?� c?i thi?n h? th?ng AI Chat ?? ??m b?o **ch? s? d?ng d? li?u th?c t? t? database**, kh�ng t?o ra d? li?u gi?.

## C�ch Ki?m Tra

### 1. Ki?m Tra K?t N?i Database v� D? Li?u Th?c
```http
GET /api/AiChat/debug/products
Authorization: Bearer {your-jwt-token}
```

**Response mong ??i:**
- `success: true` 
- `authenticatedUser`: th�ng tin user ???c authenticate
- `data.availableProducts`: danh s�ch s?n ph?m th?c t? database
- `data.connectionInfo`: th�ng tin k?t n?i TayNinhTourDb
- `verification.realDataConfirmed: true`

### 2. Test AI Chat v?i D? Li?u Th?c
```http
POST /api/AiChat/debug/test-product-chat
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "testMessage": "T�i mu?n mua b�nh tr�ng T�y Ninh"
}
```

**Response mong ??i:**
- T?o session Product Chat th�nh c�ng
- AI s? ch? t? v?n s?n ph?m c� trong database
- Kh�ng t?o ra th�ng tin s?n ph?m gi?

### 3. Test Chat Th�ng Th??ng
```http
POST /api/AiChat/sessions
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "chatType": 2,
  "firstMessage": "C� nh?ng s?n ph?m g� ?ang b�n?"
}
```

## ?i?m Kh�c Bi?t Tr??c v� Sau

### ? Tr??c khi s?a:
- AI c� th? t?o ra th�ng tin s?n ph?m gi?
- Fallback responses c� d? li?u hardcode
- Kh�ng ki?m so�t ch?t ch? ngu?n d? li?u

### ? Sau khi s?a:
- AI **CH?** s? d?ng d? li?u t? database th?c t?
- System prompt nghi�m c?m t?o d? li?u gi?  
- Fallback s? d?ng database queries thay v� d? li?u c?ng
- Logging chi ti?t ?? theo d�i

## Monitoring v� Logs

Ki?m tra logs ?? x�c nh?n:
```
"Retrieved X real products from database for AI context"
"SUCCESS: Found X REAL products from database (not fake data)"
"Sample product names from DB: ..."
```

## L?u � Quan Tr?ng

1. **Authentication Required**: T?t c? debug endpoints y�u c?u JWT token h?p l?
2. **Database Connection**: ??m b?o connection string trong appsettings.json ch�nh x�c
3. **Product Data**: C?n c� s?n ph?m v?i `IsActive=true`, `IsDeleted=false`, `QuantityInStock>0`

## K?t Qu? Mong ??i

- ? AI ch? t? v?n s?n ph?m c� th?t trong database
- ? Kh�ng t?o ra gi� c?, t�n shop, th�ng tin s?n ph?m gi?
- ? Khi kh�ng c� d? li?u ? th�ng b�o th�nh th?t thay v� b?a ??t
- ? C� th? trace ???c qua logs ?? verify

---
**C?p nh?t:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}