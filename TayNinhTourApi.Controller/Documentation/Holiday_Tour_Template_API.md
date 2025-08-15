# Holiday Tour Template API Documentation

## T?ng quan

API m?i ???c t?o ?? h? tr? vi?c t?o tour template cho c�c ng�y l? ??c bi?t. Kh�c v?i tour template th??ng th??ng (t?o slots cho c? th�ng d?a tr�n schedule days), holiday template cho ph�p t?o 1 slot duy nh?t cho ng�y c? th? ???c ch?n.

**?? VALIDATION QUAN TR?NG**: Holiday template �p d?ng **c�ng quy t?c 30 ng�y** nh? tour template b�nh th??ng ?? ??m b?o Tour Company c� ?? th?i gian chu?n b?.

**? ??C BI?T M?I**: Holiday template s? d?ng validator ri�ng `HolidayTourTemplateValidator` cho ph�p ch?n **b?t k? ng�y n�o trong tu?n** (Monday-Sunday), kh�ng b? gi?i h?n Saturday/Sunday nh? regular template.

## Endpoint M?i

### T?o Holiday Tour Template
POST /api/TourCompany/template/holiday
**Authorization**: Bearer Token (Role: "Tour Company")

#### Request Body{
  "title": "Tour N�i B� ?en - Ng�y L? 30/4",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
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
  "title": "Tour N�i B� ?en - Ng�y L? 30/4 (C?p nh?t)",
  "tourDate": "2025-05-01",
  "templateType": "PaidAttraction"
}#### Request Body Schema

| Field | Type | Required | Constraints | Description |
|-------|------|----------|-------------|-------------|
| `title` | string | No | 1-200 chars | T�n tour template m?i |
| `startLocation` | string | No | 1-500 chars | ?i?m b?t ??u m?i |
| `endLocation` | string | No | 1-500 chars | ?i?m k?t th�c m?i |
| `templateType` | string | No | `FreeScenic`, `PaidAttraction` | Lo?i tour template m?i |
| `tourDate` | DateOnly | No | **Ph?i sau �t nh?t 30 ng�y t? ng�y t?o template** | Ng�y tour m?i |
| `images` | array[string] | No | max 10 items | Danh s�ch URL ?nh m?i |

**?? L?u � quan tr?ng**:
- Ch? g?i c�c fields mu?n thay ??i (partial update)
- Fields kh�ng g?i s? gi? nguy�n gi� tr? c?
- N?u c?p nh?t `tourDate`, slot c? s? ???c c?p nh?t v?i ng�y m?i
- `tourDate` m?i v?n ph?i tu�n th? quy t?c 30 ng�y t? ng�y t?o template

#### Response

**Status Code**: `200 OK`{
  "statusCode": 200,
  "message": "C?p nh?t holiday tour template th�nh c�ng v� ?� c?p nh?t slot cho ng�y 01/05/2025 (Th? n?m)",
  "success": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "title": "Tour N�i B� ?en - Ng�y L? 30/4 (C?p nh?t)",
    "templateType": "PaidAttraction",
    "scheduleDays": "Thursday",
    "startLocation": "TP.HCM",
    "endLocation": "T�y Ninh",
    "month": 5,
    "year": 2025,
    "isActive": true,
    "updatedAt": "2025-01-15T14:30:00Z"
  }
}#### Error Responses

