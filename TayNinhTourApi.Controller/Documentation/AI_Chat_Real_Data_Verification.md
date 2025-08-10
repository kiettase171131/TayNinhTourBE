# H??ng D?n Ki?m Tra AI Chat System - S? D?ng D? Li?u Th?c

## Tóm T?t Thay ??i
?ã c?i thi?n h? th?ng AI Chat ?? ??m b?o **ch? s? d?ng d? li?u th?c t? t? database**, không t?o ra d? li?u gi?.

## Cách Ki?m Tra

### 1. Ki?m Tra K?t N?i Database và D? Li?u Th?c
```http
GET /api/AiChat/debug/products
Authorization: Bearer {your-jwt-token}
```

**Response mong ??i:**
- `success: true` 
- `authenticatedUser`: thông tin user ???c authenticate
- `data.availableProducts`: danh sách s?n ph?m th?c t? database
- `data.connectionInfo`: thông tin k?t n?i TayNinhTourDb
- `verification.realDataConfirmed: true`

### 2. Test AI Chat v?i D? Li?u Th?c
```http
POST /api/AiChat/debug/test-product-chat
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "testMessage": "Tôi mu?n mua bánh tráng Tây Ninh"
}
```

**Response mong ??i:**
- T?o session Product Chat thành công
- AI s? ch? t? v?n s?n ph?m có trong database
- Không t?o ra thông tin s?n ph?m gi?

### 3. Test Chat Thông Th??ng
```http
POST /api/AiChat/sessions
Authorization: Bearer {your-jwt-token}
Content-Type: application/json

{
  "chatType": 2,
  "firstMessage": "Có nh?ng s?n ph?m gì ?ang bán?"
}
```

## ?i?m Khác Bi?t Tr??c và Sau

### ? Tr??c khi s?a:
- AI có th? t?o ra thông tin s?n ph?m gi?
- Fallback responses có d? li?u hardcode
- Không ki?m soát ch?t ch? ngu?n d? li?u

### ? Sau khi s?a:
- AI **CH?** s? d?ng d? li?u t? database th?c t?
- System prompt nghiêm c?m t?o d? li?u gi?  
- Fallback s? d?ng database queries thay vì d? li?u c?ng
- Logging chi ti?t ?? theo dõi

## Monitoring và Logs

Ki?m tra logs ?? xác nh?n:
```
"Retrieved X real products from database for AI context"
"SUCCESS: Found X REAL products from database (not fake data)"
"Sample product names from DB: ..."
```

## L?u Ý Quan Tr?ng

1. **Authentication Required**: T?t c? debug endpoints yêu c?u JWT token h?p l?
2. **Database Connection**: ??m b?o connection string trong appsettings.json chính xác
3. **Product Data**: C?n có s?n ph?m v?i `IsActive=true`, `IsDeleted=false`, `QuantityInStock>0`

## K?t Qu? Mong ??i

- ? AI ch? t? v?n s?n ph?m có th?t trong database
- ? Không t?o ra giá c?, tên shop, thông tin s?n ph?m gi?
- ? Khi không có d? li?u ? thông báo thành th?t thay vì b?a ??t
- ? Có th? trace ???c qua logs ?? verify

---
**C?p nh?t:** {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}