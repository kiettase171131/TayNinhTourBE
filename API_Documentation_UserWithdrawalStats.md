# API Documentation - User Withdrawal Stats

## API Endpoint: GET /api/WithdrawalRequest/stats

### M� t?
API n�y cho ph�p user (TourCompany ho?c SpecialtyShop) l?y th?ng k� c� nh�n v? c�c y�u c?u r�t ti?n c?a m�nh v?i kh? n?ng l?c theo ng�y.

### URL
```
GET /api/WithdrawalRequest/stats
```

### Authentication
- Y�u c?u JWT Token
- Role: TourCompany ho?c SpecialtyShop

### Parameters (Query String)

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| startDate | DateTime? | No | Ng�y b?t ??u l?c (format: yyyy-MM-dd) |
| endDate | DateTime? | No | Ng�y k?t th�c l?c (format: yyyy-MM-dd) |

### Request Examples

```http
# L?y th?ng k� t?t c? th?i gian
GET /api/WithdrawalRequest/stats
Authorization: Bearer <user_jwt_token>

# L?y th?ng k� theo kho?ng th?i gian
GET /api/WithdrawalRequest/stats?startDate=2024-01-01&endDate=2024-01-31
Authorization: Bearer <user_jwt_token>

# L?y th?ng k� t? ng�y c? th?
GET /api/WithdrawalRequest/stats?startDate=2024-01-15
Authorization: Bearer <user_jwt_token>
```

### Response Format

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "message": "L?y th?ng k� th�nh c�ng",
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

#### Th?ng k� y�u c?u (Request Statistics)
- **totalRequests**: T?ng s? y�u c?u r�t ti?n ?� t?o
- **pendingRequests**: S? y�u c?u ?ang ch? duy?t  
- **approvedRequests**: S? y�u c?u ?� ???c duy?t
- **rejectedRequests**: S? y�u c?u b? t? ch?i
- **cancelledRequests**: S? y�u c?u ?� h?y

#### Th?ng k� s? ti?n (Amount Statistics)  
- **totalAmountRequested**: ?? **T?ng s? ti?n ?� y�u c?u r�t** (VN?)
- **pendingAmount**: T?ng s? ti?n ?ang ch? duy?t (VN?)
- **approvedAmount**: ?? **T?ng s? ti?n ?� ???c r�t th�nh c�ng** (VN?)
- **rejectedAmount**: T?ng s? ti?n b? t? ch?i (VN?)

#### Th?ng k� hi?u su?t (Performance Metrics)
- **averageProcessingTimeHours**: Th?i gian x? l� trung b�nh (gi?)
- **approvalRate**: T? l? duy?t c?a b?n (%)

#### So s�nh theo th�ng (Monthly Comparison)
- **currentMonth**: Th?ng k� th�ng hi?n t?i
  - `month`: Th�ng/n?m (MM/yyyy)
  - `requestCount`: S? y�u c?u trong th�ng
  - `totalAmount`: T?ng ti?n y�u c?u trong th�ng
  - `approvedCount`: S? y�u c?u ???c duy?t
  - `approvedAmount`: T?ng ti?n ???c duy?t
  - `approvalRate`: T? l? duy?t trong th�ng (%)
- **previousMonth**: Th?ng k� th�ng tr??c (c�ng format)

#### Th�ng tin th?i gian (Timestamps)
- **lastRequestDate**: Th?i gian t?o y�u c?u g?n nh?t
- **lastApprovalDate**: Th?i gian ???c duy?t g?n nh?t
- **startDate**: Ng�y b?t ??u l?c (n?u c�)
- **endDate**: Ng�y k?t th�c l?c (n?u c�)
- **generatedAt**: Th?i gian t?o b�o c�o

### Error Responses

#### 400 Bad Request
```json
{
  "isSuccess": false,
  "statusCode": 400,
  "message": "Ng�y b?t ??u kh�ng th? l?n h?n ng�y k?t th�c"
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
  "message": "L?i server khi l?y th?ng k�"
}
```

### Business Logic

1. **Date Filtering**: 
   - N?u kh�ng cung c?p startDate v� endDate, API tr? v? th?ng k� t?t c? th?i gian
   - Filter �p d?ng d?a tr�n `requestedAt` c?a withdrawal request

2. **Processing Time**: 
   - Ch? t�nh cho c�c y�u c?u ?� ???c x? l� (approved/rejected)
   - T�nh t? l�c t?o request ??n l�c admin x? l�

3. **Approval Rate**: 
   - C�ng th?c: approved / (approved + rejected + cancelled) * 100

4. **Monthly Stats**: 
   - Lu�n t�nh cho th�ng hi?n t?i v� th�ng tr??c
   - Kh�ng b? ?nh h??ng b?i date filter parameters

5. **Amounts**: 
   - T?t c? s? ti?n t�nh b?ng VN?
   - **approvedAmount** = t?ng s? ti?n ?� ???c r�t th�nh c�ng
   - **totalAmountRequested** = t?ng s? ti?n ?� y�u c?u r�t

### Use Cases

#### Cho TourCompany:
- Xem t?ng s? ti?n ?� r�t ???c t? doanh thu tour
- Theo d�i s? ti?n ?ang ch? x? l�
- Ph�n t�ch xu h??ng r�t ti?n theo th�ng
- Ki?m tra t? l? duy?t ?? c?i thi?n

#### Cho SpecialtyShop:
- Theo d�i doanh thu ?� r�t t? b�n s?n ph?m ??c s?n
- Qu?n l� cash flow v?i th�ng tin pending amount
- So s�nh hi?u su?t kinh doanh qua c�c th�ng
- L?p k? ho?ch t�i ch�nh d?a tr�n approval rate

### Frontend Integration

API n�y ph?c v? cho:
- **Dashboard c� nh�n** c?a shop/tour company
- **Cards th?ng k�** hi?n th? s? ti?n quan tr?ng
- **Charts** so s�nh th�ng hi?n t?i vs th�ng tr??c
- **Date pickers** ?? filter theo kho?ng th?i gian
- **Performance indicators** v? approval rate v� processing time

### Sample Frontend Display

```
???????????????????????????????????????????????
?  ?? Th?ng k� r�t ti?n c?a t�i               ?
???????????????????????????????????????????????
?  ?? T?ng ?� r�t: 11,000,000 VN?            ?
?  ? ?ang ch?: 2,500,000 VN?                ?  
?  ?? T? l? duy?t: 85.7%                     ?
?  ? Th?i gian x? l� TB: 18.5 gi?           ?
???????????????????????????????????????????????
```