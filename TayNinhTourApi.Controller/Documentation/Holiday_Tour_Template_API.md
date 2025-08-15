# Holiday Tour Template API Documentation

## T?ng quan

API m?i ???c t?o ?? h? tr? vi?c t?o tour template cho các ngày l? ??c bi?t. Khác v?i tour template th??ng th??ng (t?o slots cho c? tháng d?a trên schedule days), holiday template cho phép t?o 1 slot duy nh?t cho ngày c? th? ???c ch?n.

**?? VALIDATION QUAN TR?NG**: Holiday template áp d?ng **cùng quy t?c 30 ngày** nh? tour template bình th??ng ?? ??m b?o Tour Company có ?? th?i gian chu?n b?.

**? ??C BI?T M?I**: Holiday template s? d?ng validator riêng `HolidayTourTemplateValidator` cho phép ch?n **b?t k? ngày nào trong tu?n** (Monday-Sunday), không b? gi?i h?n Saturday/Sunday nh? regular template.

## Endpoint M?i

### T?o Holiday Tour Template
POST /api/TourCompany/template/holiday
**Authorization**: Bearer Token (Role: "Tour Company")

#### Request Body{
  "title": "Tour Núi Bà ?en - Ngày L? 30/4",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "tourDate": "2025-04-30",
  "images": ["image1.jpg", "image2.jpg"]
}### ? C?p nh?t Holiday Tour Template
PATCH /api/TourCompany/template/holiday/{id}
**Authorization**: Bearer Token (Role: "Tour Company")

#### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | string (UUID) | Yes | ID c?a holiday tour template |

#### Request Body (Partial Update){
  "title": "Tour Núi Bà ?en - Ngày L? 30/4 (C?p nh?t)",
  "tourDate": "2025-05-01",
  "templateType": "PaidAttraction"
}#### Request Body Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `title` | string | No | 1-200 chars | Tên tour template m?i |
| `startLocation` | string | No | 1-500 chars | ?i?m b?t ??u m?i |
| `endLocation` | string | No | 1-500 chars | ?i?m k?t thúc m?i |
| `templateType` | string | No | `FreeScenic`, `PaidAttraction` | Lo?i tour template m?i |
| `tourDate` | DateOnly | No | **Ph?i sau ít nh?t 30 ngày t? ngày t?o template** | Ngày tour m?i |
| `images` | array[string] | No | max 10 items | Danh sách URL ?nh m?i |

**?? L?u ý quan tr?ng**:
- Ch? g?i các fields mu?n thay ??i (partial update)
- Fields không g?i s? gi? nguyên giá tr? c?
- N?u c?p nh?t `tourDate`, slot c? s? ???c c?p nh?t v?i ngày m?i
- `tourDate` m?i v?n ph?i tuân th? quy t?c 30 ngày t? ngày t?o template

#### Response

**Status Code**: `200 OK`{
  "statusCode": 200,
  "message": "C?p nh?t holiday tour template thành công và ?ã c?p nh?t slot cho ngày 01/05/2025 (Th? n?m)",
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Tour Núi Bà ?en - Ngày L? 30/4 (C?p nh?t)",
    "templateType": "PaidAttraction",
    "scheduleDays": "Thursday",
    "startLocation": "TP.HCM",
    "endLocation": "Tây Ninh",
    "month": 5,
    "year": 2025,
    "isActive": true,
    "updatedAt": "2025-01-15T14:30:00Z"
  }
}#### Error Responses

