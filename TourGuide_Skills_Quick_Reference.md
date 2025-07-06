# TourGuide Skills Quick Reference

## 🚀 Quick Lookup Table

### **Languages (1-8)**
```
1  = Vietnamese     = Tiếng Việt
2  = English        = Tiếng Anh  
3  = Chinese        = Tiếng Trung
4  = Japanese       = Tiếng Nhật
5  = Korean         = Tiếng Hàn
6  = French         = Tiếng Pháp
7  = German         = Tiếng Đức
8  = Russian        = Tiếng Nga
```

### **Knowledge Areas (101-108)**
```
101 = History       = Lịch sử
102 = Culture       = Văn hóa
103 = Religion      = Tôn giáo
104 = Cuisine       = Ẩm thực
105 = Geography     = Địa lý
106 = Nature        = Thiên nhiên
107 = Arts          = Nghệ thuật
108 = Architecture  = Kiến trúc
```

### **Activity Skills (201-208)**
```
201 = MountainClimbing  = Leo núi
202 = Trekking          = Trekking
203 = Photography       = Chụp ảnh
204 = WaterSports       = Thể thao nước
205 = Cycling           = Đi xe đạp
206 = Camping           = Cắm trại
207 = BirdWatching      = Quan sát chim
208 = AdventureSports   = Thể thao mạo hiểm
```

### **Special Skills (301-305)**
```
301 = FirstAid            = Sơ cứu
302 = Driving             = Lái xe
303 = Cooking             = Nấu ăn
304 = Meditation          = Hướng dẫn thiền
305 = TraditionalMassage  = Massage truyền thống
```

## 💻 Code Examples

### **Convert ID to Skill Name**
```csharp
// Method 1: Direct enum cast
TourGuideSkill skill = (TourGuideSkill)1; // Vietnamese
string displayName = TourGuideSkillUtility.GetDisplayName(skill); // "Tiếng Việt"

// Method 2: Parse from string
if (Enum.TryParse<TourGuideSkill>("Vietnamese", out TourGuideSkill parsedSkill))
{
    string name = TourGuideSkillUtility.GetDisplayName(parsedSkill);
}
```

### **Convert Skills Array to String**
```csharp
var skills = new List<TourGuideSkill> { 
    TourGuideSkill.Vietnamese, 
    TourGuideSkill.English, 
    TourGuideSkill.History 
};
string skillsString = TourGuideSkillUtility.SkillsToString(skills);
// Result: "Vietnamese,English,History"
```

### **Parse Skills String**
```csharp
string skillsString = "Vietnamese,English,History";
List<TourGuideSkill> skills = TourGuideSkillUtility.StringToSkills(skillsString);
// Result: [TourGuideSkill.Vietnamese, TourGuideSkill.English, TourGuideSkill.History]
```

### **Validate Skills**
```csharp
bool isValid = TourGuideSkillUtility.IsValidSkillsString("Vietnamese,English,InvalidSkill");
// Result: false (InvalidSkill is not a valid skill)
```

## 🎯 Common Use Cases

### **Frontend Skill Selection**
```javascript
// Skills data for frontend dropdown/checkbox
const skillCategories = {
  languages: [
    { id: 1, name: "Vietnamese", display: "Tiếng Việt" },
    { id: 2, name: "English", display: "Tiếng Anh" },
    // ... more languages
  ],
  knowledge: [
    { id: 101, name: "History", display: "Lịch sử" },
    { id: 102, name: "Culture", display: "Văn hóa" },
    // ... more knowledge areas
  ]
  // ... other categories
};
```

### **API Request Examples**
```json
// TourGuide Application
{
  "fullName": "Nguyễn Văn A",
  "skills": [1, 2, 101, 201],
  "skillsString": "Vietnamese,English,History,MountainClimbing"
}

// Tour Creation with Required Skills
{
  "title": "Tour Núi Bà Đen",
  "skillsRequired": "Vietnamese,English,Religion,MountainClimbing"
}
```

### **Database Query Examples**
```sql
-- Find guides with specific skills
SELECT * FROM TourGuideApplications 
WHERE Skills LIKE '%Vietnamese%' 
  AND Skills LIKE '%English%'
  AND Status = 1; -- Approved

-- Count guides by skill category
SELECT 
  CASE 
    WHEN Skills LIKE '%Vietnamese%' OR Skills LIKE '%English%' THEN 'Languages'
    WHEN Skills LIKE '%History%' OR Skills LIKE '%Culture%' THEN 'Knowledge'
    ELSE 'Other'
  END as Category,
  COUNT(*) as GuideCount
FROM TourGuideApplications 
WHERE Status = 1
GROUP BY Category;
```

## 🔍 Debugging Tips

### **Common Issues**
1. **Case Sensitivity**: Skill names are case-sensitive (`"vietnamese"` ≠ `"Vietnamese"`)
2. **Spaces**: No spaces in skills string (`"Vietnamese, English"` should be `"Vietnamese,English"`)
3. **Invalid Skills**: Check enum values before parsing
4. **Empty Skills**: Handle null/empty skills gracefully

### **Validation Checklist**
- ✅ Skills string format: comma-separated, no spaces
- ✅ All skills exist in TourGuideSkill enum
- ✅ No duplicate skills in list
- ✅ At least one skill selected for applications

## 📊 Statistics (Current System)

- **Total Skills**: 29
- **Most Common Category**: Languages (8 skills)
- **ID Ranges**: 
  - Languages: 1-8
  - Knowledge: 101-108  
  - Activities: 201-208
  - Special: 301-305

---

**Quick Access**: Use Ctrl+F to search for specific skill names or IDs  
**Last Updated**: 2025-07-02
