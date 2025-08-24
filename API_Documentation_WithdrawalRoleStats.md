# API Th?ng Kê Yêu C?u Rút Ti?n Theo Role

## Mô t?
API này cung c?p th?ng kê yêu c?u rút ti?n cho role **TourCompany** và **SpecialtyShop**, v?i kh? n?ng l?c theo kho?ng th?i gian.

## Endpoint
```
GET /api/Admin/Withdrawal/role-stats
```

## Quy?n truy c?p
- **Role**: Admin
- **Authentication**: JWT Bearer Token

## Tham s? truy v?n (Query Parameters)

| Tham s? | Ki?u | B?t bu?c | Mô t? | Ví d? |
|---------|------|----------|-------|-------|
| `startDate` | DateTime | Không | Ngày b?t ??u l?c (yyyy-MM-dd) | `2024-01-01` |
| `endDate` | DateTime | Không | Ngày k?t thúc l?c (yyyy-MM-dd) | `2024-12-31` |

## Ph?n h?i thành công (200 OK)

```json
{
  "success": true,
  "message": "L?y th?ng kê thành công",
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

## C?u trúc d? li?u ph?n h?i

### WithdrawalRoleStatsSummaryDto
| Tr??ng | Ki?u | Mô t? |
|--------|------|-------|
| `tourCompanyStats` | WithdrawalRoleStatsDto | Th?ng kê cho TourCompany |
| `specialtyShopStats` | WithdrawalRoleStatsDto | Th?ng kê cho SpecialtyShop |
| `generatedAt` | DateTime | Th?i gian t?o báo cáo |
| `startDate` | DateTime? | Ngày b?t ??u l?c |
| `endDate` | DateTime? | Ngày k?t thúc l?c |

### WithdrawalRoleStatsDto
| Tr??ng | Ki?u | Mô t? |
|--------|------|-------|
| `role` | string | Tên role (Tour Company ho?c Specialty Shop) |
| `totalRequests` | int | T?ng s? yêu c?u rút ti?n |
| `pendingRequests` | int | S? yêu c?u ?ang ch? duy?t |
| `approvedRequests` | int | S? yêu c?u ?ã ???c duy?t |
| `rejectedRequests` | int | S? yêu c?u b? t? ch?i |
| `totalAmountRequested` | decimal | T?ng s? ti?n ?ã yêu c?u rút (VN?) |
| `pendingAmount` | decimal | T?ng s? ti?n ?ang ch? duy?t (VN?) |
| `approvedAmount` | decimal | T?ng s? ti?n ?ã ???c duy?t (VN?) |
| `rejectedAmount` | decimal | T?ng s? ti?n b? t? ch?i (VN?) |
| `startDate` | DateTime? | Ngày b?t ??u l?c |
| `endDate` | DateTime? | Ngày k?t thúc l?c |

## Ví d? s? d?ng

### 1. L?y th?ng kê t?ng th? (không l?c th?i gian)
```bash
GET /api/Admin/Withdrawal/role-stats
Authorization: Bearer [JWT_TOKEN]
```

### 2. L?y th?ng kê theo kho?ng th?i gian
```bash
GET /api/Admin/Withdrawal/role-stats?startDate=2024-01-01&endDate=2024-12-31
Authorization: Bearer [JWT_TOKEN]
```

### 3. L?y th?ng kê t? m?t ngày c? th?
```bash
GET /api/Admin/Withdrawal/role-stats?startDate=2024-06-01
Authorization: Bearer [JWT_TOKEN]
```

## L?i có th? x?y ra

### 400 Bad Request
```json
{
  "success": false,
  "message": "Ngày b?t ??u không th? l?n h?n ngày k?t thúc"
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
  "message": "L?i server khi l?y th?ng kê theo role"
}
```

## Ghi chú
- API này ch? th?ng kê cho 2 role chính: **Tour Company** và **Specialty Shop**
- N?u không cung c?p `startDate` và `endDate`, h? th?ng s? l?y th?ng kê t?t c? d? li?u
- Th?i gian ???c tính theo UTC
- S? ti?n ???c hi?n th? theo ??n v? VN? (Vietnamese Dong)
- Ch? tính các yêu c?u rút ti?n có `IsActive = true` và `IsDeleted = false`