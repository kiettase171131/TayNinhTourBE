# Tour Template Delete & Update Logic Update

## Summary

?ã c?p nh?t logic xóa và c?p nh?t tour template theo yêu c?u:
- **Tr??c ?ây (Delete)**: Ng?n c?n xóa khi có tour slots ho?c tour details
- **Hi?n t?i (Delete)**: Ch? ng?n c?n xóa khi có tour details ???c t?o s? d?ng template ?ó
- **Tr??c ?ây (Update)**: Ch? ng?n c?n c?p nh?t khi có tour details ? tr?ng thái Public
- **Hi?n t?i (Update)**: Ng?n c?n c?p nh?t khi có b?t k? tour details nào ???c t?o s? d?ng template ?ó

## Changes Made

### 1. Enhanced Tour Template Service
**File**: `TayNinhTourApi.BusinessLogicLayer/Services/EnhancedTourTemplateService.cs`

#### Method Updated: `CanDeleteTourTemplateAsync(Guid id)` ?

**Logic m?i**:// NEW LOGIC: Ch? ng?n c?n xóa khi có tour details ???c t?o s? d?ng template này
// Không quan tâm ??n tour slots n?a - tour slots ch? là d? li?u ph? tr?

// Ki?m tra xem có tour details nào ???c t?o t? template này không
var existingTourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
    td => td.TourTemplateId == id && !td.IsDeleted);

if (existingTourDetails.Any())
{
    // Ng?n c?n xóa và ??a ra thông tin chi ti?t
    // - S? l??ng tour details
    // - Phân tích Public vs Draft
    // - S? l??ng bookings n?u có
    // - H??ng d?n gi?i quy?t
}

// NOTE: Không còn ki?m tra tour slots n?a
// Tour slots là d? li?u ph? tr? và có th? t?n t?i mà không ?nh h??ng ??n vi?c xóa template
#### Method Updated: `CanUpdateTourTemplateAsync(Guid id)` ?

**Logic m?i**:// NEW LOGIC: Ng?n c?n c?p nh?t khi có b?t k? tour details nào ???c t?o s? d?ng template này
// Không ch? riêng Public nh? tr??c ?ây - b?t k? tr?ng thái nào c?ng ng?n c?n update

// Ki?m tra xem có tour details nào ???c t?o t? template này không
var existingTourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
    td => td.TourTemplateId == id && !td.IsDeleted);

if (existingTourDetails.Any())
{
    // Ng?n c?n c?p nh?t và ??a ra thông tin chi ti?t
    // - S? l??ng tour details (b?t k? tr?ng thái nào)
    // - Phân tích Public vs Draft 
    // - S? l??ng bookings n?u có
    // - H??ng d?n gi?i quy?t
}

// NOTE: Không còn ki?m tra tour slots n?a
// Tour slots là d? li?u ph? tr? và không ng?n c?n vi?c c?p nh?t template
### 2. Legacy Tour Template Service
**File**: `TayNinhTourApi.BusinessLogicLayer/Services/TourTemplateService.cs`

#### Method Updated: `CanDeleteTourTemplateAsync(Guid id)` ?

**Logic m?i**:public async Task<bool> CanDeleteTourTemplateAsync(Guid id)
{
    // NEW LOGIC: Ch? ng?n c?n xóa khi có tour details ???c t?o s? d?ng template này
    // Không quan tâm ??n tour slots n?a - tour slots ch? là d? li?u ph? tr?
    
    // Ki?m tra xem có tour details nào ???c t?o t? template này không
    var existingTourDetails = await _unitOfWork.TourDetailsRepository.GetAllAsync(
        td => td.TourTemplateId == id && !td.IsDeleted);
    
    // Ch? ng?n c?n xóa n?u có tour details ???c t?o t? template này
    // Tour slots không còn là y?u t? ng?n c?n xóa
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
        throw new InvalidOperationException($"Không th? c?p nh?t tour template vì có {existingTourDetails.Count()} tour details ?ang s? d?ng template này");
    }
    
    // Continue with update...
}
### 3. API Documentation Update
**File**: `TayNinhTourApi.Controller/Documentation/TourTemplate_API_Documentation.md`

**C?p nh?t**:
- Thêm thông tin v? logic m?i cho c? Delete và Update
- C?p nh?t ví d? response cho Update endpoint
- Gi?i thích rõ ràng khi nào có th? c?p nh?t vs không th? c?p nh?t

## Business Logic

### DELETE - Tr??ng h?p CÓ TH? XÓA ?
1. Template ch? có tour slots (không có tour details nào ???c t?o)
2. Tour slots là d? li?u ph? tr? và không ng?n c?n vi?c xóa template

### DELETE - Tr??ng h?p KHÔNG TH? XÓA ?
1. Có tour details ???c t?o t? template này (b?t k? tr?ng thái nào)
2. Chi ti?t phân tích:
   - **Public tour details**: Có th? có bookings t? khách hàng
   - **Draft/WaitToPublic tour details**: ?ang ???c chu?n b?
   - **Bookings**: Confirmed ho?c Pending t? khách hàng

