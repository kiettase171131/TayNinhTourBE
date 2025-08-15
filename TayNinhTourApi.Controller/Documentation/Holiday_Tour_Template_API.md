# Holiday Tour Template API Documentation

## T?ng quan

API m?i ???c t?o ?? h? tr? vi?c t?o tour template cho các ngày l? ??c bi?t. Khác v?i tour template thông th??ng (t?o slots cho c? tháng d?a trên schedule days), holiday template cho phép t?o 1 slot duy nh?t cho ngày c? th? ???c ch?n.

**?? VALIDATION QUAN TR?NG**: Holiday template áp d?ng **cùng quy t?c 30 ngày** nh? tour template bình th??ng ?? ??m b?o Tour Company có ?? th?i gian chu?n b?.

## Endpoint M?i

### T?o Holiday Tour Template
POST /api/TourCompany/template/holiday
**Authorization**: Bearer Token (Role: "Tour Company")

#### Request Body
{
  "title": "Tour Núi Bà ?en - Ngày L? 30/4",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "tourDate": "2025-04-30",
  "images": ["image1.jpg", "image2.jpg"]
}
#### Request Body Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `title` | string | Yes | 1-200 chars | Tên tour template |
| `startLocation` | string | Yes | 1-500 chars | ?i?m b?t ??u |
| `endLocation` | string | Yes | 1-500 chars | ?i?m k?t thúc |
| `templateType` | string | Yes | `FreeScenic`, `PaidAttraction` | Lo?i tour template |
| `tourDate` | DateOnly | Yes | **Ph?i sau ít nh?t 30 ngày t? ngày t?o** | Ngày di?n ra tour |
| `images` | array[string] | No | max 10 items | Danh sách URL ?nh |

#### ?? Quy T?c Validation Quan Tr?ng

**1. Quy t?c 30 ngày** (gi?ng tour template bình th??ng):
- `tourDate` ph?i **sau ít nh?t 30 ngày** t? ngày t?o template
- Ví d?: N?u hôm nay là 15/01/2025 ? `tourDate` s?m nh?t là 14/02/2025

**2. Quy t?c th?i gian h?p l?**:
- `tourDate` ph?i trong t??ng lai (> ngày hi?n t?i)
- Không quá 2 n?m t? ngày hi?n t?i
- N?m ph?i t? 2024-2030

**3. Linh ho?t v? ngày trong tu?n**:
- Holiday template có th? ch?n **b?t k? ngày nào** trong tu?n (không ch? Saturday/Sunday)

#### Response

**Status Code**: `201 Created`
{
  "statusCode": 201,
  "message": "T?o tour template ngày l? thành công và ?ã t?o slot cho ngày 30/04/2025 (sau 105 ngày t? ngày t?o)",
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Tour Núi Bà ?en - Ngày L? 30/4",
    "templateType": "FreeScenic",
    "scheduleDays": "Wednesday",
    "startLocation": "TP.HCM",
    "endLocation": "Tây Ninh",
    "month": 4,
    "year": 2025,
    "isActive": true,
    "createdAt": "2025-01-15T10:00:00Z"
  }
}
#### Error Responses

**Status Code**: `400 Bad Request` - Vi ph?m quy t?c 30 ngày{
  "statusCode": 400,
  "message": "D? li?u không h?p l? - Vui lòng ki?m tra và s?a các l?i sau",
  "success": false,
  "validationErrors": [
    "tourDate: Ngày tour ph?i sau ít nh?t 30 ngày t? ngày t?o (15/01/2025). Ngày s?m nh?t có th?: 14/02/2025. G?i ý: Ch?n ngày 21/02/2025 ho?c mu?n h?n. Ví d? JSON h?p l?: \"tourDate\": \"2025-02-21\"",
    "?? H??NG D?N HOLIDAY TEMPLATE:",
    "• Ngày hi?n t?i: 15/01/2025 - KHÔNG th? ch?n",
    "• Ngày s?m nh?t: 14/02/2025 (sau 30 ngày)",
    "• Ngày mu?n nh?t: 15/01/2027 (t?i ?a 2 n?m)",
    "• Ví d? JSON h?p l?: {\"tourDate\": \"2025-02-21\"}",
    "• Khác template th??ng: Holiday template có th? ch?n b?t k? ngày nào trong tu?n"
  ],
  "fieldErrors": {
    "tourDate": ["Ngày tour ph?i sau ít nh?t 30 ngày t? ngày t?o (15/01/2025). Ngày s?m nh?t có th?: 14/02/2025. G?i ý: Ch?n ngày 21/02/2025 ho?c mu?n h?n. Ví d? JSON h?p l?: \"tourDate\": \"2025-02-21\""]
  }
}
**Status Code**: `401 Unauthorized`{
  "statusCode": 401,
  "message": "Token không h?p l? ho?c ?ã h?t h?n"
}
**Status Code**: `403 Forbidden`{
  "statusCode": 403,
  "message": "B?n không có quy?n truy c?p endpoint này"
}
## ??c ?i?m c?a Holiday Template

### 1. **Cùng quy t?c validation v?i Regular Template**
- ? **Quy t?c 30 ngày**: Ngày tour ph?i sau ít nh?t 30 ngày t? ngày t?o
- ? **Quy t?c n?m**: 2024-2030
- ? **Quy t?c th?i gian t?i ?a**: Không quá 2 n?m t? hi?n t?i

