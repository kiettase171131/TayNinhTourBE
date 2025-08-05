# ?? Tour Template Validation Test Cases

Các test cases JSON ?? test validation errors trong Swagger UI cho endpoint `POST /api/TourCompany/template`

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
**Expected Error:** `Tên template là b?t bu?c`
{
  "title": null,
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2025,
  "images": []
}
### 2. Title Too Long
**Expected Error:** `Tên template không ???c v??t quá 200 ký t?`
{
  "title": "Tour Núi Bà ?en v?i tr?i nghi?m tuy?t v?i khám phá thiên nhiên hoang dã và v?n hóa ??a ph??ng ??c ?áo cùng v?i các ho?t ??ng thú v? nh? leo núi chinh ph?c ??nh cao ng?m c?nh toàn v? và tham quan các di tích l?ch s? v?n hóa quan tr?ng c?a vùng ??t Tây Ninh",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2025,
  "images": []
}
### 3. Null Start Location
**Expected Error:** `?i?m b?t ??u là b?t bu?c`
{
  "title": "Tour Núi Bà ?en",
  "startLocation": null,
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2025,
  "images": []
}
### 4. Null End Location
**Expected Error:** `?i?m k?t thúc là b?t bu?c`
{
  "title": "Tour Núi Bà ?en",
  "startLocation": "TP.HCM",
  "endLocation": null,
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2025,
  "images": []
}
### 5. Invalid Month (0)
**Expected Error:** `Tháng ph?i t? 1 ??n 12`
{
  "title": "Tour Núi Bà ?en",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 0,
  "year": 2025,
  "images": []
}
### 6. Invalid Month (13)
**Expected Error:** `Tháng ph?i t? 1 ??n 12`
{
  "title": "Tour Núi Bà ?en",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 13,
  "year": 2025,
  "images": []
}
### 7. Invalid Year (2023)
**Expected Error:** `N?m ph?i t? 2024 ??n 2030`
{
  "title": "Tour Núi Bà ?en",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2023,
  "images": []
}
### 8. Invalid Year (2031)
**Expected Error:** `N?m ph?i t? 2024 ??n 2030`
{
  "title": "Tour Núi Bà ?en",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 8,
  "year": 2031,
  "images": []
}
### 9. Invalid Schedule Day (Weekday)
**Expected Error:** `Ch? ???c ch?n Th? 7 ho?c Ch? nh?t`
{
  "title": "Tour Núi Bà ?en",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Monday",
  "month": 8,
  "year": 2025,
  "images": []
}
### 10. Current Month ?? **M?I**
**Expected Error:** `Không th? t?o template cho tháng hi?n t?i ho?c tháng ?ã qua`

?? **Adjust theo tháng hi?n t?i khi test**
{
  "title": "Tour Núi Bà ?en - Current Month",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 1,
  "year": 2025,
  "images": []
}
### 11. Past Month ?? **M?I**
**Expected Error:** `Không th? t?o template cho tháng hi?n t?i ho?c tháng ?ã qua`
{
  "title": "Tour Núi Bà ?en - Past Month",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Sunday",
  "month": 12,
  "year": 2024,
  "images": []
}
### 12. First Slot Date - Same Month
**Expected Error:** `Slot ??u tiên ph?i b?t ??u sau ít nh?t 30 ngày và n?m t? tháng k? ti?p tr? ?i`

?? **Test này ph? thu?c vào ngày hi?n t?i**
{
  "title": "Tour Núi Bà ?en - Same Month Test",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
  "templateType": "FreeScenic",
  "scheduleDays": "Saturday",
  "month": 2,
  "year": 2025,
  "images": []
}
### 13. First Slot Date - Not 30 Days
**Expected Error:** `Slot ??u tiên ph?i b?t ??u sau ít nh?t 30 ngày và n?m t? tháng k? ti?p tr? ?i`

?? **Test này ph? thu?c vào ngày hi?n t?i**
{
  "title": "Tour Núi Bà ?en - Not 30 Days Test",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
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
  "message": "D? li?u không h?p l?",
  "validationErrors": [
    "Tên template là b?t bu?c",
    "?i?m b?t ??u là b?t bu?c",
    "?i?m k?t thúc là b?t bu?c",
    "Ch? ???c ch?n Th? 7 ho?c Ch? nh?t",
    "N?m ph?i t? 2024 ??n 2030",
    "Không th? t?o template cho tháng hi?n t?i ho?c tháng ?ã qua"
  ],
  "fieldErrors": {
    "Title": ["Tên template là b?t bu?c"],
    "StartLocation": ["?i?m b?t ??u là b?t bu?c"],
    "EndLocation": ["?i?m k?t thúc là b?t bu?c"],
    "ScheduleDays": ["Ch? ???c ch?n Th? 7 ho?c Ch? nh?t"],
    "Year": ["N?m ph?i t? 2024 ??n 2030"],
    "FirstSlotDate": ["Không th? t?o template cho tháng hi?n t?i ho?c tháng ?ã qua"]
  }
}
---

## ? Valid Test Case

### 15. Valid Case
**Expected Result:** Template created successfully (201)
{
  "title": "Tour Núi Bà ?en Cu?i Tu?n",
  "startLocation": "TP.HCM",
  "endLocation": "Tây Ninh",
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
  "message": "T?o tour template thành công và ?ã t?o X slots cho tháng 6/2025",
  "data": {
    "id": "guid-here",
    "title": "Tour Núi Bà ?en Cu?i Tu?n",
    "templateType": "FreeScenic",
    "scheduleDays": "Saturday",
    "startLocation": "TP.HCM",
    "endLocation": "Tây Ninh",
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
3. Tìm endpoint: `POST /api/TourCompany/template`

### Testing Steps
1. **Copy JSON t? test case**
2. **Paste vào Request body trong Swagger**
3. **Click Execute**
4. **Verify response matches expected error**

### Expected Response Format for Errors{
  "statusCode": 400,
  "message": "D? li?u không h?p l?",
  "validationErrors": ["List of error messages"],
  "fieldErrors": {
    "FieldName": ["Specific field errors"]
  }
}
---

## ?? Edge Cases to Test

### Boundary Values
- **Month:** Test 1, 12 (valid n?u future) và 0, 13 (invalid)
- **Year:** Test 2024, 2030 (valid) và 2023, 2031 (invalid)
- **Title Length:** Test 200 chars (valid) và 201 chars (invalid)

### Date Logic Edge Cases ?? **C?P NH?T**
- **Current Month:** Template không ???c t?o cho tháng hi?n t?i
- **Past Month:** Template không ???c t?o cho tháng ?ã qua
- **Current Date Dependent:** First slot date validation ph? thu?c ngày hi?n t?i
- **Cross Year:** Test template t?o cu?i n?m cho tháng ??u n?m sau
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
1. ? **Template ph?i ???c t?o cho tháng t??ng lai** (không ph?i tháng hi?n t?i ho?c quá kh?)
2. ? **Slot ??u tiên ph?i sau ít nh?t 30 ngày** t? ngày t?o
3. ? **Slot ??u tiên ph?i t? tháng k? ti?p tr? ?i** (ngày 1 c?a tháng m?i)

### **Error Messages:**
- Current/Past Month: `"Không th? t?o template cho tháng hi?n t?i ho?c tháng ?ã qua. Template ph?i ???c t?o cho tháng t??ng lai."`
- 30 Days + Next Month: `"Slot ??u tiên ph?i b?t ??u sau ít nh?t 30 ngày và n?m t? tháng k? ti?p tr? ?i (t? ngày 1 c?a tháng m?i)."`