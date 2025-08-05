# ?? Tour Template Validation Test Cases

C�c test cases JSON ?? test validation errors trong Swagger UI cho endpoint `POST /api/TourCompany/template`

## ?? Test Cases Overview

| Test Case | Expected Error | Status Code |
|-----------|----------------|-------------|
| [Null Title](#1-null-title) | Title required | 400 |
| [Title Too Long](#2-title-too-long) | Title max 200 chars | 400 |
| [Null Start Location](#3-null-start-location) | Start location required | 400 |
| [Null End Location](#4-null-end-location) | End location required | 400 |
| [Invalid Month (0)](#5-invalid-month-0) | Month 1-12 | 400 |
| [Invalid Month (13)](#6-invalid-month-13) | Month 1-12 | 400 |
| [Invalid Year (2023)](#7-invalid-year-2023) | Year 2024-2030 | 400 |
| [Invalid Year (2031)](#8-invalid-year-2031) | Year 2024-2030 | 400 |
| [Invalid Schedule Day](#9-invalid-schedule-day-weekday) | Only Saturday/Sunday | 400 |
| [Current Month](#10-current-month) | Must be future month | 400 |
| [Past Month](#11-past-month) | Must be future month | 400 |
| [First Slot Date - Same Month](#12-first-slot-date-same-month) | 30 days + next month rule | 400 |
| [First Slot Date - Not 30 Days](#13-first-slot-date-not-30-days) | 30 days + next month rule | 400 |
| [Multiple Errors](#14-multiple-validation-errors) | Multiple field errors | 400 |
| [Valid Case](#15-valid-case) | Should succeed | 201 |

---

## ?? Error Test Cases

### 1. Null Title
**Expected Error:** `T�n template l� b?t bu?c`
{
  "title": null,
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2025,
  "images": []
}
### 2. Title Too Long
**Expected Error:** `T�n template kh�ng ???c v??t qu� 200 k� t?`
{
  "title": "Tour N�i B� ?en v?i tr?i nghi?m tuy?t v?i kh�m ph� thi�n nhi�n hoang d� v� v?n h�a ??a ph??ng ??c ?�o c�ng v?i c�c ho?t ??ng th� v? nh? leo n�i chinh ph?c ??nh cao ng?m c?nh to�n v? v� tham quan c�c di t�ch l?ch s? v?n h�a quan tr?ng c?a v�ng ??t T�y Ninh",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2025,
  "images": []
}
### 3. Null Start Location
**Expected Error:** `?i?m b?t ??u l� b?t bu?c`
{
  "title": "Tour N�i B� ?en",
  "startLocation": null,
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2025,
  "images": []
}
### 4. Null End Location
**Expected Error:** `?i?m k?t th�c l� b?t bu?c`
{
  "title": "Tour N�i B� ?en",
  "startLocation": "TP.HCM",
  "endLocation": null,
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2025,
  "images": []
}
### 5. Invalid Month (0)
**Expected Error:** `Th�ng ph?i t? 1 ??n 12`
{
  "title": "Tour N�i B� ?en",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 0,
  "year": 2025,
  "images": []
}
### 6. Invalid Month (13)
**Expected Error:** `Th�ng ph?i t? 1 ??n 12`
{
  "title": "Tour N�i B� ?en",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 13,
  "year": 2025,
  "images": []
}
### 7. Invalid Year (2023)
**Expected Error:** `N?m ph?i t? 2024 ??n 2030`
{
  "title": "Tour N�i B� ?en",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2023,
  "images": []
}
### 8. Invalid Year (2031)
**Expected Error:** `N?m ph?i t? 2024 ??n 2030`
{
  "title": "Tour N�i B� ?en",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2031,
  "images": []
}
### 9. Invalid Schedule Day (Weekday)
**Expected Error:** `Ch? ???c ch?n Th? 7 ho?c Ch? nh?t`
{
  "title": "Tour N�i B� ?en",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Monday",
  "month": 8,
  "year": 2025,
  "images": []
}
### 10. Current Month ?? **M?I**
**Expected Error:** `Kh�ng th? t?o template cho th�ng hi?n t?i ho?c th�ng ?� qua`

?? **Adjust theo th�ng hi?n t?i khi test**
{
  "title": "Tour N�i B� ?en - Current Month",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 1,
  "year": 2025,
  "images": []
}
### 11. Past Month ?? **M?I**
**Expected Error:** `Kh�ng th? t?o template cho th�ng hi?n t?i ho?c th�ng ?� qua`
{
  "title": "Tour N�i B� ?en - Past Month",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Sunday",
  "month": 12,
  "year": 2024,
  "images": []
}
### 12. First Slot Date - Same Month
**Expected Error:** `Slot ??u ti�n ph?i b?t ??u sau �t nh?t 30 ng�y v� n?m t? th�ng k? ti?p tr? ?i`

?? **Test n�y ph? thu?c v�o ng�y hi?n t?i**
{
  "title": "Tour N�i B� ?en - Same Month Test",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 2,
  "year": 2025,
  "images": []
}
### 13. First Slot Date - Not 30 Days
**Expected Error:** `Slot ??u ti�n ph?i b?t ??u sau �t nh?t 30 ng�y v� n?m t? th�ng k? ti?p tr? ?i`

?? **Test n�y ph? thu?c v�o ng�y hi?n t?i**
{
  "title": "Tour N�i B� ?en - Not 30 Days Test",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 2,
  "year": 2025,
  "images": []
}
### 14. Multiple Validation Errors
**Expected Error:** Multiple field errors
{
  "title": null,
  "startLocation": null,
  "endLocation": null,
  "templateType": "FreeScenic",
  "scheduleDays": "Monday",
  "month": 1,
  "year": 2023,
  "images": []
}
**Expected Response:**{
  "statusCode": 400,
  "message": "D? li?u kh�ng h?p l?",
  "validationErrors": [
    "T�n template l� b?t bu?c",
    "?i?m b?t ??u l� b?t bu?c",
    "?i?m k?t th�c l� b?t bu?c",
    "Ch? ???c ch?n Th? 7 ho?c Ch? nh?t",
    "N?m ph?i t? 2024 ??n 2030",
    "Kh�ng th? t?o template cho th�ng hi?n t?i ho?c th�ng ?� qua"
  ],
  "fieldErrors": {
    "Title": ["T�n template l� b?t bu?c"],
    "StartLocation": ["?i?m b?t ??u l� b?t bu?c"],
    "EndLocation": ["?i?m k?t th�c l� b?t bu?c"],
    "ScheduleDays": ["Ch? ???c ch?n Th? 7 ho?c Ch? nh?t"],
    "Year": ["N?m ph?i t? 2024 ??n 2030"],
    "FirstSlotDate": ["Kh�ng th? t?o template cho th�ng hi?n t?i ho?c th�ng ?� qua"]
  }
}
---

## ? Valid Test Case

### 15. Valid Case
**Expected Result:** Template created successfully (201)
{
  "title": "Tour N�i B� ?en Cu?i Tu?n",
  "startLocation": "TP.HCM",
  "endLocation": "T�y Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 6,
  "year": 2025,
  "images": [
    "https://example.com/image1.jpg",
    "https://example.com/image2.jpg"
  ]
}
**Expected Response:**{
  "statusCode": 201,
  "message": "T?o tour template th�nh c�ng v� ?� t?o X slots cho th�ng 6/2025",
  "data": {
    "id": "guid-here",
    "title": "Tour N�i B� ?en Cu?i Tu?n",
    "templateType": "FreeScenic",
    "scheduleDays": "Saturday",
    "startLocation": "TP.HCM",
    "endLocation": "T�y Ninh",
    "month": 6,
    "year": 2025,
    "createdAt": "2025-01-XX...",
    "isActive": true
  }
}
---

## ??? Testing Instructions

### Prerequisites
1. M? Swagger UI: `http://localhost:5267/swagger`
2. Authenticate v?i JWT token (role: Tour Company)
3. T�m endpoint: `POST /api/TourCompany/template`

### Testing Steps
1. **Copy JSON t? test case**
2. **Paste v�o Request body trong Swagger**
3. **Click Execute**
4. **Verify response matches expected error**

### Expected Response Format for Errors{
  "statusCode": 400,
  "message": "D? li?u kh�ng h?p l?",
  "validationErrors": ["List of error messages"],
  "fieldErrors": {
    "FieldName": ["Specific field errors"]
  }
}
---

## ?? Edge Cases to Test

### Boundary Values
- **Month:** Test 1, 12 (valid n?u future) v� 0, 13 (invalid)
- **Year:** Test 2024, 2030 (valid) v� 2023, 2031 (invalid)
- **Title Length:** Test 200 chars (valid) v� 201 chars (invalid)

### Date Logic Edge Cases ?? **C?P NH?T**
- **Current Month:** Template kh�ng ???c t?o cho th�ng hi?n t?i
- **Past Month:** Template kh�ng ???c t?o cho th�ng ?� qua
- **Current Date Dependent:** First slot date validation ph? thu?c ng�y hi?n t?i
- **Cross Year:** Test template t?o cu?i n?m cho th�ng ??u n?m sau
- **Leap Year:** Test February trong n?m nhu?n

### Schedule Day Validation
- **Valid:** `Saturday`, `Sunday`
- **Invalid:** `Monday`, `Tuesday`, `Wednesday`, `Thursday`, `Friday`

---

## ?? Test Results Checklist

- [ ] All error cases return status code 400
- [ ] Valid case returns status code 201
- [ ] Error messages are in Vietnamese
- [ ] Field errors are properly structured
- [ ] Multiple errors are all captured
- [ ] **NEW:** Current month validation works correctly
- [ ] **NEW:** Past month validation works correctly
- [ ] First slot date validation works correctly
- [ ] Automatic slot generation happens after successful creation

---

## ?? **QUY T?C M?I**

### **Template Date Rules:**
1. ? **Template ph?i ???c t?o cho th�ng t??ng lai** (kh�ng ph?i th�ng hi?n t?i ho?c qu� kh?)
2. ? **Slot ??u ti�n ph?i sau �t nh?t 30 ng�y** t? ng�y t?o
3. ? **Slot ??u ti�n ph?i t? th�ng k? ti?p tr? ?i** (ng�y 1 c?a th�ng m?i)

### **Error Messages:**
- Current/Past Month: `"Kh�ng th? t?o template cho th�ng hi?n t?i ho?c th�ng ?� qua. Template ph?i ???c t?o cho th�ng t??ng lai."`
- 30 Days + Next Month: `"Slot ??u ti�n ph?i b?t ??u sau �t nh?t 30 ng�y v� n?m t? th�ng k? ti?p tr? ?i (t? ng�y 1 c?a th�ng m?i)."`