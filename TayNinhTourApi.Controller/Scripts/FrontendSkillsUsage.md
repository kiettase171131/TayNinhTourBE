# 🎯 Frontend Skills Usage Guide

## 📋 **Overview**

Skills System sử dụng **string format đơn giản** - Frontend chỉ cần làm việc với skill names như "Vietnamese,English,History".

**Key Points:**
- ✅ **No enum numbers** - chỉ cần skill names
- ✅ **Comma-separated** - "Vietnamese,English,History"
- ✅ **Validated** - API validation đảm bảo data integrity
- ✅ **User-friendly** - Display names tiếng Việt

## ⚡ **Quick Start**

```typescript
// 1. Get available skills
const response = await fetch('/api/skill/categories');
const skillsData = await response.json();

// 2. Create skills string
const selectedSkills = ['Vietnamese', 'English', 'History'];
const skillsString = selectedSkills.join(','); // "Vietnamese,English,History"

// 3. Validate skills
const validateResponse = await fetch('/api/skill/validate', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify(skillsString)
});

// 4. Submit form
const formData = {
  skillsRequired: skillsString, // or skills: skillsString for tour guide
  // ... other fields
};
```

## 🔧 **Available Skills**

### **Languages (Ngôn ngữ)**
- `Vietnamese` - Tiếng Việt
- `English` - Tiếng Anh  
- `Chinese` - Tiếng Trung
- `Japanese` - Tiếng Nhật
- `Korean` - Tiếng Hàn
- `French` - Tiếng Pháp
- `German` - Tiếng Đức
- `Russian` - Tiếng Nga

### **Knowledge (Kiến thức chuyên môn)**
- `History` - Lịch sử
- `Culture` - Văn hóa
- `Religion` - Tôn giáo
- `Cuisine` - Ẩm thực
- `Geography` - Địa lý
- `Nature` - Thiên nhiên
- `Arts` - Nghệ thuật
- `Architecture` - Kiến trúc

### **Activities (Kỹ năng hoạt động)**
- `MountainClimbing` - Leo núi
- `Trekking` - Trekking
- `Photography` - Chụp ảnh
- `WaterSports` - Thể thao nước
- `Cycling` - Đi xe đạp
- `Camping` - Cắm trại
- `BirdWatching` - Quan sát chim
- `AdventureSports` - Thể thao mạo hiểm

### **Special (Kỹ năng đặc biệt)**
- `FirstAid` - Sơ cứu
- `Driving` - Lái xe
- `Cooking` - Nấu ăn
- `Meditation` - Hướng dẫn thiền
- `TraditionalMassage` - Massage truyền thống

## 🚀 **API Endpoints**

### **1. Get Skills Categories**
```http
GET /api/skill/categories
```

**Response:**
```json
{
  "data": {
    "languages": [
      {
        "skill": 1,
        "displayName": "Tiếng Việt",
        "englishName": "Vietnamese",
        "category": "Ngôn ngữ"
      }
    ],
    "knowledge": [...],
    "activities": [...],
    "special": [...]
  }
}
```

### **2. Validate Skills String**
```http
POST /api/skill/validate
Content-Type: application/json

"Vietnamese,English,History,MountainClimbing"
```

**Response:**
```json
{
  "data": true,
  "message": "Skills string hợp lệ",
  "success": true
}
```

## 💻 **Frontend Implementation**

### **React/Vue/Angular Example**

```typescript
// 1. Get available skills
const getSkillsCategories = async () => {
  const response = await fetch('/api/skill/categories');
  const data = await response.json();
  return data.data; // { languages: [...], knowledge: [...], ... }
};

// 2. Create skills selector
const createSkillsString = (selectedSkills: string[]) => {
  return selectedSkills.join(',');
};

// 3. Parse skills string
const parseSkillsString = (skillsString: string) => {
  return skillsString ? skillsString.split(',') : [];
};

// 4. Validate skills
const validateSkills = async (skillsString: string) => {
  const response = await fetch('/api/skill/validate', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(skillsString)
  });
  const data = await response.json();
  return data.success;
};

// 5. Submit tour guide application
const submitApplication = async (formData: any) => {
  const skillsString = createSkillsString(formData.selectedSkills);
  
  const applicationData = {
    ...formData,
    skills: skillsString // "Vietnamese,English,History,MountainClimbing"
  };
  
  // Submit to API...
};

// 6. Create tour details
const createTourDetail = async (formData: any) => {
  const skillsString = createSkillsString(formData.requiredSkills);
  
  const tourDetailData = {
    title: formData.title,
    description: formData.description,
    skillsRequired: skillsString, // "Vietnamese,English,History"
    specialtyShopIds: formData.shopIds
  };
  
  // Submit to API...
};
```

### **Skills Selector Component Example**

```typescript
interface SkillsSelectorProps {
  selectedSkills: string[];
  onSkillsChange: (skills: string[]) => void;
}

const SkillsSelector: React.FC<SkillsSelectorProps> = ({ 
  selectedSkills, 
  onSkillsChange 
}) => {
  const [skillsCategories, setSkillsCategories] = useState(null);

  useEffect(() => {
    getSkillsCategories().then(setSkillsCategories);
  }, []);

  const handleSkillToggle = (skillName: string) => {
    const newSkills = selectedSkills.includes(skillName)
      ? selectedSkills.filter(s => s !== skillName)
      : [...selectedSkills, skillName];
    
    onSkillsChange(newSkills);
  };

  return (
    <div className="skills-selector">
      {skillsCategories && Object.entries(skillsCategories).map(([category, skills]) => (
        <div key={category} className="skill-category">
          <h3>{category}</h3>
          {skills.map(skill => (
            <label key={skill.englishName}>
              <input
                type="checkbox"
                checked={selectedSkills.includes(skill.englishName)}
                onChange={() => handleSkillToggle(skill.englishName)}
              />
              {skill.displayName}
            </label>
          ))}
        </div>
      ))}
    </div>
  );
};
```

## ✅ **Best Practices**

1. **Always validate** skills string before submitting
2. **Use skill names** exactly as provided by API
3. **Handle empty skills** gracefully (empty string or null)
4. **Cache skills categories** to avoid repeated API calls
5. **Provide user-friendly** skill selection interface

## 🎉 **Summary**

- ✅ **Simple**: Chỉ cần string format "Vietnamese,English,History"
- ✅ **Readable**: Skill names dễ hiểu và debug
- ✅ **Flexible**: Dễ dàng thêm/bớt skills
- ✅ **Validated**: API validation đảm bảo data integrity
- ✅ **User-friendly**: Display names tiếng Việt cho UI
