# Holiday Tour Template API Documentation

## T?ng quan

API m?i ???c t?o ?? h? tr? vi?c t?o tour template cho c�c ng�y l? ??c bi?t. Kh�c v?i tour template th�ng th??ng (t?o slots cho c? th�ng d?a tr�n schedule days), holiday template cho ph�p t?o 1 slot duy nh?t cho ng�y c? th? ???c ch?n.

**?? VALIDATION QUAN TR?NG**: Holiday template �p d?ng **c�ng quy t?c 30 ng�y** nh? tour template b�nh th??ng ?? ??m b?o Tour Company c� ?? th?i gian chu?n b?.

## Endpoint M?i

### T?o Holiday Tour Template
POST /api/TourCompany/template/holiday
**Authorization**: Bearer Token (Role: "Tour Company")

#### Request Body
{
  "title": "Tour N�i B� ?en - Ng�y L? 30/4",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "tourDate": "2025-04-30",
  "images": ["image1.jpg", "image2.jpg"]
}
#### Request Body Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `title` | string | Yes | 1-200 chars | T�n tour template |
| `startLocation` | string | Yes | 1-500 chars | ?i?m b?t ??u |
| `endLocation` | string | Yes | 1-500 chars | ?i?m k?t th�c |
| `templateType` | string | Yes | `FreeScenic`, `PaidAttraction` | Lo?i tour template |
| `tourDate` | DateOnly | Yes | **Ph?i sau �t nh?t 30 ng�y t? ng�y t?o** | Ng�y di?n ra tour |
| `images` | array[string] | No | max 10 items | Danh s�ch URL ?nh |

#### ?? Quy T?c Validation Quan Tr?ng

**1. Quy t?c 30 ng�y** (gi?ng tour template b�nh th??ng):
- `tourDate` ph?i **sau �t nh?t 30 ng�y** t? ng�y t?o template
- V� d?: N?u h�m nay l� 15/01/2025 ? `tourDate` s?m nh?t l� 14/02/2025

**2. Quy t?c th?i gian h?p l?**:
- `tourDate` ph?i trong t??ng lai (> ng�y hi?n t?i)
- Kh�ng qu� 2 n?m t? ng�y hi?n t?i
- N?m ph?i t? 2024-2030

**3. Linh ho?t v? ng�y trong tu?n**:
- Holiday template c� th? ch?n **b?t k? ng�y n�o** trong tu?n (kh�ng ch? Saturday/Sunday)

#### Response

**Status Code**: `201 Created`
{
  "statusCode": 201,
  "message": "T?o tour template ng�y l? th�nh c�ng v� ?� t?o slot cho ng�y 30/04/2025 (sau 105 ng�y t? ng�y t?o)",
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Tour N�i B� ?en - Ng�y L? 30/4",
    "templateType": "FreeScenic",
    "scheduleDays": "Wednesday",
    "startLocation": "TP.HCM",
    "endLocation": "T�y Ninh",
    "month": 4,
    "year": 2025,
    "isActive": true,
    "createdAt": "2025-01-15T10:00:00Z"
  }
}
#### Error Responses

**Status Code**: `400 Bad Request` - Vi ph?m quy t?c 30 ng�y{
  "statusCode": 400,
  "message": "D? li?u kh�ng h?p l? - Vui l�ng ki?m tra v� s?a c�c l?i sau",
  "success": false,
  "validationErrors": [
    "tourDate: Ng�y tour ph?i sau �t nh?t 30 ng�y t? ng�y t?o (15/01/2025). Ng�y s?m nh?t c� th?: 14/02/2025. G?i �: Ch?n ng�y 21/02/2025 ho?c mu?n h?n. V� d? JSON h?p l?: \"tourDate\": \"2025-02-21\"",
    "?? H??NG D?N HOLIDAY TEMPLATE:",
    "� Ng�y hi?n t?i: 15/01/2025 - KH�NG th? ch?n",
    "� Ng�y s?m nh?t: 14/02/2025 (sau 30 ng�y)",
    "� Ng�y mu?n nh?t: 15/01/2027 (t?i ?a 2 n?m)",
    "� V� d? JSON h?p l?: {\"tourDate\": \"2025-02-21\"}",
    "� Kh�c template th??ng: Holiday template c� th? ch?n b?t k? ng�y n�o trong tu?n"
  ],
  "fieldErrors": {
    "tourDate": ["Ng�y tour ph?i sau �t nh?t 30 ng�y t? ng�y t?o (15/01/2025). Ng�y s?m nh?t c� th?: 14/02/2025. G?i �: Ch?n ng�y 21/02/2025 ho?c mu?n h?n. V� d? JSON h?p l?: \"tourDate\": \"2025-02-21\""]
  }
}
**Status Code**: `401 Unauthorized`{
  "statusCode": 401,
  "message": "Token kh�ng h?p l? ho?c ?� h?t h?n"
}
**Status Code**: `403 Forbidden`{
  "statusCode": 403,
  "message": "B?n kh�ng c� quy?n truy c?p endpoint n�y"
}
## ??c ?i?m c?a Holiday Template