**Status Code**: `400 Bad Request` - Validation Error{
  "statusCode": 400,
  "message": "D? li?u c?p nh?t không h?p l? - Vui lòng ki?m tra và s?a các l?i sau",
  "success": false,
  "validationErrors": [
    "tourDate: Ngày tour ph?i sau ít nh?t 30 ngày t? ngày t?o template (15/01/2025). Ngày s?m nh?t có th?: 14/02/2025.",
    "?? H??NG D?N C?P NH?T HOLIDAY TEMPLATE:",
    "• Template ???c t?o: 15/01/2025",
    "• Ngày s?m nh?t cho tourDate: 14/02/2025 (sau 30 ngày t? ngày t?o)",
    "• Ngày mu?n nh?t cho tourDate: 15/01/2027 (t?i ?a 2 n?m)",
    "• ? ??C BI?T: Holiday template có th? ch?n b?t k? ngày nào trong tu?n",
    "• ?? Ch? g?i fields mu?n thay ??i, ?? null cho fields không thay ??i"
  },
  "fieldErrors" : {
    "tourDate": ["Ngày tour ph?i sau ít nh?t 30 ngày t? ngày t?o template (15/01/2025)"]
  }
}**Status Code**: `404 Not Found`{
  "statusCode": 404,
  "message": "Không tìm th?y holiday tour template"
}**Status Code**: `409 Conflict` - Template ?ang ???c s? d?ng{
  "statusCode": 409,
  "message": "Không th? c?p nh?t tour template vì có tour details ?ang s? d?ng",
  "success": false,
  "validationErrors": [
    "Có 1 tour details ?ã ???c t?o s? d?ng template này",
    "Vi?c c?p nh?t template có th? ?nh h??ng ??n tour details ?ã ???c t?o",
    "Vui lòng xóa t?t c? tour details liên quan tr??c khi c?p nh?t template",
    "Ho?c chuy?n các tour details sang s? d?ng template khác"
  ]
}
## So sánh v?i Tour Template th??ng th??ng

| Aspect | Regular Template | Holiday Template |
|--------|------------------|------------------|
| **Input** | Month, Year, ScheduleDays | TourDate |
| **Validator** | TourTemplateScheduleValidator | **HolidayTourTemplateValidator** |
| **Slots Created** | 4 slots/tháng theo schedule | 1 slot cho ngày c? th? |
| **Use Case** | Tour ??nh k? hàng tu?n | Tour ??c bi?t cho ngày l? |
| **ScheduleDays** | User ch?n (Saturday/Sunday) | T? ??ng t? tourDate |
| **30-day Rule** | ? Áp d?ng | ? Áp d?ng |
| **Year Range** | ? 2024-2030 | ? 2024-2030 |
| **Weekend Only** | ? Saturday/Sunday | ? **B?t k? ngày nào** |
| **Validation Logic** | `ValidateScheduleDay()` v?i restrict | `ValidateHolidayBusinessRules()` không restrict |
| **Create Endpoint** | `POST /template` | `POST /template/holiday` |
| **Update Endpoint** | `PATCH /template/{id}` | `PATCH /template/holiday/{id}` |
| **Update Validation** | TourTemplateValidator | **HolidayTourTemplateValidator** |

## API Endpoints Hoàn Ch?nh

### ?? Holiday Template Endpoints
1. **Create**: `POST /api/TourCompany/template/holiday`
2. **Update**: `PATCH /api/TourCompany/template/holiday/{id}` ? M?I
3. **Get by ID**: `GET /api/TourCompany/template/{id}` (dùng chung)
4. **Delete**: `DELETE /api/TourCompany/template/{id}` (dùng chung)
5. **Get List**: `GET /api/TourCompany/template` (dùng chung)

### ?? Regular Template Endpoints  
1. **Create**: `POST /api/TourCompany/template`
2. **Update**: `PATCH /api/TourCompany/template/{id}`
3. **Get by ID**: `GET /api/TourCompany/template/{id}`
4. **Delete**: `DELETE /api/TourCompany/template/{id}`
5. **Get List**: `GET /api/TourCompany/template`

