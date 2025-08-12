# Tour Template Delete & Update Logic Update

## Summary

?� c?p nh?t logic x�a v� c?p nh?t tour template theo y�u c?u:
- **Tr??c ?�y (Delete)**: Ng?n c?n x�a khi c� tour slots ho?c tour details
- **Hi?n t?i (Delete)**: Ch? ng?n c?n x�a khi c� tour details ???c t?o s? d?ng template ?�
- **Tr??c ?�y (Update)**: Ch? ng?n c?n c?p nh?t khi c� tour details ? tr?ng th�i Public
- **Hi?n t?i (Update)**: Ng?n c?n c?p nh?t khi c� b?t k? tour details n�o ???c t?o s? d?ng template ?�

## Changes Made

### 1. Enhanced Tour Template Service
**File**: `TayNinhTourApi.BusinessLogicLayer/Services/EnhancedTourTemplateService.cs`

#### Method Updated: `CanDeleteTourTemplateAsync(Guid id)` ?

**Logic m?i**:// NEW LOGIC: Ch? ng?n c?n x�a khi c� tour details ???c t?o s? d?ng template n�y
// Kh�ng quan t�m ??n tour slots n?a - tour slots ch? l� d? li?u ph? tr?

// Ki?m tra xem c� tour details n�o ???c t?o t? template n�y kh�ng
var existingTourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
    td => td.TourTemplateId == id && !td.IsDeleted);

if (existingTourDetails.Any())
{
    // Ng?n c?n x�a v� ??a ra th�ng tin chi ti?t
    // - S? l??ng tour details
    // - Ph�n t�ch Public vs Draft
    // - S? l??ng bookings n?u c�
    // - H??ng d?n gi?i quy?t
}

// NOTE: Kh�ng c�n ki?m tra tour slots n?a
// Tour slots l� d? li?u ph? tr? v� c� th? t?n t?i m� kh�ng ?nh h??ng ??n vi?c x�a template
#### Method Updated: `CanUpdateTourTemplateAsync(Guid id)` ?

**Logic m?i**:// NEW LOGIC: Ng?n c?n c?p nh?t khi c� b?t k? tour details n�o ???c t?o s? d?ng template n�y
// Kh�ng ch? ri�ng Public nh? tr??c ?�y - b?t k? tr?ng th�i n�o c?ng ng?n c?n update

// Ki?m tra xem c� tour details n�o ???c t?o t? template n�y kh�ng
var existingTourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
    td => td.TourTemplateId == id && !td.IsDeleted);

if (existingTourDetails.Any())
{
    // Ng?n c?n c?p nh?t v� ??a ra th�ng tin chi ti?t
    // - S? l??ng tour details (b?t k? tr?ng th�i n�o)
    // - Ph�n t�ch Public vs Draft 
    // - S? l??ng bookings n?u c�
    // - H??ng d?n gi?i quy?t
}

// NOTE: Kh�ng c�n ki?m tra tour slots n?a
// Tour slots l� d? li?u ph? tr? v� kh�ng ng?n c?n vi?c c?p nh?t template
### 2. Legacy Tour Template Service
**File**: `TayNinhTourApi.BusinessLogicLayer/Services/TourTemplateService.cs`

#### Method Updated: `CanDeleteTourTemplateAsync(Guid id)` ?

**Logic m?i**:public async Task<bool> CanDeleteTourTemplateAsync(Guid id)
{
    // NEW LOGIC: Ch? ng?n c?n x�a khi c� tour details ???c t?o s? d?ng template n�y
    // Kh�ng quan t�m ??n tour slots n?a - tour slots ch? l� d? li?u ph? tr?
    
    // Ki?m tra xem c� tour details n�o ???c t?o t? template n�y kh�ng
    var existingTourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
        td => td.TourTemplateId == id && !td.IsDeleted);
    
    // Ch? ng?n c?n x�a n?u c� tour details ???c t?o t? template n�y
    // Tour slots kh�ng c�n l� y?u t? ng?n c?n x�a
    return !existingTourDetails.Any();
}
#### Method Updated: `UpdateTourTemplateAsync(Guid id, RequestUpdateTourTemplateDto request, Guid updatedById)` ?

