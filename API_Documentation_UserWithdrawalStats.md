# API Documentation - User Withdrawal Stats

## API Endpoint: GET /api/WithdrawalRequest/stats

### Mô t?
API này cho phép user (TourCompany ho?c SpecialtyShop) l?y th?ng kê cá nhân v? các yêu c?u rút ti?n c?a mình v?i kh? n?ng l?c theo ngày.

### URL
```
GET /api/WithdrawalRequest/stats
```

### Authentication
- Yêu c?u JWT Token
- Role: TourCompany ho?c SpecialtyShop

### Parameters (Query String)

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| startDate | DateTime? | No | Ngày b?t ??u l?c (format: yyyy-MM-dd) |
| endDate | DateTime? | No | Ngày k?t thúc l?c (format: yyyy-MM-dd) |

### Request Examples

```http
# L?y th?ng kê t?t c? th?i gian
GET /api/WithdrawalRequest/stats
Authorization: Bearer <user_jwt_token>

# L?y th?ng kê theo kho?ng th?i gian
GET /api/WithdrawalRequest/stats?startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer <user_jwt_token>

# L?y th?ng kê t? ngày c? th?
GET /api/WithdrawalRequest/stats?startDate=2024-01-15
Authorization: Bearer <user_jwt_token>
```

### Response Format

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "L?y th?ng kê thành công",
  "data": {
    "totalRequests": 25,
    "pendingRequests": 3,
    "approvedRequests": 18,
    "rejectedRequests": 3,
    "cancelledRequests": 1,
    "totalAmountRequested": 15000000,
    "pendingAmount": 2500000,
    "approvedAmount": 11000000,
    "rejectedAmount": 1200000,
    "averageProcessingTimeHours": 18.5,
    "approvalRate": 85.7,
    "currentMonth": {
      "month": "01/2024",
      "requestCount": 8,
      "totalAmount": 4500000,
      "approvedCount": 6,
      "approvedAmount": 3800000,
      "approvalRate": 75.0
    },
    "previousMonth": {
      "month": "12/2023",
      "requestCount": 5,
      "totalAmount": 2800000,
      "approvedCount": 4,
      "approvedAmount": 2500000,
      "approvalRate": 80.0
    },
    "lastRequestDate": "2024-01-31T14:30:00Z",
    "lastApprovalDate": "2024-01-31T16:45:00Z",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-01-31T23:59:59Z",
    "generatedAt": "2024-02-01T10:00:00Z"
  }
}
```

### Response Fields Description

#### Th?ng kê yêu c?u (Request Statistics)
- **totalRequests**: T?ng s? yêu c?u rút ti?n ?ã t?o
- **pendingRequests**: S? yêu c?u ?ang ch? duy?t  
- **approvedRequests**: S? yêu c?u ?ã ???c duy?t
- **rejectedRequests**: S? yêu c?u b? t? ch?i
- **cancelledRequests**: S? yêu c?u ?ã h?y

#### Th?ng kê s? ti?n (Amount Statistics)  
- **totalAmountRequested**: ?? **T?ng s? ti?n ?ã yêu c?u rút** (VN?)
- **pendingAmount**: T?ng s? ti?n ?ang ch? duy?t (VN?)
- **approvedAmount**: ?? **T?ng s? ti?n ?ã ???c rút thành công** (VN?)
- **rejectedAmount**: T?ng s? ti?n b? t? ch?i (VN?)

#### Th?ng kê hi?u su?t (Performance Metrics)
- **averageProcessingTimeHours**: Th?i gian x? lý trung bình (gi?)
- **approvalRate**: T? l? duy?t c?a b?n (%)

#### So sánh theo tháng (Monthly Comparison)
- **currentMonth**: Th?ng kê tháng hi?n t?i
  - `month`: Tháng/n?m (MM/yyyy)
  - `requestCount`: S? yêu c?u trong tháng
  - `totalAmount`: T?ng ti?n yêu c?u trong tháng
  - `approvedCount`: S? yêu c?u ???c duy?t
  - `approvedAmount`: T?ng ti?n ???c duy?t
  - `approvalRate`: T? l? duy?t trong tháng (%)
- **previousMonth**: Th?ng kê tháng tr??c (cùng format)

#### Thông tin th?i gian (Timestamps)
- **lastRequestDate**: Th?i gian t?o yêu c?u g?n nh?t
- **lastApprovalDate**: Th?i gian ???c duy?t g?n nh?t
- **startDate**: Ngày b?t ??u l?c (n?u có)
- **endDate**: Ngày k?t thúc l?c (n?u có)
- **generatedAt**: Th?i gian t?o báo cáo

### Error Responses

#### 400 Bad Request
```json
{
  "isSuccess": false,
  "statusCode": 400,
  "message": "Ngày b?t ??u không th? l?n h?n ngày k?t thúc"
}
```

#### 401 Unauthorized
```json
{
  "isSuccess": false,
  "statusCode": 401,
  "message": "Unauthorized"
}
```

#### 403 Forbidden
```json
{
  "isSuccess": false,
  "statusCode": 403,
  "message": "Forbidden - TourCompany or SpecialtyShop role required"
}
```

#### 500 Internal Server Error
```json
{
  "isSuccess": false,
  "statusCode": 500,
  "message": "L?i server khi l?y th?ng kê"
}
```

### Business Logic

1. **Date Filtering**: 
   - N?u không cung c?p startDate và endDate, API tr? v? th?ng kê t?t c? th?i gian
   - Filter áp d?ng d?a trên `requestedAt` c?a withdrawal request

2. **Processing Time**: 
   - Ch? tính cho các yêu c?u ?ã ???c x? lý (approved/rejected)
   - Tính t? lúc t?o request ??n lúc admin x? lý

3. **Approval Rate**: 
   - Công th?c: approved / (approved + rejected + cancelled) * 100

4. **Monthly Stats**: 
   - Luôn tính cho tháng hi?n t?i và tháng tr??c
   - Không b? ?nh h??ng b?i date filter parameters

5. **Amounts**: 
   - T?t c? s? ti?n tính b?ng VN?
   - **approvedAmount** = t?ng s? ti?n ?ã ???c rút thành công
   - **totalAmountRequested** = t?ng s? ti?n ?ã yêu c?u rút

### Use Cases

#### Cho TourCompany:
- Xem t?ng s? ti?n ?ã rút ???c t? doanh thu tour
- Theo dõi s? ti?n ?ang ch? x? lý
- Phân tích xu h??ng rút ti?n theo tháng
- Ki?m tra t? l? duy?t ?? c?i thi?n

#### Cho SpecialtyShop:
- Theo dõi doanh thu ?ã rút t? bán s?n ph?m ??c s?n
- Qu?n lý cash flow v?i thông tin pending amount
- So sánh hi?u su?t kinh doanh qua các tháng
- L?p k? ho?ch tài chính d?a trên approval rate

### Frontend Integration

API này ph?c v? cho:
- **Dashboard cá nhân** c?a shop/tour company
- **Cards th?ng kê** hi?n th? s? ti?n quan tr?ng
- **Charts** so sánh tháng hi?n t?i vs tháng tr??c
- **Date pickers** ?? filter theo kho?ng th?i gian
- **Performance indicators** v? approval rate và processing time

### Sample Frontend Display

```
???????????????????????????????????????????????
?  ?? Th?ng kê rút ti?n c?a tôi               ?
???????????????????????????????????????????????
?  ?? T?ng ?ã rút: 11,000,000 VN?            ?
?  ? ?ang ch?: 2,500,000 VN?                ?  
?  ?? T? l? duy?t: 85.7%                     ?
?  ? Th?i gian x? lý TB: 18.5 gi?           ?
???????????????????????????????????????????????
```