### ? Ví d? c?p nh?t thành công// Ch? thay ??i title và ngày tour
{
  "title": "Tour Tây Ninh - T?t D??ng L?ch 2025 (Updated)",
  "tourDate": "2025-03-15"  // Th? 7 c?ng OK!
}
### ? Ví d? c?p nh?t ch? m?t s? field// Ch? thay ??i lo?i template
{
  "templateType": "PaidAttraction"
}
### ? Ví d? c?p nh?t tour sang ngày gi?a tu?n{
  "title": "Tour L? Ph?t ??n",
  "tourDate": "2025-04-13",  // ? Ch? nh?t
  "endLocation": "Chùa Cao ?ài"
}
### ? Ví d? l?i c?p nh?t ngày quá s?m// Template ???c t?o ngày 15/01/2025
{
  "tourDate": "2025-02-01"  // ? Ch? sau 17 ngày, c?n sau 30 ngày
}

### ?? Ví d? l?i khi template có tour details// Template ID: holiday-template-123 ?ã có tour details s? d?ng
PATCH /api/TourCompany/template/holiday/holiday-template-123
{
  "title": "Updated Holiday Template",
  "tourDate": "2025-12-25"
}

// Response:
{
  "statusCode": 409,
  "message": "Không th? c?p nh?t tour template vì có tour details ?ang s? d?ng",
  "success": false,
  "validationErrors": [
    "Có 2 tour details ?ã ???c t?o s? d?ng template này",
    "Trong ?ó có 1 tour details ?ang ? tr?ng thái Public",
    "Có 3 booking ?ã ???c khách hàng xác nh?n",
    "Có 1 tour details ?ang ? tr?ng thái Draft/WaitToPublic",
    "Vi?c c?p nh?t template có th? ?nh h??ng ??n các tour details ?ã ???c t?o",
    "Vui lòng xóa t?t c? tour details liên quan tr??c khi c?p nh?t template",
    "Ho?c chuy?n các tour details sang s? d?ng template khác"
  ]
}
### ? Ví d? thành công khi template ch? có slots// Template ID: holiday-template-456 ch? có tour slots, ch?a có tour details
PATCH /api/TourCompany/template/holiday/holiday-template-456
{
  "title": "Updated Holiday Template",
  "tourDate": "2025-12-25"
}

// Response:
{
  "statusCode": 200,
  "message": "C?p nh?t holiday tour template thành công và ?ã c?p nh?t slot cho ngày 25/12/2025 (Th? n?m)",
  "success": true,
  "data": {
    "id": "holiday-template-456",
    "title": "Updated Holiday Template",
    "scheduleDays": "Thursday",
    "month": 12,
    "year": 2025,
    "updatedAt": "2025-01-15T16:00:00Z"
  }
}
## ?? Quy T?c B?o V? D? Li?u

### ?? **Không th? UPDATE Holiday Template khi:**
1. **Có b?t k? TourDetails nào** ???c t?o t? template này (b?t k? status nào: Draft, WaitToPublic, Public)
2. **Có bookings** liên quan ??n tour details ?ó (Confirmed, Pending, ho?c b?t k? status nào)
3. **Có operations** ?ang active s? d?ng template này

### ? **Có th? UPDATE Holiday Template khi:**
1. **Ch? có tour slots** (d? li?u ph? tr?, không ?nh h??ng)
2. **Ch?a có tour details nào** ???c t?o t? template
3. **Template m?i t?o** và ch?a ???c s? d?ng

### ?? **Cách ki?m tra tr??c khi update:**
# Ki?m tra template có th? update không
GET /api/TourCompany/template/{id}/can-update

# Response n?u có th? update:
{
  "statusCode": 200,
  "message": "Có th? c?p nh?t tour template",
  "canUpdate": true,
  "reason": "Template này ch? có tour slots và có th? c?p nh?t an toàn"
}

# Response n?u KHÔNG th? update:
{
  "statusCode": 409,
  "message": "Không th? c?p nh?t tour template vì có tour details ?ang s? d?ng",
  "canUpdate": false,
  "reason": "Tour template này ?ang ???c s? d?ng b?i các tour details và không th? c?p nh?t",
  "blockingReasons": [
    "Có 2 tour details ?ã ???c t?o s? d?ng template này",
    "Trong ?ó có 1 tour details ?ang ? tr?ng thái Public",
    "Có 5 booking ?ã ???c khách hàng xác nh?n"
  ]
}
## ?? H??ng D?n X? Lý Conflict