**Status Code**: `400 Bad Request` - Validation Error{
  "statusCode": 400,
  "message": "D? li?u c?p nh?t kh�ng h?p l? - Vui l�ng ki?m tra v� s?a c�c l?i sau",
  "success": false,
  "validationErrors": [
    "tourDate: Ng�y tour ph?i sau �t nh?t 30 ng�y t? ng�y t?o template (15/01/2025). Ng�y s?m nh?t c� th?: 14/02/2025.",
    "?? H??NG D?N C?P NH?T HOLIDAY TEMPLATE:",
    "� Template ???c t?o: 15/01/2025",
    "� Ng�y s?m nh?t cho tourDate: 14/02/2025 (sau 30 ng�y t? ng�y t?o)",
    "� Ng�y mu?n nh?t cho tourDate: 15/01/2027 (t?i ?a 2 n?m)",
    "� ? ??C BI?T: Holiday template c� th? ch?n b?t k? ng�y n�o trong tu?n",
    "� ?? Ch? g?i fields mu?n thay ??i, ?? null cho fields kh�ng thay ??i"
  },
  "fieldErrors" : {
    "tourDate": ["Ng�y tour ph?i sau �t nh?t 30 ng�y t? ng�y t?o template (15/01/2025)"]
  }
}**Status Code**: `404 Not Found`{
  "statusCode": 404,
  "message": "Kh�ng t�m th?y holiday tour template"
}**Status Code**: `409 Conflict` - Template ?ang ???c s? d?ng{
  "statusCode": 409,
  "message": "Kh�ng th? c?p nh?t tour template v� c� tour details ?ang s? d?ng",
  "success": false,
  "validationErrors": [
    "C� 1 tour details ?� ???c t?o s? d?ng template n�y",
    "Vi?c c?p nh?t template c� th? ?nh h??ng ??n tour details ?� ???c t?o",
    "Vui l�ng x�a t?t c? tour details li�n quan tr??c khi c?p nh?t template",
    "Ho?c chuy?n c�c tour details sang s? d?ng template kh�c"
  ]
}
## So s�nh v?i Tour Template th??ng th??ng

| Aspect | Regular Template | Holiday Template |
|--------|------------------|------------------|
| **Input** | Month, Year, ScheduleDays | TourDate |
| **Validator** | TourTemplateScheduleValidator | **HolidayTourTemplateValidator** |
| **Slots Created** | 4 slots/th�ng theo schedule | 1 slot cho ng�y c? th? |
| **Use Case** | Tour ??nh k? h�ng tu?n | Tour ??c bi?t cho ng�y l? |
| **ScheduleDays** | User ch?n (Saturday/Sunday) | T? ??ng t? tourDate |
| **30-day Rule** | ? �p d?ng | ? �p d?ng |
| **Year Range** | ? 2024-2030 | ? 2024-2030 |
| **Weekend Only** | ? Saturday/Sunday | ? **B?t k? ng�y n�o** |
| **Validation Logic** | `ValidateScheduleDay()` v?i restrict | `ValidateHolidayBusinessRules()` kh�ng restrict |
| **Create Endpoint** | `POST /template` | `POST /template/holiday` |
| **Update Endpoint** | `PATCH /template/{id}` | `PATCH /template/holiday/{id}` |
| **Update Validation** | TourTemplateValidator | **HolidayTourTemplateValidator** |

## API Endpoints Ho�n Ch?nh

### ?? Holiday Template Endpoints
1. **Create**: `POST /api/TourCompany/template/holiday`
2. **Update**: `PATCH /api/TourCompany/template/holiday/{id}` ? M?I
3. **Get by ID**: `GET /api/TourCompany/template/{id}` (d�ng chung)
4. **Delete**: `DELETE /api/TourCompany/template/{id}` (d�ng chung)
5. **Get List**: `GET /api/TourCompany/template` (d�ng chung)

### ?? Regular Template Endpoints  
1. **Create**: `POST /api/TourCompany/template`
2. **Update**: `PATCH /api/TourCompany/template/{id}`
3. **Get by ID**: `GET /api/TourCompany/template/{id}`
4. **Delete**: `DELETE /api/TourCompany/template/{id}`
5. **Get List**: `GET /api/TourCompany/template`

