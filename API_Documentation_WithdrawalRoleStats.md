# API Th?ng K� Y�u C?u R�t Ti?n Theo Role

## M� t?
API n�y cung c?p th?ng k� y�u c?u r�t ti?n cho role **TourCompany** v� **SpecialtyShop**, v?i kh? n?ng l?c theo kho?ng th?i gian.

## Endpoint
```
GET /api/Admin/Withdrawal/role-stats
```

## Quy?n truy c?p
- **Role**: Admin
- **Authentication**: JWT Bearer Token

## Tham s? truy v?n (Query Parameters)

| Tham s? | Ki?u | B?t bu?c | M� t? | V� d? |
|---------|------|----------|-------|-------|
| `startDate` | DateTime | Kh�ng | Ng�y b?t ??u l?c (yyyy-MM-dd) | `2024-01-01` |
| `endDate` | DateTime | Kh�ng | Ng�y k?t th�c l?c (yyyy-MM-dd) | `2024-12-31` |

## Ph?n h?i th�nh c�ng (200 OK)

```json
{
  "success": true,
  "message": "L?y th?ng k� th�nh c�ng",
  "data": {
    "tourCompanyStats": {
      "role": "Tour Company",
      "totalRequests": 150,
      "pendingRequests": 25,
      "approvedRequests": 100,
      "rejectedRequests": 25,
      "totalAmountRequested": 75000000.00,
      "pendingAmount": 12500000.00,
      "approvedAmount": 50000000.00,
      "rejectedAmount": 12500000.00,
      "startDate": "2024-01-01T00:00:00",
      "endDate": "2024-12-31T23:59:59"
    },
    "specialtyShopStats": {
      "role": "Specialty Shop",
      "totalRequests": 80,
      "pendingRequests": 15,
      "approvedRequests": 55,
      "rejectedRequests": 10,
      "totalAmountRequested": 40000000.00,
      "pendingAmount": 7500000.00,
      "approvedAmount": 27500000.00,
      "rejectedAmount": 5000000.00,
      "startDate": "2024-01-01T00:00:00",
      "endDate": "2024-12-31T23:59:59"
    },
    "generatedAt": "2024-01-15T10:30:00Z",
    "startDate": "2024-01-01T00:00:00",
    "endDate": "2024-12-31T23:59:59"
  }
}
```

## C?u tr�c d? li?u ph?n h?i

### WithdrawalRoleStatsSummaryDto
| Tr??ng | Ki?u | M� t? |
|--------|------|-------|
| `tourCompanyStats` | WithdrawalRoleStatsDto | Th?ng k� cho TourCompany |
| `specialtyShopStats` | WithdrawalRoleStatsDto | Th?ng k� cho SpecialtyShop |
| `generatedAt` | DateTime | Th?i gian t?o b�o c�o |
| `startDate` | DateTime? | Ng�y b?t ??u l?c |
| `endDate` | DateTime? | Ng�y k?t th�c l?c |

### WithdrawalRoleStatsDto
| Tr??ng | Ki?u | M� t? |
|--------|------|-------|
| `role` | string | T�n role (Tour Company ho?c Specialty Shop) |
| `totalRequests` | int | T?ng s? y�u c?u r�t ti?n |
| `pendingRequests` | int | S? y�u c?u ?ang ch? duy?t |
| `approvedRequests` | int | S? y�u c?u ?� ???c duy?t |
| `rejectedRequests` | int | S? y�u c?u b? t? ch?i |
| `totalAmountRequested` | decimal | T?ng s? ti?n ?� y�u c?u r�t (VN?) |
| `pendingAmount` | decimal | T?ng s? ti?n ?ang ch? duy?t (VN?) |
| `approvedAmount` | decimal | T?ng s? ti?n ?� ???c duy?t (VN?) |
| `rejectedAmount` | decimal | T?ng s? ti?n b? t? ch?i (VN?) |
| `startDate` | DateTime? | Ng�y b?t ??u l?c |
| `endDate` | DateTime? | Ng�y k?t th�c l?c |

## V� d? s? d?ng

### 1. L?y th?ng k� t?ng th? (kh�ng l?c th?i gian)
```bash
GET /api/Admin/Withdrawal/role-stats
Authorization: Bearer [JWT_TOKEN]
```

### 2. L?y th?ng k� theo kho?ng th?i gian
```bash
GET /api/Admin/Withdrawal/role-stats?startDate=2024-01-01&endDate=2024-12-31
Authorization: Bearer [JWT_TOKEN]
```

### 3. L?y th?ng k� t? m?t ng�y c? th?
```bash
GET /api/Admin/Withdrawal/role-stats?startDate=2024-06-01
Authorization: Bearer [JWT_TOKEN]
```

## L?i c� th? x?y ra

### 400 Bad Request
```json
{
  "success": false,
  "message": "Ng�y b?t ??u kh�ng th? l?n h?n ng�y k?t th�c"
}
```

### 401 Unauthorized
```json
{
  "success": false,
  "message": "Unauthorized"
}
```

### 403 Forbidden
```json
{
  "success": false,
  "message": "Forbidden - Admin role required"
}
```

### 500 Internal Server Error
```json
{
  "success": false,
  "message": "L?i server khi l?y th?ng k� theo role"
}
```

## Ghi ch�
- API n�y ch? th?ng k� cho 2 role ch�nh: **Tour Company** v� **Specialty Shop**
- N?u kh�ng cung c?p `startDate` v� `endDate`, h? th?ng s? l?y th?ng k� t?t c? d? li?u
- Th?i gian ???c t�nh theo UTC
- S? ti?n ???c hi?n th? theo ??n v? VN? (Vietnamese Dong)
- Ch? t�nh c�c y�u c?u r�t ti?n c� `IsActive = true` v� `IsDeleted = false`