# API H??ng D?n Check ??n H�ng - Specialty Shop

## ?? M?c ?�ch
H? th?ng cho ph�p shop ki?m tra v� x�c nh?n giao h�ng ??n h�ng b?ng PayOS Order Code.

## ?? X�c th?c
**T?t c? API c?n JWT token v?i role "Specialty Shop"**
```
Header: Authorization: Bearer {jwt_token}
```

---

## ?? Danh s�ch API

### 1. Check v� Giao ??n H�ng
**API ch�nh ?? x�c nh?n giao h�ng**

```
POST /api/Order/check
```

**Truy?n v�o:**
```json
{
    "payOsOrderCode": "TNDT1234567890"
}
```

**Nh?n v? khi th�nh c�ng:**
```json
{
    "statusCode": 200,
    "message": "Check ??n h�ng th�nh c�ng - ?� giao h�ng cho kh�ch",
    "success": true,
    "isProcessed": true,
    "wasAlreadyChecked": false,
    "checkedAt": "2025-01-17T10:30:00Z",
    "checkedByShopName": "T�n Shop",
    "customer": {
        "name": "T�n kh�ch h�ng",
        "email": "email@example.com",
        "phoneNumber": "0987654321"
    },
    "products": [
        {
            "productName": "T�n s?n ph?m",
            "quantity": 2,
            "unitPrice": 75000
        }
    ]
}
```

**C�c l?i c� th? g?p:**
- `404`: Kh�ng t�m th?y ??n h�ng
- `403`: Shop kh�ng c� quy?n (kh�ng b�n s?n ph?m trong ??n h�ng)
- `400`: ??n h�ng ch?a thanh to�n
- `409`: ??n h�ng ?� ???c check r?i

---

### 2. L?y Danh S�ch ??n H�ng C� Th? Check

```
GET /api/Order/checkable
```

**Tham s? t�y ch?n:**
- `page`: Trang hi?n t?i (m?c ??nh: 1)
- `pageSize`: S? ??n h�ng m?i trang (m?c ??nh: 10)
- `isChecked`: L?c theo tr?ng th�i check (true/false)

**V� d?:** `/api/Order/checkable?page=1&pageSize=10&isChecked=false`

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

### 3. Xem Chi Ti?t ??n H�ng

```
GET /api/Order/details/{payOsOrderCode}
```

**V� d?:** `/api/Order/details/TNDT1234567890`

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

### 4. Ki?m Tra Th�ng Tin Shop (Debug)

```
GET /api/Order/debug/shop-info
```

**Nh?n v?:**
```json
{
    "currentUser": {
        "id": "guid-here",
        "name": "T�n shop",
        "email": "shop@example.com"
    },
    "specialtyShop": {
        "shopName": "T�n shop",
        "isActive": true,
        "isShopActive": true,
        "location": "??a ch? shop"
    },
    "message": "Shop found successfully"
}
```

---

### 5. Ki?m Tra Quy?n S? H?u ??n H�ng (Debug)

```
GET /api/Order/debug/order-ownership/{payOsOrderCode}
```

**V� d?:** `/api/Order/debug/order-ownership/TNDT1234567890`

**Nh?n v?:**
```json
{
    "currentShop": {
        "shopName": "T�n shop"
    },
    "order": {
        "payOsOrderCode": "TNDT1234567890",
        "status": 1,
        "isChecked": false
    },
    "validation": {
        "hasAnyOwnedProducts": true,
        "canCheckOrder": true,
        "message": "Shop c� quy?n check ??n h�ng n�y"
    }
}
```

---

## ?? PayOS Order Code

**Format:** `TNDT` + 10 ch? s? (v� d?: `TNDT1234567890`)
- Lu�n vi?t HOA
- B?t ??u b?ng "TNDT"
- Theo sau l� 10 ch? s?

---

## ?? Tr?ng Th�i ??n H�ng

| Status Code | � ngh?a |
|-------------|---------|
| 0 | Ch?a thanh to�n |
| 1 | ?� thanh to�n |

**Ch? ??n h�ng c� `status = 1` m?i c� th? check ???c**

---

## ?? Quy Tr�nh Ho?t ??ng

1. **Kh�ch h�ng** mua s?n ph?m ? Thanh to�n ? Nh?n PayOS Order Code
2. **Kh�ch h�ng** ??n shop v?i PayOS Order Code
3. **Shop** nh?p code v�o app ? G?i API check
4. **H? th?ng** ki?m tra:
   - Shop c� b�n s?n ph?m trong ??n h�ng kh�ng?
   - ??n h�ng ?� thanh to�n ch?a?
   - ??n h�ng ?� check ch?a?
5. **N?u h?p l?** ? ?�nh d?u ?� giao h�ng

---

## ?? L?u � Quan Tr?ng

1. **M?i ??n h�ng ch? check ???c 1 l?n**
2. **Ch? shop b�n s?n ph?m m?i check ???c**
3. **??n h�ng ph?i ?� thanh to�n**
4. **JWT token ph?i c� role "Specialty Shop"**

---

## ?? D? Li?u Test

**Account test:**
- Email: `specialtyshop1@example.com`
- Password: `12345678h@`

**PayOS Code m?u:**
- `TNDT1234567890` - ??n h�ng h?p l?, ch?a check
- `TNDT9876543210` - ??n h�ng ?� check r?i
- `TNDT5555555555` - ??n h�ng ch?a thanh to�n