### ? V� d? c?p nh?t th�nh c�ng// Ch? thay ??i title v� ng�y tour
{
  "title": "Tour T�y Ninh - T?t D??ng L?ch 2025 (Updated)",
  "tourDate": "2025-03-15"  // Th? 7 c?ng OK!
}
### ? V� d? c?p nh?t ch? m?t s? field// Ch? thay ??i lo?i template
{
  "templateType": "PaidAttraction"
}
### ? V� d? c?p nh?t tour sang ng�y gi?a tu?n{
  "title": "Tour L? Ph?t ??n",
  "tourDate": "2025-04-13",  // ? Ch? nh?t
  "endLocation": "Ch�a Cao ?�i"
}
### ? V� d? l?i c?p nh?t ng�y qu� s?m// Template ???c t?o ng�y 15/01/2025
{
  "tourDate": "2025-02-01"  // ? Ch? sau 17 ng�y, c?n sau 30 ng�y
}

### ?? V� d? l?i khi template c� tour details// Template ID: holiday-template-123 ?� c� tour details s? d?ng
PATCH /api/TourCompany/template/holiday/holiday-template-123
{
  "title": "Updated Holiday Template",
  "tourDate": "2025-12-25"
}

// Response:
{
  "statusCode": 409,
  "message": "Kh�ng th? c?p nh?t tour template v� c� tour details ?ang s? d?ng",
  "success": false,
  "validationErrors": [
    "C� 2 tour details ?� ???c t?o s? d?ng template n�y",
    "Trong ?� c� 1 tour details ?ang ? tr?ng th�i Public",
    "C� 3 booking ?� ???c kh�ch h�ng x�c nh?n",
    "C� 1 tour details ?ang ? tr?ng th�i Draft/WaitToPublic",
    "Vi?c c?p nh?t template c� th? ?nh h??ng ??n c�c tour details ?� ???c t?o",
    "Vui l�ng x�a t?t c? tour details li�n quan tr??c khi c?p nh?t template",
    "Ho?c chuy?n c�c tour details sang s? d?ng template kh�c"
  ]
}
### ? V� d? th�nh c�ng khi template ch? c� slots// Template ID: holiday-template-456 ch? c� tour slots, ch?a c� tour details
PATCH /api/TourCompany/template/holiday/holiday-template-456
{
  "title": "Updated Holiday Template",
  "tourDate": "2025-12-25"
}

// Response:
{
  "statusCode": 200,
  "message": "C?p nh?t holiday tour template th�nh c�ng v� ?� c?p nh?t slot cho ng�y 25/12/2025 (Th? n?m)",
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

### ?? **Kh�ng th? UPDATE Holiday Template khi:**
1. **C� b?t k? TourDetails n�o** ???c t?o t? template n�y (b?t k? status n�o: Draft, WaitToPublic, Public)
2. **C� bookings** li�n quan ??n tour details ?� (Confirmed, Pending, ho?c b?t k? status n�o)
3. **C� operations** ?ang active s? d?ng template n�y

### ? **C� th? UPDATE Holiday Template khi:**
1. **Ch? c� tour slots** (d? li?u ph? tr?, kh�ng ?nh h??ng)
2. **Ch?a c� tour details n�o** ???c t?o t? template
3. **Template m?i t?o** v� ch?a ???c s? d?ng

### ?? **C�ch ki?m tra tr??c khi update:**
# Ki?m tra template c� th? update kh�ng
GET /api/TourCompany/template/{id}/can-update

# Response n?u c� th? update:
{
  "statusCode": 200,
  "message": "C� th? c?p nh?t tour template",
  "canUpdate": true,
  "reason": "Template n�y ch? c� tour slots v� c� th? c?p nh?t an to�n"
}

# Response n?u KH�NG th? update:
{
  "statusCode": 409,
  "message": "Kh�ng th? c?p nh?t tour template v� c� tour details ?ang s? d?ng",
  "canUpdate": false,
  "reason": "Tour template n�y ?ang ???c s? d?ng b?i c�c tour details v� kh�ng th? c?p nh?t",
  "blockingReasons": [
    "C� 2 tour details ?� ???c t?o s? d?ng template n�y",
    "Trong ?� c� 1 tour details ?ang ? tr?ng th�i Public",
    "C� 5 booking ?� ???c kh�ch h�ng x�c nh?n"
  ]
}
## ?? H??ng D?n X? L� Conflict

### Khi g?p l?i 409 - C� tour details ?ang s? d?ng:

#### **Ph??ng �n 1: X�a tour details**# 1. L?y danh s�ch tour details s? d?ng template
GET /api/TourDetails/by-template/{templateId}

# 2. X�a t?ng tour detail (n?u ch?a c� bookings)
DELETE /api/TourDetails/{tourDetailId}

# 3. Sau ?� m?i update holiday template
PATCH /api/TourCompany/template/holiday/{id}
#### **Ph??ng �n 2: Chuy?n tour details sang template kh�c**# 1. T?o template m?i
POST /api/TourCompany/template/holiday

# 2. Chuy?n tour details sang template m?i
PATCH /api/TourDetails/{tourDetailId}
{
  "tourTemplateId": "new-template-id"
}

# 3. Update template c?
PATCH /api/TourCompany/template/holiday/{oldTemplateId}
#### **Ph??ng �n 3: T?o template m?i thay th?**# 1. Copy template hi?n t?i
POST /api/TourCompany/template/{id}/copy
{
  "newTitle": "Holiday Template v2"
}

# 2. Update template m?i
PATCH /api/TourCompany/template/holiday/{newTemplateId}

# 3. S? d?ng template m?i cho c�c tour details m?i
## Validation Flow cho Holiday Template Update

### ?? Quy Tr�nh Ki?m Tra (Theo th? t?)

1. **?? Ki?m tra Template t?n t?i**
   - Template c� t?n t?i kh�ng?
   - Template c� b? x�a (soft delete) kh�ng?

2. **?? Ki?m tra Quy?n (Permission)**
   - User c� quy?n update template n�y kh�ng?
   - Template c� ph?i do user n�y t?o kh�ng?

3. **?? Ki?m tra Blocking Conditions - QUAN TR?NG NH?T**// Ki?m tra c� tour details ?ang s? d?ng template kh�ng
var existingTourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
    td => td.TourTemplateId == id && !td.IsDeleted);