### 1. **C�ng quy t?c validation v?i Regular Template**
- ? **Quy t?c 30 ng�y**: Ng�y tour ph?i sau �t nh?t 30 ng�y t? ng�y t?o
- ? **Quy t?c n?m**: 2024-2030
- ? **Quy t?c th?i gian t?i ?a**: Kh�ng qu� 2 n?m t? hi?n t?i

### 2. **T? ??ng x�c ??nh Schedule Day**
- H? th?ng t? ??ng x�c ??nh th? trong tu?n d?a tr�n ng�y ???c ch?n
- V� d?: Ch?n ng�y 30/4/2025 (Th? 4) ? `scheduleDays = "Wednesday"`
- **Linh ho?t**: C� th? ch?n b?t k? ng�y n�o trong tu?n (kh�ng ch? Saturday/Sunday)

### 3. **T?o 1 slot duy nh?t**
- Kh�c v?i template th�ng th??ng (t?o 4 slots/th�ng), holiday template ch? t?o 1 slot cho ng�y c? th?
- Slot ???c t?o v?i:
  - `tourDate`: Ng�y ???c ch?n
  - `scheduleDay`: Th? t??ng ?ng
  - `status`: Available
  - `isActive`: true

### 4. **Validation t?ng c??ng**
- �p d?ng c? validation c? b?n v� business rules
- Ki?m tra c? `ValidateFirstSlotDate` ?? ??m b?o tu�n th? quy t?c 30 ng�y

## So s�nh v?i Tour Template th�ng th??ng

| Aspect | Regular Template | Holiday Template |
|--------|------------------|------------------|
| **Input** | Month, Year, ScheduleDays | TourDate |
| **Slots Created** | 4 slots/th�ng theo schedule | 1 slot cho ng�y c? th? |
| **Use Case** | Tour ??nh k? h�ng tu?n | Tour ??c bi?t cho ng�y l? |
| **ScheduleDays** | User ch?n (Saturday/Sunday) | T? ??ng t? tourDate |
| **30-day Rule** | ? �p d?ng | ? �p d?ng |
| **Year Range** | ? 2024-2030 | ? 2024-2030 |
| **Weekend Only** | ? Saturday/Sunday | ? B?t k? ng�y n�o |

## Workflow s? d?ng

### 1. **Tour Company t?o Holiday Template**# V� d? h�m nay l� 15/01/2025
curl -X POST "/api/TourCompany/template/holiday" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Tour T?t Nguy�n ?�n 2025",
    "startLocation": "TP.HCM", 
    "endLocation": "T�y Ninh",
    "templateType": "FreeScenic",
    "tourDate": "2025-02-21"
  }'
### 2. **H? th?ng t? ??ng**
- Ki?m tra quy t?c 30 ng�y: 21/02/2025 > 14/02/2025 ?
- T?o TourTemplate v?i `scheduleDays = "Friday"`
- T?o 1 TourSlot cho ng�y 21/02/2025
- Set `month = 2, year = 2025`

### 3. **Ti?p theo c� th?**
- T?o TourDetails cho template n�y
- T?o TourOperation ?? customers c� th? book
- Qu?n l� nh? c�c tour template kh�c

## Business Rules

1. **?? 30-Day Rule**: Ng�y tour ph?i sau �t nh?t 30 ng�y t? ng�y t?o template
2. **?? Date Validation**: Ng�y tour ph?i trong t??ng lai v� kh�ng qu� 2 n?m
3. **?? Year Range**: N?m ph?i t? 2024-2030
4. **?? Single Slot**: Ch? t?o 1 slot duy nh?t cho ng�y ???c ch?n
5. **? Auto Schedule**: T? ??ng x�c ??nh scheduleDays t? ng�y ???c ch?n
6. **?? Standard Permissions**: C?n role "Tour Company" ?? t?o
7. **?? Any Day**: C� th? ch?n b?t k? ng�y n�o trong tu?n (kh�c regular template)

## Examples

### ? V� d? th�nh c�ng
// H�m nay: 15/01/2025, ng�y s?m nh?t: 14/02/2025
{
  "title": "Tour T�y Ninh - T?t Nguy�n ?�n 2025",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh", 
  "templateType": "FreeScenic",
  "tourDate": "2025-02-21"  // ? Sau 30 ng�y
}
### ? V� d? l?i vi ph?m 30 ng�y
// H�m nay: 15/01/2025
{
  "title": "Tour Qu� S?m",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic", 
  "tourDate": "2025-02-01"  // ? Ch? sau 17 ng�y
}
### ? V� d? tour cu?i tu?n
{
  "title": "Tour Qu?c Kh�nh 2/9",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
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
// Ki?m tra quy t?c 30 ng�y
TourTemplateValidator.ValidateFirstSlotDate(createdAt, month, year)

// Validation t?ng th? business rules
TourTemplateValidator.ValidateBusinessRules(tourTemplate)

// Validation c? b?n holiday template
ValidateHolidayTemplateRequest(request)
---

**Ng�y t?o**: 15/01/2025  
**Version**: 2.0 (Th�m quy t?c 30 ng�y)  
**Contact**: ??i ph�t tri?n TayNinhTour