# Holiday Tour Template Validation Rules Summary

## ??? Validation ?� ???c implement

Holiday Tour Template b�y gi? �p d?ng **c�ng business rules** nh? tour template b�nh th??ng ?? ??m b?o t�nh nh?t qu�n v� ?�ng tin c?y.

### ? ?� th�m c�c Validation Rules:

#### 1. **?? Quy t?c 30 ng�y** (Quan tr?ng nh?t)
```csharp
// Ng�y tour ph?i sau �t nh?t 30 ng�y t? ng�y t?o template
var minimumDate = currentTime.AddDays(30);
if (tourDateTime < minimumDate) {
    // B�o l?i v?i g?i � c? th?
}
```

**V� d?**:
- H�m nay: 15/01/2025
- Ng�y s?m nh?t c� th?: 14/02/2025
- G?i �: 21/02/2025 (th�m 7 ng�y an to�n)

#### 2. **?? Validation th?i gian**
```csharp
// Ph?i trong t??ng lai
if (request.TourDate <= DateOnly.FromDateTime(currentTime))

// Kh�ng qu� 2 n?m
var maxFutureDate = DateOnly.FromDateTime(currentTime.AddYears(2));
if (request.TourDate > maxFutureDate)

// N?m h?p l? 2024-2030
if (request.TourDate.Year < 2024 || request.TourDate.Year > 2030)
```

#### 3. **?? Validation Business Rules**
```csharp
// �p d?ng c�ng business validation nh? regular template
var businessValidation = TourTemplateValidator.ValidateBusinessRules(tourTemplate);

// Ki?m tra slot date validation
var slotValidation = TourTemplateValidator.ValidateFirstSlotDate(
    tourTemplate.CreatedAt, 
    tourDateTime.Month, 
    tourDateTime.Year
);
```

#### 4. **?? Validation c? b?n**
- Title: B?t bu?c, max 200 k� t?
- StartLocation: B?t bu?c
- EndLocation: B?t bu?c
- Images: T�y ch?n, max 10 items

## ?? Error Messages chi ti?t

### L?i vi ph?m 30 ng�y:
```json
{
  "fieldErrors": {
    "tourDate": [
      "Ng�y tour ph?i sau �t nh?t 30 ng�y t? ng�y t?o (15/01/2025). Ng�y s?m nh?t c� th?: 14/02/2025. G?i �: Ch?n ng�y 21/02/2025 ho?c mu?n h?n. V� d? JSON h?p l?: \"tourDate\": \"2025-02-21\""
    ]
  },
  "validationErrors": [
    "?? H??NG D?N HOLIDAY TEMPLATE:",
    "� Ng�y hi?n t?i: 15/01/2025 - KH�NG th? ch?n",
    "� Ng�y s?m nh?t: 14/02/2025 (sau 30 ng�y)",
    "� Ng�y mu?n nh?t: 15/01/2027 (t?i ?a 2 n?m)",
    "� V� d? JSON h?p l?: {\"tourDate\": \"2025-02-21\"}",
    "� Kh�c template th??ng: Holiday template c� th? ch?n b?t k? ng�y n�o trong tu?n"
  ]
}
```

## ?? So s�nh Validation v?i Regular Template

| Rule | Regular Template | Holiday Template | Status |
|------|------------------|------------------|--------|
| **30-day rule** | ? Slot ??u th�ng ph?i sau 30 ng�y | ? TourDate ph?i sau 30 ng�y | **SAME** |
| **Year range** | ? 2024-2030 | ? 2024-2030 | **SAME** |
| **Future date** | ? Th�ng/n?m > hi?n t?i | ? TourDate > hi?n t?i | **SAME** |
| **Max range** | ? Kh�ng qu� 2 n?m | ? Kh�ng qu� 2 n?m | **SAME** |
| **Schedule day** | ?? Ch? Sat/Sun | ?? B?t k? ng�y n�o | **DIFFERENT** |
| **Input format** | Month + Year + ScheduleDay | TourDate | **DIFFERENT** |

## ? Validation Flow

```mermaid
graph TD
    A[Request] --> B[Basic Validation]
    B --> C{Valid?}
    C -->|No| D[Return 400 + Field Errors]
    C -->|Yes| E[Image Validation]
    E --> F[Create Template Entity]
    F --> G[Business Rules Validation]
    G --> H{Valid?}
    H -->|No| I[Return 400 + Business Errors]
    H -->|Yes| J[Slot Date Validation]
    J --> K{Valid?}
    K -->|No| L[Return 400 + Slot Errors]
    K -->|Yes| M[Save Template]
    M --> N[Create Slot]
    N --> O[Return 201 Success]
```

## ?? Benefits c?a vi?c th�m Validation

### 1. **T�nh nh?t qu�n**
- C�ng business rules cho c? 2 lo?i template
- C�ng error format v� user experience

### 2. **B?o v? business logic**
- ??m b?o Tour Company c� ?? th?i gian chu?n b? (30 ng�y)
- Tr�nh t?o tour cho qu� kh? ho?c qu� xa trong t??ng lai

### 3. **User Experience t?t**
- Error messages chi ti?t v?i v� d? c? th?
- G?i � ng�y h?p l?
- H??ng d?n format JSON

### 4. **Maintainability**
- T�i s? d?ng validation logic ?� c�
- D? test v� debug
- Consistent v?i existing codebase

## ?? Testing Scenarios

### ? Valid Cases:
```json
// Case 1: Ng�y h?p l? (sau 30 ng�y)
{"tourDate": "2025-02-21"} // H�m nay: 15/01/2025

// Case 2: Ng�y cu?i tu?n
{"tourDate": "2025-03-15"} // Saturday

// Case 3: Ng�y trong tu?n  
{"tourDate": "2025-04-02"} // Wednesday
```

### ? Invalid Cases:
```json
// Case 1: Qu� g?n (< 30 ng�y)
{"tourDate": "2025-02-01"} // Ch? 17 ng�y

// Case 2: Qu� kh?
{"tourDate": "2025-01-10"} // ?� qua

// Case 3: Qu� xa (> 2 n?m)
{"tourDate": "2027-06-01"} // Qu� 2 n?m
```

---

**Last Updated**: 15/01/2025  
**Version**: 2.0  
**Validation Status**: ? Complete