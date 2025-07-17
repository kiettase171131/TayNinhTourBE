# API H??ng D?n Check ??n Hàng - Specialty Shop

## ?? M?c ?ích
H? th?ng cho phép shop ki?m tra và xác nh?n giao hàng ??n hàng b?ng PayOS Order Code.

## ?? Xác th?c
**T?t c? API c?n JWT token v?i role "Specialty Shop"**
```
Header: Authorization: Bearer {jwt_token}
```

---

## ?? Danh sách API

### 1. Check và Giao ??n Hàng
**API chính ?? xác nh?n giao hàng**

```
POST /api/Order/check
```

**Truy?n vào:**
```json
{
    "payOsOrderCode": "TNDT1234567890"
}
```

**Nh?n v? khi thành công:**
```json
{
    "statusCode": 200,
    "message": "Check ??n hàng thành công - ?ã giao hàng cho khách",
    "success": true,
    "isProcessed": true,
    "wasAlreadyChecked": false,
    "checkedAt": "2025-01-17T10:30:00Z",
    "checkedByShopName": "Tên Shop",
    "customer": {
        "name": "Tên khách hàng",
        "email": "email@example.com",
        "phoneNumber": "0987654321"
    },
    "products": [
        {
            "productName": "Tên s?n ph?m",
            "quantity": 2,
            "unitPrice": 75000
        }
    ]
}
```

**Các l?i có th? g?p:**
- `404`: Không tìm th?y ??n hàng
- `403`: Shop không có quy?n (không bán s?n ph?m trong ??n hàng)
- `400`: ??n hàng ch?a thanh toán
- `409`: ??n hàng ?ã ???c check r?i

---

### 2. L?y Danh Sách ??n Hàng Có Th? Check

```
GET /api/Order/checkable
```

**Tham s? tùy ch?n:**
- `page`: Trang hi?n t?i (m?c ??nh: 1)
- `pageSize`: S? ??n hàng m?i trang (m?c ??nh: 10)
- `isChecked`: L?c theo tr?ng thái check (true/false)

**Ví d?:** `/api/Order/checkable?page=1&pageSize=10&isChecked=false`

**Nh?n v?:**
```json
{
    "statusCode": 200,
    "success": true,
    "orders": [
        {
            "payOsOrderCode": "TNDT1234567890",
            "totalAfterDiscount": 150000,
            "status": 1,
            "isChecked": false,
            "createdAt": "2025-01-17T09:00:00Z",
            "orderDetails": [
                {
                    "productName": "S?n ph?m A",
                    "quantity": 2,
                    "unitPrice": 75000
                }
            ]
        }
    ],
    "totalCount": 5
}
```

---

### 3. Xem Chi Ti?t ??n Hàng

```
GET /api/Order/details/{payOsOrderCode}
```

**Ví d?:** `/api/Order/details/TNDT1234567890`

**Nh?n v?:**
```json
{
    "statusCode": 200,
    "success": true,
    "payOsOrderCode": "TNDT1234567890",
    "isChecked": false,
    "checkedAt": null,
    "checkedByShopName": null,
    "order": {
        "payOsOrderCode": "TNDT1234567890",
        "totalAfterDiscount": 150000,
        "status": 1,
        "orderDetails": [...]
    }
}
```

---

### 4. Ki?m Tra Thông Tin Shop (Debug)

```
GET /api/Order/debug/shop-info
```

**Nh?n v?:**
```json
{
    "currentUser": {
        "id": "guid-here",
        "name": "Tên shop",
        "email": "shop@example.com"
    },
    "specialtyShop": {
        "shopName": "Tên shop",
        "isActive": true,
        "isShopActive": true,
        "location": "??a ch? shop"
    },
    "message": "Shop found successfully"
}
```

---

### 5. Ki?m Tra Quy?n S? H?u ??n Hàng (Debug)

```
GET /api/Order/debug/order-ownership/{payOsOrderCode}
```

**Ví d?:** `/api/Order/debug/order-ownership/TNDT1234567890`

**Nh?n v?:**
```json
{
    "currentShop": {
        "shopName": "Tên shop"
    },
    "order": {
        "payOsOrderCode": "TNDT1234567890",
        "status": 1,
        "isChecked": false
    },
    "validation": {
        "hasAnyOwnedProducts": true,
        "canCheckOrder": true,
        "message": "Shop có quy?n check ??n hàng này"
    }
}
```

---

## ?? PayOS Order Code

**Format:** `TNDT` + 10 ch? s? (ví d?: `TNDT1234567890`)
- Luôn vi?t HOA
- B?t ??u b?ng "TNDT"
- Theo sau là 10 ch? s?

---

## ?? Tr?ng Thái ??n Hàng

| Status Code | Ý ngh?a |
|-------------|---------|
| 0 | Ch?a thanh toán |
| 1 | ?ã thanh toán |

**Ch? ??n hàng có `status = 1` m?i có th? check ???c**

---

## ?? Quy Trình Ho?t ??ng

1. **Khách hàng** mua s?n ph?m ? Thanh toán ? Nh?n PayOS Order Code
2. **Khách hàng** ??n shop v?i PayOS Order Code
3. **Shop** nh?p code vào app ? G?i API check
4. **H? th?ng** ki?m tra:
   - Shop có bán s?n ph?m trong ??n hàng không?
   - ??n hàng ?ã thanh toán ch?a?
   - ??n hàng ?ã check ch?a?
5. **N?u h?p l?** ? ?ánh d?u ?ã giao hàng

---

## ?? L?u Ý Quan Tr?ng

1. **M?i ??n hàng ch? check ???c 1 l?n**
2. **Ch? shop bán s?n ph?m m?i check ???c**
3. **??n hàng ph?i ?ã thanh toán**
4. **JWT token ph?i có role "Specialty Shop"**

---

## ?? D? Li?u Test

**Account test:**
- Email: `specialtyshop1@example.com`
- Password: `12345678h@`

**PayOS Code m?u:**
- `TNDT1234567890` - ??n hàng h?p l?, ch?a check
- `TNDT9876543210` - ??n hàng ?ã check r?i
- `TNDT5555555555` - ??n hàng ch?a thanh toán