if (existingTourDetails.Any()) {
    // NG?N C?N UPDATE - B?t k? tour details n�o c?ng block
       return 409 Conflict;
   }
4. **?? Ki?m tra Input Validation**
   - S? d?ng `HolidayTourTemplateValidator.ValidateUpdateRequest()`
   - Cho ph�p b?t k? ng�y n�o trong tu?n
   - �p d?ng quy t?c 30 ng�y t? ng�y t?o template

5. **??? Ki?m tra Images (n?u c�)**
   - Validate image URLs
   - Check image accessibility

6. **? Apply Updates**
   - C?p nh?t template fields
   - C?p nh?t tour slot t??ng ?ng (n?u thay ??i ng�y)
   - C?p nh?t audit fields

### ?? ?i?m Kh�c Bi?t v?i Regular Template

| Aspect | Regular Template Update | Holiday Template Update |
|--------|------------------------|------------------------|
| **Validation Step 3** | D�ng `CanUpdateTourTemplateAsync()` | **D�ng c�ng method** ? |
| **Validation Step 4** | `TourTemplateValidator.ValidateUpdateRequest()` | **`HolidayTourTemplateValidator.ValidateUpdateRequest()`** ? |
| **Schedule Days** | Ch? Saturday/Sunday | **B?t k? ng�y n�o** ? |
| **tourDate Field** | Kh�ng c� | **C� th? update** ? |
| **Slot Update** | Kh�ng c?p nh?t slot | **T? ??ng c?p nh?t slot** ? |

### ?? B?o M?t & T�nh Nh?t Qu�n

- **C�ng logic b?o v? d? li?u**: C? regular v� holiday template ??u d�ng chung `CanUpdateTourTemplateAsync()`
- **Ng?n c?n update**: Khi c� b?t k? tour details n�o (Draft, WaitToPublic, Public)
- **L� do**: Tr�nh ?nh h??ng ??n tour details v� bookings ?� t?n t?i
- **Solution**: Ph?i x�a ho?c chuy?n tour details tr??c khi update template