### 2. **T? ??ng xác ??nh Schedule Day**
- H? th?ng t? ??ng xác ??nh th? trong tu?n d?a trên ngày ???c ch?n
- Ví d?: Ch?n ngày 30/4/2025 (Th? 4) ? `scheduleDays = "Wednesday"`
- **Linh ho?t**: Có th? ch?n b?t k? ngày nào trong tu?n (không ch? Saturday/Sunday)

### 3. **T?o 1 slot duy nh?t**
- Khác v?i template thông th??ng (t?o 4 slots/tháng), holiday template ch? t?o 1 slot cho ngày c? th?
- Slot ???c t?o v?i:
  - `tourDate`: Ngày ???c ch?n
  - `scheduleDay`: Th? t??ng ?ng
  - `status`: Available
  - `isActive`: true

### 4. **Validation t?ng c??ng**
- Áp d?ng c? validation c? b?n và business rules
- Ki?m tra c? `ValidateFirstSlotDate` ?? ??m b?o tuân th? quy t?c 30 ngày

## So sánh v?i Tour Template thông th??ng

| Aspect | Regular Template | Holiday Template |
|--------|------------------|------------------|
| **Input** | Month, Year, ScheduleDays | TourDate |
| **Slots Created** | 4 slots/tháng theo schedule | 1 slot cho ngày c? th? |
| **Use Case** | Tour ??nh k? hàng tu?n | Tour ??c bi?t cho ngày l? |
| **ScheduleDays** | User ch?n (Saturday/Sunday) | T? ??ng t? tourDate |
| **30-day Rule** | ? Áp d?ng | ? Áp d?ng |
| **Year Range** | ? 2024-2030 | ? 2024-2030 |
| **Weekend Only** | ? Saturday/Sunday | ? B?t k? ngày nào |

## Workflow s? d?ng

### 1. **Tour Company t?o Holiday Template**# Ví d? hôm nay là 15/01/2025
curl -X POST "/api/TourCompany/template/holiday" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Tour T?t Nguyên ?án 2025",
    "startLocation": "TP.HCM", 
    "endLocation": "Tây Ninh",
    "templateType": "FreeScenic",
    "tourDate": "2025-02-21"
  }'
### 2. **H? th?ng t? ??ng**
- Ki?m tra quy t?c 30 ngày: 21/02/2025 > 14/02/2025 ?
- T?o TourTemplate v?i `scheduleDays = "Friday"`
- T?o 1 TourSlot cho ngày 21/02/2025
- Set `month = 2, year = 2025`

### 3. **Ti?p theo có th?**
- T?o TourDetails cho template này
- T?o TourOperation ?? customers có th? book
- Qu?n lý nh? các tour template khác

## Business Rules

1. **?? 30-Day Rule**: Ngày tour ph?i sau ít nh?t 30 ngày t? ngày t?o template
2. **?? Date Validation**: Ngày tour ph?i trong t??ng lai và không quá 2 n?m
3. **?? Year Range**: N?m ph?i t? 2024-2030
4. **?? Single Slot**: Ch? t?o 1 slot duy nh?t cho ngày ???c ch?n
5. **? Auto Schedule**: T? ??ng xác ??nh scheduleDays t? ngày ???c ch?n
6. **?? Standard Permissions**: C?n role "Tour Company" ?? t?o
7. **?? Any Day**: Có th? ch?n b?t k? ngày nào trong tu?n (khác regular template)

## Examples

### ? Ví d? thành công
// Hôm nay: 15/01/2025, ngày s?m nh?t: 14/02/2025
{
  "title": "Tour Tây Ninh - T?t Nguyên ?án 2025",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh", 
  "templateType": "FreeScenic",
  "tourDate": "2025-02-21"  // ? Sau 30 ngày
}
### ? Ví d? l?i vi ph?m 30 ngày
// Hôm nay: 15/01/2025
{
  "title": "Tour Quá S?m",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic", 
  "tourDate": "2025-02-01"  // ? Ch? sau 17 ngày
}
### ? Ví d? tour cu?i tu?n
{
  "title": "Tour Qu?c Khánh 2/9",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "PaidAttraction", 
  "tourDate": "2025-09-02"  // ? Th? 3 c?ng ???c
}
## Implementation Details

### Validation Chain

1. **Basic Validation** ? Title, Locations, Template Type
2. **Date Validation** ? Future date, 30-day rule, max 2 years  
3. **Business Rules** ? ValidateBusinessRules()
4. **Slot Validation** ? ValidateFirstSlotDate()
5. **Image Validation** ? Validate image URLs (if provided)

### Key Validation Methods
// Ki?m tra quy t?c 30 ngày
TourTemplateValidator.ValidateFirstSlotDate(createdAt, month, year)

// Validation t?ng th? business rules
TourTemplateValidator.ValidateBusinessRules(tourTemplate)

// Validation c? b?n holiday template
ValidateHolidayTemplateRequest(request)
---

**Ngày t?o**: 15/01/2025  
**Version**: 2.0 (Thêm quy t?c 30 ngày)  
**Contact**: ??i phát tri?n TayNinhTour