# ValidSkillSelection AllowEmpty Fix Test

## 🐛 Bug Description

**Issue**: `[ValidSkillSelection(AllowEmpty = true)]` vẫn trả về error "Định dạng kỹ năng không hợp lệ" khi skillsString có giá trị invalid.

**Expected**: Khi `AllowEmpty = true`, invalid skillsString nên được treat như empty và return `true`.

## 🔧 Root Cause

Trong `ValidSkillSelectionAttribute.IsValid()`:

### **Before Fix (Bug):**
```csharp
// Validate skills string format
var skillsList = TourGuideSkillUtility.StringToSkills(skillsString);
if (!skillsList.Any())
{
    ErrorMessage = "Định dạng kỹ năng không hợp lệ";
    return false; // ❌ Always return false, ignore AllowEmpty
}
```

### **After Fix:**
```csharp
// Validate skills string format
var skillsList = TourGuideSkillUtility.StringToSkills(skillsString);
if (!skillsList.Any())
{
    // Nếu AllowEmpty = true, cho phép invalid string (treat as empty)
    if (AllowEmpty)
    {
        return true; // ✅ Allow invalid when AllowEmpty = true
    }
    ErrorMessage = "Định dạng kỹ năng không hợp lệ";
    return false;
}
```

## 🧪 Test Cases

### **Test Case 1: Valid Skills String**
```csharp
var attribute = new ValidSkillSelectionAttribute { AllowEmpty = true };
bool result = attribute.IsValid("Vietnamese,English,History");
// Expected: true ✅
// Actual: true ✅
```

### **Test Case 2: Empty Skills String**
```csharp
var attribute = new ValidSkillSelectionAttribute { AllowEmpty = true };
bool result = attribute.IsValid("");
// Expected: true ✅
// Actual: true ✅
```

### **Test Case 3: Null Skills String**
```csharp
var attribute = new ValidSkillSelectionAttribute { AllowEmpty = true };
bool result = attribute.IsValid(null);
// Expected: true ✅
// Actual: true ✅
```

### **Test Case 4: Invalid Skills String (FIXED)**
```csharp
var attribute = new ValidSkillSelectionAttribute { AllowEmpty = true };
bool result = attribute.IsValid("InvalidSkill,AnotherInvalid");
// Expected: true ✅ (treat as empty when AllowEmpty = true)
// Before Fix: false ❌ "Định dạng kỹ năng không hợp lệ"
// After Fix: true ✅
```

### **Test Case 5: Invalid Skills String (AllowEmpty = false)**
```csharp
var attribute = new ValidSkillSelectionAttribute { AllowEmpty = false };
bool result = attribute.IsValid("InvalidSkill,AnotherInvalid");
// Expected: false ❌ "Định dạng kỹ năng không hợp lệ"
// Actual: false ❌ (correct behavior)
```

## 🎯 Use Case Context

### **SubmitTourGuideApplicationDto**
```csharp
public class SubmitTourGuideApplicationDto
{
    [Required(ErrorMessage = "Kỹ năng là bắt buộc")]
    [ValidSkillSelection(ErrorMessage = "Ít nhất một kỹ năng hợp lệ phải được chọn")]
    public List<TourGuideSkill> Skills { get; set; } = new();

    [ValidSkillSelection(AllowEmpty = true, ErrorMessage = "Định dạng kỹ năng không hợp lệ")]
    [StringLength(500, ErrorMessage = "Kỹ năng không được quá 500 ký tự")]
    public string? SkillsString { get; set; }
}
```

**Logic**:
- `Skills` field: **Required** - Must have valid skills
- `SkillsString` field: **Optional** - Can be empty/null/invalid (for API compatibility)

## 🔍 Validation Flow

### **Priority Order:**
1. **Skills List** (primary) - Required, must be valid
2. **SkillsString** (secondary) - Optional, for API compatibility

### **When SkillsString is used:**
- Frontend sends only `skillsString` (legacy API)
- Backend converts to `Skills` list
- If conversion fails and `AllowEmpty = true`, treat as empty
- Service layer will use `Skills` list as primary source

## ✅ Fix Verification

### **Manual Test:**
```json
POST /api/Account/tourguide-application/upload
{
  "fullName": "Test User",
  "phoneNumber": "0123456789",
  "email": "test@example.com",
  "experience": "2 years",
  "skills": [1, 2, 101], // Valid skills (primary)
  "skillsString": "InvalidSkill,BadSkill" // Invalid but AllowEmpty=true
}
```

**Expected Result**: ✅ Validation passes, uses `skills` field, ignores invalid `skillsString`

### **Edge Case Test:**
```json
{
  "skills": [], // Empty but required
  "skillsString": "Vietnamese,English" // Valid fallback
}
```

**Expected Result**: ❌ Validation fails on `skills` field (required)

---

**Status**: ✅ Fixed  
**File**: `TayNinhTourApi.BusinessLogicLayer/DTOs/Common/SkillSelectionDto.cs`  
**Lines**: 123-134
