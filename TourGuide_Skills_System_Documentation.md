# TourGuide Skills System Documentation

## 📋 Overview

Hệ thống kỹ năng (Skills) cho hướng dẫn viên du lịch trong TayNinhTour, bao gồm 29 kỹ năng được phân thành 4 danh mục chính.

## 🎯 Skills Categories & IDs

### 1. **NGÔN NGỮ (LANGUAGES)** - IDs: 1-8

| ID | Skill Name | Vietnamese Name | English Name |
|----|------------|-----------------|--------------|
| 1  | Vietnamese | Tiếng Việt | Vietnamese |
| 2  | English | Tiếng Anh | English |
| 3  | Chinese | Tiếng Trung | Chinese (Mandarin) |
| 4  | Japanese | Tiếng Nhật | Japanese |
| 5  | Korean | Tiếng Hàn | Korean |
| 6  | French | Tiếng Pháp | French |
| 7  | German | Tiếng Đức | German |
| 8  | Russian | Tiếng Nga | Russian |

### 2. **KIẾN THỨC CHUYÊN MÔN (KNOWLEDGE AREAS)** - IDs: 101-108

| ID  | Skill Name | Vietnamese Name | English Name | Notes |
|-----|------------|-----------------|--------------|-------|
| 101 | History | Lịch sử | Historical knowledge | |
| 102 | Culture | Văn hóa | Cultural knowledge | |
| 103 | Religion | Tôn giáo | Religious knowledge | Đặc biệt quan trọng cho Tây Ninh với Tòa Thánh Cao Đài |
| 104 | Cuisine | Ẩm thực | Culinary knowledge | |
| 105 | Geography | Địa lý | Geography knowledge | |
| 106 | Nature | Thiên nhiên | Nature and ecology knowledge | |
| 107 | Arts | Nghệ thuật | Arts and crafts knowledge | |
| 108 | Architecture | Kiến trúc | Architectural knowledge | |

### 3. **KỸ NĂNG HOẠT ĐỘNG (ACTIVITY SKILLS)** - IDs: 201-208

| ID  | Skill Name | Vietnamese Name | English Name |
|-----|------------|-----------------|--------------|
| 201 | MountainClimbing | Leo núi | Mountain climbing |
| 202 | Trekking | Trekking | Trekking |
| 203 | Photography | Chụp ảnh | Photography |
| 204 | WaterSports | Thể thao nước | Water sports |
| 205 | Cycling | Đi xe đạp | Cycling |
| 206 | Camping | Cắm trại | Camping |
| 207 | BirdWatching | Quan sát chim | Bird watching |
| 208 | AdventureSports | Thể thao mạo hiểm | Adventure sports |

### 4. **KỸ NĂNG ĐẶC BIỆT (SPECIAL SKILLS)** - IDs: 301-305

| ID  | Skill Name | Vietnamese Name | English Name |
|-----|------------|-----------------|--------------|
| 301 | FirstAid | Sơ cứu | First aid |
| 302 | Driving | Lái xe | Driving |
| 303 | Cooking | Nấu ăn | Cooking |
| 304 | Meditation | Hướng dẫn thiền | Meditation guidance |
| 305 | TraditionalMassage | Massage truyền thống | Traditional massage |

## 📊 Summary Statistics

- **Total Skills**: 29
- **Languages**: 8 skills (IDs: 1-8)
- **Knowledge Areas**: 8 skills (IDs: 101-108)
- **Activity Skills**: 8 skills (IDs: 201-208)
- **Special Skills**: 5 skills (IDs: 301-305)

## 🔧 Technical Implementation

### **Enum Definition**
```csharp
public enum TourGuideSkill
{
    // Languages (1-8)
    Vietnamese = 1, English = 2, Chinese = 3, Japanese = 4,
    Korean = 5, French = 6, German = 7, Russian = 8,
    
    // Knowledge Areas (101-108)
    History = 101, Culture = 102, Religion = 103, Cuisine = 104,
    Geography = 105, Nature = 106, Arts = 107, Architecture = 108,
    
    // Activity Skills (201-208)
    MountainClimbing = 201, Trekking = 202, Photography = 203, WaterSports = 204,
    Cycling = 205, Camping = 206, BirdWatching = 207, AdventureSports = 208,
    
    // Special Skills (301-305)
    FirstAid = 301, Driving = 302, Cooking = 303, 
    Meditation = 304, TraditionalMassage = 305
}
```

### **Storage Format**
Skills được lưu trữ dưới dạng comma-separated string:
```
"Vietnamese,English,History,MountainClimbing,FirstAid"
```