### Khi g?p l?i 409 - Có tour details ?ang s? d?ng:

#### **Ph??ng án 1: Xóa tour details**# 1. L?y danh sách tour details s? d?ng template
GET /api/TourDetails/by-template/{templateId}

# 2. Xóa t?ng tour detail (n?u ch?a có bookings)
DELETE /api/TourDetails/{tourDetailId}

# 3. Sau ?ó m?i update holiday template
PATCH /api/TourCompany/template/holiday/{id}
#### **Ph??ng án 2: Chuy?n tour details sang template khác**# 1. T?o template m?i
POST /api/TourCompany/template/holiday

# 2. Chuy?n tour details sang template m?i
PATCH /api/TourDetails/{tourDetailId}
{
  "tourTemplateId": "new-template-id"
}

# 3. Update template c?
PATCH /api/TourCompany/template/holiday/{oldTemplateId}
#### **Ph??ng án 3: T?o template m?i thay th?**# 1. Copy template hi?n t?i
POST /api/TourCompany/template/{id}/copy
{
  "newTitle": "Holiday Template v2"
}

# 2. Update template m?i
PATCH /api/TourCompany/template/holiday/{newTemplateId}

# 3. S? d?ng template m?i cho các tour details m?i
## Validation Flow cho Holiday Template Update

### ?? Quy Trình Ki?m Tra (Theo th? t?)

1. **?? Ki?m tra Template t?n t?i**
   - Template có t?n t?i không?
   - Template có b? xóa (soft delete) không?

2. **?? Ki?m tra Quy?n (Permission)**
   - User có quy?n update template này không?
   - Template có ph?i do user này t?o không?

3. **?? Ki?m tra Blocking Conditions - QUAN TR?NG NH?T**// Ki?m tra có tour details ?ang s? d?ng template không
var existingTourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
    td => td.TourTemplateId == id && !td.IsDeleted);

if (existingTourDetails.Any()) {
    // NG?N C?N UPDATE - B?t k? tour details nào c?ng block
       return 409 Conflict;
   }
4. **?? Ki?m tra Input Validation**
   - S? d?ng `HolidayTourTemplateValidator.ValidateUpdateRequest()`
   - Cho phép b?t k? ngày nào trong tu?n
   - Áp d?ng quy t?c 30 ngày t? ngày t?o template

5. **??? Ki?m tra Images (n?u có)**
   - Validate image URLs
   - Check image accessibility

6. **? Apply Updates**
   - C?p nh?t template fields
   - C?p nh?t tour slot t??ng ?ng (n?u thay ??i ngày)
   - C?p nh?t audit fields

### ?? ?i?m Khác Bi?t v?i Regular Template

| Aspect | Regular Template Update | Holiday Template Update |
|--------|------------------------|------------------------|
| **Validation Step 3** | Dùng `CanUpdateTourTemplateAsync()` | **Dùng cùng method** ? |
| **Validation Step 4** | `TourTemplateValidator.ValidateUpdateRequest()` | **`HolidayTourTemplateValidator.ValidateUpdateRequest()`** ? |
| **Schedule Days** | Ch? Saturday/Sunday | **B?t k? ngày nào** ? |
| **tourDate Field** | Không có | **Có th? update** ? |
| **Slot Update** | Không c?p nh?t slot | **T? ??ng c?p nh?t slot** ? |

### ?? B?o M?t & Tính Nh?t Quán

- **Cùng logic b?o v? d? li?u**: C? regular và holiday template ??u dùng chung `CanUpdateTourTemplateAsync()`
- **Ng?n c?n update**: Khi có b?t k? tour details nào (Draft, WaitToPublic, Public)
- **Lý do**: Tránh ?nh h??ng ??n tour details và bookings ?ã t?n t?i
- **Solution**: Ph?i xóa ho?c chuy?n tour details tr??c khi update template