### UPDATE - Tr??ng h?p CÓ TH? C?P NH?T ? 
1. Template ch? có tour slots (không có tour details nào ???c t?o)
2. Tour slots là d? li?u ph? tr? và không ng?n c?n vi?c c?p nh?t template

### UPDATE - Tr??ng h?p KHÔNG TH? C?P NH?T ?
1. Có b?t k? tour details nào ???c t?o t? template này (m?i tr?ng thái)
2. Lý do: C?p nh?t template có th? ?nh h??ng ??n các tour details ?ã ???c t?o
3. Chi ti?t phân tích:
   - **Public tour details**: ?nh h??ng tr?c ti?p ??n khách hàng ?ã booking
   - **Draft tour details**: ?nh h??ng ??n công vi?c ?ang chu?n b?
   - **Bookings**: Có th? gây inconsistency v?i d? li?u ?ã có

### Blocking Reasons Details
Khi không th? xóa/c?p nh?t, h? th?ng s? ??a ra thông tin chi ti?t:

#### Delete Response:{
  "blockingReasons": [
    "Có 2 tour details ?ã ???c t?o s? d?ng template này",
    "Trong ?ó có 1 tour details ?ang ? tr?ng thái Public", 
    "Có 5 booking ?ã ???c khách hàng xác nh?n",
    "Có 1 tour details ?ang ? tr?ng thái Draft/WaitToPublic",
    "Vui lòng xóa t?t c? tour details liên quan tr??c khi xóa template",
    "Ho?c chuy?n các tour details sang s? d?ng template khác"
  ]
}
#### Update Response:{
  "validationErrors": [
    "Có 2 tour details ?ã ???c t?o s? d?ng template này",
    "Trong ?ó có 1 tour details ?ang ? tr?ng thái Public",
    "Có 5 booking ?ã ???c khách hàng xác nh?n",
    "Có 1 tour details ?ang ? tr?ng thái Draft/WaitToPublic",
    "Vi?c c?p nh?t template có th? ?nh h??ng ??n các tour details ?ã ???c t?o",
    "Vui lòng xóa t?t c? tour details liên quan tr??c khi c?p nh?t template",
    "Ho?c chuy?n các tour details sang s? d?ng template khác"
  ]
}
## API Response Examples

### Delete

#### Thành công - Ch? có tour slots{
  "statusCode": 200,
  "message": "Xóa tour template thành công", 
  "success": true,
  "reason": "Template này ch? có tour slots và có th? xóa an toàn"
}
#### Th?t b?i - Có tour details{
  "statusCode": 409,
  "message": "Không th? xóa tour template vì có tour details ?ang s? d?ng",
  "success": false,
  "reason": "Tour template này ?ang ???c s? d?ng b?i các tour details và không th? xóa",
  "blockingReasons": [...]
}
### Update

#### Thành công - Ch? có tour slots{
  "statusCode": 200,
  "message": "C?p nh?t tour template thành công",
  "success": true,
  "reason": "Template này ch? có tour slots và có th? c?p nh?t an toàn"
}
#### Th?t b?i - Có tour details{
  "statusCode": 409,
  "message": "Không th? c?p nh?t tour template vì có tour details ?ang s? d?ng",
  "success": false,
  "validationErrors": [...]
}
## Testing Scenarios

### Test Case 1: Template v?i ch? tour slots
- T?o tour template
- Generate tour slots
- Th? xóa template ? **Thành công** ?
- Th? c?p nh?t template ? **Thành công** ?

### Test Case 2: Template v?i tour details (Draft)
- T?o tour template  
- Generate tour slots
- T?o tour details t? template
- Th? xóa template ? **Th?t b?i** ?
- Th? c?p nh?t template ? **Th?t b?i** ?

### Test Case 3: Template v?i tour details (Public + Bookings)
- T?o tour template
- Generate tour slots  
- T?o tour details t? template
- Publish tour details
- Có bookings t? khách hàng
- Th? xóa template ? **Th?t b?i v?i thông tin chi ti?t** ?
- Th? c?p nh?t template ? **Th?t b?i v?i thông tin chi ti?t** ?

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

1. **Tour Slots**: Không còn ng?n c?n vi?c xóa/c?p nh?t template
2. **Tour Details**: Là y?u t? quy?t ??nh vi?c có th? xóa/c?p nh?t template hay không  
3. **Backward Compatibility**: C? Enhanced và Legacy service ??u ???c c?p nh?t
4. **Error Messages**: ??y ?? và h??ng d?n rõ ràng cách gi?i quy?t
5. **Build**: ?ã test và build thành công

## Implementation Status

- [x] Enhanced Tour Template Service DELETE updated
- [x] Enhanced Tour Template Service UPDATE updated
- [x] Legacy Tour Template Service DELETE updated  
- [x] Legacy Tour Template Service UPDATE updated
- [x] API Documentation updated
- [x] Build verification successful
- [x] Logic tested and validated

**Ngày c?p nh?t**: 2024-12-21
**Ng??i th?c hi?n**: GitHub Copilot