**Logic m?i**:public async Task<TourTemplate?> UpdateTourTemplateAsync(Guid id, RequestUpdateTourTemplateDto request, Guid updatedById)
{
    var existingTemplate = await _unitOfWork.TourTemplateRepository.GetByIdAsync(id, null);
    if (existingTemplate == null || existingTemplate.IsDeleted)
    {
        return null;
    }

    // NEW LOGIC: Check if template has tour details - prevent update if it does
    var existingTourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
        td => td.TourTemplateId == id && !td.IsDeleted);
    
    if (existingTourDetails.Any())
    {
        // Cannot update template that has tour details
        throw new InvalidOperationException($"Kh�ng th? c?p nh?t tour template v� c� {existingTourDetails.Count()} tour details ?ang s? d?ng template n�y");
    }
    
    // Continue with update...
}
### 3. API Documentation Update
**File**: `TayNinhTourApi.Controller/Documentation/TourTemplate_API_Documentation.md`

**C?p nh?t**:
- Th�m th�ng tin v? logic m?i cho c? Delete v� Update
- C?p nh?t v� d? response cho Update endpoint
- Gi?i th�ch r� r�ng khi n�o c� th? c?p nh?t vs kh�ng th? c?p nh?t

## Business Logic

### DELETE - Tr??ng h?p C� TH? X�A ?
1. Template ch? c� tour slots (kh�ng c� tour details n�o ???c t?o)
2. Tour slots l� d? li?u ph? tr? v� kh�ng ng?n c?n vi?c x�a template

### DELETE - Tr??ng h?p KH�NG TH? X�A ?
1. C� tour details ???c t?o t? template n�y (b?t k? tr?ng th�i n�o)
2. Chi ti?t ph�n t�ch:
   - **Public tour details**: C� th? c� bookings t? kh�ch h�ng
   - **Draft/WaitToPublic tour details**: ?ang ???c chu?n b?
   - **Bookings**: Confirmed ho?c Pending t? kh�ch h�ng

### UPDATE - Tr??ng h?p C� TH? C?P NH?T ? 
1. Template ch? c� tour slots (kh�ng c� tour details n�o ???c t?o)
2. Tour slots l� d? li?u ph? tr? v� kh�ng ng?n c?n vi?c c?p nh?t template

### UPDATE - Tr??ng h?p KH�NG TH? C?P NH?T ?
1. C� b?t k? tour details n�o ???c t?o t? template n�y (m?i tr?ng th�i)
2. L� do: C?p nh?t template c� th? ?nh h??ng ??n c�c tour details ?� ???c t?o
3. Chi ti?t ph�n t�ch:
   - **Public tour details**: ?nh h??ng tr?c ti?p ??n kh�ch h�ng ?� booking
   - **Draft tour details**: ?nh h??ng ??n c�ng vi?c ?ang chu?n b?
   - **Bookings**: C� th? g�y inconsistency v?i d? li?u ?� c�

### Blocking Reasons Details
Khi kh�ng th? x�a/c?p nh?t, h? th?ng s? ??a ra th�ng tin chi ti?t:

#### Delete Response:{
  "blockingReasons": [
    "C� 2 tour details ?� ???c t?o s? d?ng template n�y",
    "Trong ?� c� 1 tour details ?ang ? tr?ng th�i Public", 
    "C� 5 booking ?� ???c kh�ch h�ng x�c nh?n",
    "C� 1 tour details ?ang ? tr?ng th�i Draft/WaitToPublic",
    "Vui l�ng x�a t?t c? tour details li�n quan tr??c khi x�a template",
    "Ho?c chuy?n c�c tour details sang s? d?ng template kh�c"
  ]
}
#### Update Response:{
  "validationErrors": [
    "C� 2 tour details ?� ???c t?o s? d?ng template n�y",
    "Trong ?� c� 1 tour details ?ang ? tr?ng th�i Public",
    "C� 5 booking ?� ???c kh�ch h�ng x�c nh?n",
    "C� 1 tour details ?ang ? tr?ng th�i Draft/WaitToPublic",
    "Vi?c c?p nh?t template c� th? ?nh h??ng ??n c�c tour details ?� ???c t?o",
    "Vui l�ng x�a t?t c? tour details li�n quan tr??c khi c?p nh?t template",
    "Ho?c chuy?n c�c tour details sang s? d?ng template kh�c"
  ]
}
## API Response Examples