### **API Usage Examples**

#### **TourGuide Application**
```json
{
  "fullName": "Nguyễn Văn A",
  "skills": [1, 2, 101, 201, 301],
  "skillsString": "Vietnamese,English,History,MountainClimbing,FirstAid"
}
```

#### **Tour Requirements**
```json
{
  "tourTitle": "Tour Núi Bà Đen",
  "skillsRequired": "Vietnamese,English,Religion,MountainClimbing"
}
```

## 🎯 Use Cases

### **1. TourGuide Registration**
- Guide chọn skills từ danh sách 29 skills
- Hệ thống validate và lưu trữ
- Hiển thị skills bằng tiếng Việt cho user

### **2. Tour Creation**
- Tour company chỉ định skills required
- Hệ thống match với available guides
- Tính toán compatibility score

### **3. Guide Matching**
- So sánh required skills vs guide skills
- Tính match score (0.0 - 1.0)
- Ưu tiên guides có skills phù hợp nhất

### **4. Skills Management**
- Admin có thể xem thống kê skills
- Phân tích skills phổ biến
- Đề xuất training cho guides

## 🔍 Validation Rules

### **Required Skills**
- Ít nhất 1 skill phải được chọn
- Skills phải thuộc enum TourGuideSkill
- Không duplicate skills

### **String Format**
- Comma-separated values
- Không có spaces thừa
- Case-sensitive enum names

### **Category Rules**
- Mỗi category có thể chọn multiple skills
- Không bắt buộc phải có skill từ mọi category
- Languages thường là required cho most tours

## 🛠️ Utility Methods

### **TourGuideSkillUtility Class**

#### **Core Methods**
```csharp
// Convert skills list to string
string skillsString = TourGuideSkillUtility.SkillsToString(skillsList);

// Convert string to skills list
List<TourGuideSkill> skills = TourGuideSkillUtility.StringToSkills("Vietnamese,English");

// Get display name
string displayName = TourGuideSkillUtility.GetDisplayName(TourGuideSkill.Vietnamese); // "Tiếng Việt"

// Validate skills string
bool isValid = TourGuideSkillUtility.IsValidSkillsString("Vietnamese,English");

// Get skills by category
Dictionary<string, List<TourGuideSkill>> categories = TourGuideSkillUtility.GetSkillsByCategory();
```

#### **Migration Support**
```csharp
// Migrate legacy languages to new skill format
string newSkills = TourGuideSkillUtility.MigrateLegacyLanguages("Vietnamese, English, Chinese");
// Result: "Vietnamese,English,Chinese"
```

### **SkillsMatchingUtility Class**

#### **Matching Methods**
```csharp
// Calculate match score between required and guide skills
double score = SkillsMatchingUtility.CalculateMatchScoreEnhanced(
    "Vietnamese,English,History",
    "Vietnamese,English,History,Culture"
); // Result: 1.0 (perfect match)

// Check if guide meets minimum requirements
bool meetsRequirements = SkillsMatchingUtility.MeetsMinimumRequirements(
    "Vietnamese,English",
    "Vietnamese,English,History"
); // Result: true
```

## 🔄 Migration & Compatibility

### **Legacy Language Support**
Hệ thống hỗ trợ migration từ old language system:
- Old: `"Vietnamese, English, Chinese"`
- New: `"Vietnamese,English,Chinese"`

### **Backward Compatibility**
- API accepts both Skills list và SkillsString
- Automatic conversion between formats
- Validation for both old and new formats

## 📈 Future Enhancements

### **Planned Features**
- Skill levels (Beginner, Intermediate, Advanced)
- Skill certifications
- Skill expiry dates
- Custom skills for specific regions

### **Analytics**
- Most requested skills
- Skills gap analysis
- Guide training recommendations
- Tour success rate by skills match

## 📚 Related Files

- **Enum**: `TayNinhTourApi.DataAccessLayer/Enums/TourGuideSkill.cs`
- **Utility**: `TayNinhTourApi.BusinessLogicLayer/Utilities/TourGuideSkillUtility.cs`
- **Matching**: `TayNinhTourApi.BusinessLogicLayer/Utilities/SkillsMatchingUtility.cs`
- **Service**: `TayNinhTourApi.BusinessLogicLayer/Services/SkillManagementService.cs`
- **DTO**: `TayNinhTourApi.BusinessLogicLayer/DTOs/Request/SubmitTourGuideApplicationDto.cs`

---

**Last Updated**: 2025-07-02
**Total Skills**: 29
**Version**: 1.0