### Delete

#### Th�nh c�ng - Ch? c� tour slots{
  "statusCode": 200,
  "message": "X�a tour template th�nh c�ng", 
  "success": true,
  "reason": "Template n�y ch? c� tour slots v� c� th? x�a an to�n"
}
#### Th?t b?i - C� tour details{
  "statusCode": 409,
  "message": "Kh�ng th? x�a tour template v� c� tour details ?ang s? d?ng",
  "success": false,
  "reason": "Tour template n�y ?ang ???c s? d?ng b?i c�c tour details v� kh�ng th? x�a",
  "blockingReasons": [...]
}
### Update

#### Th�nh c�ng - Ch? c� tour slots{
  "statusCode": 200,
  "message": "C?p nh?t tour template th�nh c�ng",
  "success": true,
  "reason": "Template n�y ch? c� tour slots v� c� th? c?p nh?t an to�n"
}
#### Th?t b?i - C� tour details{
  "statusCode": 409,
  "message": "Kh�ng th? c?p nh?t tour template v� c� tour details ?ang s? d?ng",
  "success": false,
  "validationErrors": [...]
}
## Testing Scenarios

### Test Case 1: Template v?i ch? tour slots
- T?o tour template
- Generate tour slots
- Th? x�a template ? **Th�nh c�ng** ?
- Th? c?p nh?t template ? **Th�nh c�ng** ?

### Test Case 2: Template v?i tour details (Draft)
- T?o tour template  
- Generate tour slots
- T?o tour details t? template
- Th? x�a template ? **Th?t b?i** ?
- Th? c?p nh?t template ? **Th?t b?i** ?

### Test Case 3: Template v?i tour details (Public + Bookings)
- T?o tour template
- Generate tour slots  
- T?o tour details t? template
- Publish tour details
- C� bookings t? kh�ch h�ng
- Th? x�a template ? **Th?t b?i v?i th�ng tin chi ti?t** ?
- Th? c?p nh?t template ? **Th?t b?i v?i th�ng tin chi ti?t** ?

## Key Changes Summary

### DELETE Logic:
- ? **Before**: Blocked by tour slots OR tour details
- ? **After**: Blocked ONLY by tour details (any status)

### UPDATE Logic:  
- ? **Before**: Blocked ONLY by PUBLIC tour details
- ? **After**: Blocked by ANY tour details (any status)

### Consistency:
- ? Both Enhanced and Legacy services updated
- ? Same logic applied to both DELETE and UPDATE
- ? Tour slots no longer block operations
- ? Only actual tour usage (tour details) blocks operations

## Notes

1. **Tour Slots**: Kh�ng c�n ng?n c?n vi?c x�a/c?p nh?t template
2. **Tour Details**: L� y?u t? quy?t ??nh vi?c c� th? x�a/c?p nh?t template hay kh�ng  
3. **Backward Compatibility**: C? Enhanced v� Legacy service ??u ???c c?p nh?t
4. **Error Messages**: ??y ?? v� h??ng d?n r� r�ng c�ch gi?i quy?t
5. **Build**: ?� test v� build th�nh c�ng

## Implementation Status

- [x] Enhanced Tour Template Service DELETE updated
- [x] Enhanced Tour Template Service UPDATE updated
- [x] Legacy Tour Template Service DELETE updated  
- [x] Legacy Tour Template Service UPDATE updated
- [x] API Documentation updated
- [x] Build verification successful
- [x] Logic tested and validated

**Ng�y c?p nh?t**: 2024-12-21
**Ng??i th?c hi?n**: GitHub Copilot