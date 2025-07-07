# TayNinhTour Frontend API Integration Guide

## 🚨 QUAN TRỌNG: Field Mapping Issues & Solutions

### Vấn đề hiện tại với Timeline API

**❌ Vấn đề**: Frontend gửi `shopId` nhưng Backend không lưu vào `specialtyShopId`

**🔍 Root Cause**:
1. Frontend gửi field `shopId` trong request
2. Backend entity `TimelineItem` có field `SpecialtyShopId` 
3. Service layer không map `shopId` → `SpecialtyShopId` (có TODO comment)
4. AutoMapper thiếu mapping cho SpecialtyShop relationship

---

## 📋 API Endpoints & Correct Field Mapping

### 1. Create Timeline Items (Bulk)

**Endpoint**: `POST /api/TourDetails/timeline`

**❌ Frontend hiện tại gửi**:
```json
{
  "tourDetailsId": "c90a4b34-d800-43d6-9c1e-f8234e0a574e",
  "timelineItems": [
    {
      "checkInTime": "01:00",
      "activity": "cao dai special shop", 
      "shopId": "f6a7b8c9-d0e1-2345-fab6-678901234567",  // ❌ Sai field name
      "sortOrder": 1
    }
  ]
}
```

**✅ Frontend cần gửi**:
```json
{
  "tourDetailsId": "c90a4b34-d800-43d6-9c1e-f8234e0a574e",
  "timelineItems": [
    {
      "checkInTime": "01:00",
      "activity": "cao dai special shop", 
      "specialtyShopId": "f6a7b8c9-d0e1-2345-fab6-678901234567",  // ✅ Đúng field name
      "sortOrder": 1
    }
  ]
}
```

**Response hiện tại**:
```json
{
  "statusCode": 201,
  "message": "Tạo 1 timeline items thành công",
  "success": true,
  "data": [
    {
      "id": "32da4669-30d7-4e1b-a19a-f095caac5f4d",
      "tourDetailsId": "c90a4b34-d800-43d6-9c1e-f8234e0a574e",
      "checkInTime": "01:00",
      "activity": "cao dai special shop",
      "specialtyShopId": null,  // ❌ Null vì không map đúng
      "sortOrder": 1,
      "specialtyShop": null,    // ❌ Null vì không có specialtyShopId
      "createdAt": "2025-07-06T05:33:14",
      "updatedAt": "2025-07-06T05:33:14"
    }
  ]
}
```

### 2. Create Single Timeline Item

**Endpoint**: `POST /api/TourDetails/timeline/single`

**✅ Correct Request**:
```json
{
  "tourDetailsId": "c90a4b34-d800-43d6-9c1e-f8234e0a574e",
  "checkInTime": "02:00",
  "activity": "Ghé shop bánh tráng",
  "specialtyShopId": "f6a7b8c9-d0e1-2345-fab6-678901234567",  // ✅ Đúng field
  "sortOrder": 2
}
```

### 3. Get Timeline by TourDetails

**Endpoint**: `GET /api/TourDetails/{tourDetailsId}/timeline`

**Query Parameters**:
- `includeInactive`: boolean (default: false)
- `includeShopInfo`: boolean (default: true)

**Response khi fix**:
```json
{
  "statusCode": 200,
  "message": "Lấy timeline thành công",
  "success": true,
  "data": {
    "tourTemplateId": "b740b8a6-716f-41a6-a7e7-f7f9e09d7925",
    "tourTemplateTitle": "Tour Núi Bà Đen",
    "items": [
      {
        "id": "32da4669-30d7-4e1b-a19a-f095caac5f4d",
        "tourDetailsId": "c90a4b34-d800-43d6-9c1e-f8234e0a574e",
        "checkInTime": "01:00",
        "activity": "cao dai special shop",
        "specialtyShopId": "f6a7b8c9-d0e1-2345-fab6-678901234567",  // ✅ Có giá trị
        "sortOrder": 1,
        "specialtyShop": {  // ✅ Có thông tin shop
          "id": "f6a7b8c9-d0e1-2345-fab6-678901234567",
          "shopName": "Cao Dai Special Shop",
          "address": "Tây Ninh",
          "phoneNumber": "0123456789",
          "shopType": "Restaurant",
          "userName": "Shop Owner",
          "userEmail": "shop@example.com"
        },
        "createdAt": "2025-07-06T05:33:14",
        "updatedAt": "2025-07-06T05:33:14"
      }
    ],
    "totalItems": 1,
    "startLocation": "TP.HCM",
    "endLocation": "Tây Ninh"
  }
}
```

---

## ✅ Backend Fixes Completed

### 1. ✅ Service Layer Fixed

**File**: `TayNinhTourApi.BusinessLogicLayer\Services\TourDetailsService.cs`

**Fixed** (lines 851 & 965):
```csharp
SpecialtyShopId = request.SpecialtyShopId,        // ✅ Single item creation
SpecialtyShopId = itemRequest.SpecialtyShopId,    // ✅ Bulk items creation
```

### 2. ✅ Request DTOs Fixed

**Files**: `RequestCreateTimelineItemDto.cs`, `TimelineItemCreateDto.cs`, `RequestUpdateTimelineItemDto.cs`

**Fixed**:
```csharp
public Guid? SpecialtyShopId { get; set; }  // ✅ Correct field name
```

### 3. ✅ AutoMapper Fixed

**File**: `TayNinhTourApi.BusinessLogicLayer\Mapping\MappingProfile.cs`

**Fixed**:
```csharp
CreateMap<TimelineItem, TimelineItemDto>()
    .ForMember(dest => dest.CheckInTime, opt => opt.MapFrom(src => src.CheckInTime.ToString(@"hh\:mm")))
    .ForMember(dest => dest.SpecialtyShop, opt => opt.MapFrom(src => src.SpecialtyShop));  // ✅ Enabled mapping
```

---

## 📝 Frontend Implementation Checklist

### ✅ Immediate Actions Required

1. **Update all Timeline API calls**:
   - Change `shopId` → `specialtyShopId` in request payloads
   - Update TypeScript interfaces/types
   - Update form field names

2. **Update Response Handling**:
   - Expect `specialtyShopId` instead of `shopId` in responses
   - Handle `specialtyShop` object in timeline items
   - Update UI components to display shop information

3. **Test Cases to Verify**:
   - Create timeline with shop: verify `specialtyShopId` is saved
   - Create timeline without shop: verify `specialtyShopId` is null
   - Get timeline: verify shop information is populated
   - Update timeline: verify shop changes are saved

### 🔍 Field Mapping Reference

| Frontend Field | Backend Entity Field | DTO Field | Notes |
|---|---|---|---|
| `specialtyShopId` | `SpecialtyShopId` | `SpecialtyShopId` | ✅ Correct mapping |
| ~~`shopId`~~ | ~~N/A~~ | ~~`ShopId`~~ | ❌ Legacy, remove |

---

## 🚀 Testing Endpoints

### Authentication Required
All timeline endpoints require JWT token with appropriate role:
- **Tour Company**: Can manage their own tour details
- **Admin**: Can manage all tour details

### Sample Test Flow

1. **Login as Tour Company**:
```bash
POST /api/Authentication/login
{
  "email": "tourcompany@gmail.com",
  "password": "12345678h@"
}
```

2. **Create TourDetails** (if needed):
```bash
POST /api/TourDetails
{
  "tourTemplateId": "template-id",
  "title": "Test Tour",
  "description": "Test Description"
}
```

3. **Add Timeline with Shop**:
```bash
POST /api/TourDetails/timeline
{
  "tourDetailsId": "tour-details-id",
  "timelineItems": [
    {
      "checkInTime": "09:00",
      "activity": "Visit specialty shop",
      "specialtyShopId": "shop-id",  // ✅ Use this field
      "sortOrder": 1
    }
  ]
}
```

4. **Verify Timeline**:
```bash
GET /api/TourDetails/{tourDetailsId}/timeline?includeShopInfo=true
```

---

## 📞 Support

Nếu gặp vấn đề khi implement, hãy kiểm tra:
1. Field names trong request payload
2. JWT token có đúng role không
3. TourDetails có tồn tại không
4. SpecialtyShop ID có hợp lệ không

**Backend Developer**: Cần fix service layer và AutoMapper trước khi frontend test
**Frontend Developer**: Cần update field names theo guide này

---

## 📚 Complete API Reference

### TourDetails Management

#### Create TourDetails
```bash
POST /api/TourDetails
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "tourTemplateId": "b740b8a6-716f-41a6-a7e7-f7f9e09d7925",
  "title": "Tour Núi Bà Đen - Khám phá",
  "description": "Tour khám phá Núi Bà Đen với các điểm tham quan đặc sắc",
  "skillsRequired": "Photography,Communication"
}
```

#### Get TourDetails by ID
```bash
GET /api/TourDetails/{id}
Authorization: Bearer {jwt-token}
```

#### Update TourDetails
```bash
PATCH /api/TourDetails/{id}
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "title": "Updated Title",
  "description": "Updated Description"
}
```

### Timeline Management

#### Update Timeline Item
```bash
PATCH /api/TourDetails/timeline/{timelineItemId}
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "checkInTime": "10:30",
  "activity": "Updated activity",
  "specialtyShopId": "new-shop-id",
  "sortOrder": 2
}
```

#### Delete Timeline Item
```bash
DELETE /api/TourDetails/timeline/{timelineItemId}
Authorization: Bearer {jwt-token}
```

#### Reorder Timeline Items
```bash
POST /api/TourDetails/timeline/reorder
Authorization: Bearer {jwt-token}
Content-Type: application/json

{
  "tourDetailsId": "tour-details-id",
  "timelineOrders": [
    {
      "timelineItemId": "item-1-id",
      "newSortOrder": 1
    },
    {
      "timelineItemId": "item-2-id",
      "newSortOrder": 2
    }
  ]
}
```

### SpecialtyShop Management

#### Get Available Shops
```bash
GET /api/SpecialtyShop/active
Authorization: Bearer {jwt-token}
```

#### Get Shops by Type
```bash
GET /api/SpecialtyShop/by-type/{shopType}
Authorization: Bearer {jwt-token}

# shopType values: Restaurant, Souvenir, Entertainment, Accommodation
```

#### Search Shops
```bash
GET /api/SpecialtyShop/search?keyword=bánh tráng&location=Tây Ninh
Authorization: Bearer {jwt-token}
```

---

## 🎯 Frontend TypeScript Interfaces

### Request Interfaces
```typescript
interface CreateTimelineItemsRequest {
  tourDetailsId: string;
  timelineItems: TimelineItemCreate[];
}

interface TimelineItemCreate {
  checkInTime: string;  // Format: "HH:mm"
  activity: string;
  specialtyShopId?: string;  // ✅ Correct field name
  sortOrder?: number;
}

interface CreateSingleTimelineItemRequest {
  tourDetailsId: string;
  checkInTime: string;
  activity: string;
  specialtyShopId?: string;  // ✅ Correct field name
  sortOrder?: number;
}
```

### Response Interfaces
```typescript
interface TimelineResponse {
  statusCode: number;
  message: string;
  success: boolean;
  data: Timeline;
}

interface Timeline {
  tourTemplateId: string;
  tourTemplateTitle: string;
  items: TimelineItem[];
  totalItems: number;
  startLocation: string;
  endLocation: string;
  createdAt: string;
  updatedAt: string;
}

interface TimelineItem {
  id: string;
  tourDetailsId: string;
  checkInTime: string;
  activity: string;
  specialtyShopId?: string;  // ✅ Correct field name
  sortOrder: number;
  specialtyShop?: SpecialtyShop;  // ✅ Shop information
  createdAt: string;
  updatedAt?: string;
}

interface SpecialtyShop {
  id: string;
  shopName: string;
  address: string;
  phoneNumber: string;
  shopType: string;
  userName: string;
  userEmail: string;
  userAvatar?: string;
  userRole: string;
}
```

---

## 🔄 Migration Guide for Existing Frontend Code

### Step 1: Update API Service Functions

**Before**:
```typescript
const createTimelineItems = async (data: any) => {
  return await api.post('/api/TourDetails/timeline', {
    tourDetailsId: data.tourDetailsId,
    timelineItems: data.items.map(item => ({
      checkInTime: item.time,
      activity: item.activity,
      shopId: item.shopId,  // ❌ Wrong field
      sortOrder: item.order
    }))
  });
};
```

**After**:
```typescript
const createTimelineItems = async (data: CreateTimelineItemsRequest) => {
  return await api.post<TimelineResponse>('/api/TourDetails/timeline', {
    tourDetailsId: data.tourDetailsId,
    timelineItems: data.timelineItems.map(item => ({
      checkInTime: item.checkInTime,
      activity: item.activity,
      specialtyShopId: item.specialtyShopId,  // ✅ Correct field
      sortOrder: item.sortOrder
    }))
  });
};
```

### Step 2: Update Form Components

**Before**:
```typescript
const [formData, setFormData] = useState({
  time: '',
  activity: '',
  shopId: '',  // ❌ Wrong field
  order: 1
});
```

**After**:
```typescript
const [formData, setFormData] = useState<TimelineItemCreate>({
  checkInTime: '',
  activity: '',
  specialtyShopId: '',  // ✅ Correct field
  sortOrder: 1
});
```

### Step 3: Update Response Handling

**Before**:
```typescript
const handleResponse = (response: any) => {
  const items = response.data.map((item: any) => ({
    id: item.id,
    time: item.checkInTime,
    activity: item.activity,
    shopId: item.specialtyShopId,  // ❌ Inconsistent naming
    shop: item.specialtyShop
  }));
};
```

**After**:
```typescript
const handleResponse = (response: TimelineResponse) => {
  const items = response.data.items.map((item: TimelineItem) => ({
    id: item.id,
    checkInTime: item.checkInTime,
    activity: item.activity,
    specialtyShopId: item.specialtyShopId,  // ✅ Consistent naming
    specialtyShop: item.specialtyShop
  }));
};
```

---

## ⚠️ Common Pitfalls & Solutions

### 1. Field Name Confusion
**Problem**: Mixing `shopId` and `specialtyShopId`
**Solution**: Always use `specialtyShopId` in all API calls

### 2. Time Format Issues
**Problem**: Sending invalid time format
**Solution**: Always use "HH:mm" format (e.g., "09:00", "14:30")

### 3. Missing Authorization
**Problem**: 401 Unauthorized errors
**Solution**: Include JWT token in Authorization header

### 4. Invalid Shop References
**Problem**: Timeline created but shop info is null
**Solution**: Verify `specialtyShopId` exists before sending request

### 5. Sort Order Conflicts
**Problem**: Duplicate sort orders causing display issues
**Solution**: Let backend auto-assign if not specified, or ensure unique values

---

## 📋 Testing Checklist

### Backend Fixes Verification
- [ ] Service layer maps `specialtyShopId` correctly
- [ ] AutoMapper includes SpecialtyShop relationship
- [ ] DTOs use consistent field names
- [ ] Database saves `specialtyShopId` values

### Frontend Implementation
- [ ] All API calls use `specialtyShopId` field
- [ ] TypeScript interfaces updated
- [ ] Form components use correct field names
- [ ] Response handling expects correct structure
- [ ] Error handling for invalid shop IDs

### Integration Testing
- [ ] Create timeline with shop - shop info appears
- [ ] Create timeline without shop - no errors
- [ ] Update timeline shop - changes persist
- [ ] Delete timeline item - no orphaned data
- [ ] Reorder timeline - sort order updates

---

## 🆘 Troubleshooting

### Issue: `specialtyShopId` is always null in response
**Cause**: Backend service not mapping field correctly
**Fix**: Update service layer to set `SpecialtyShopId = itemRequest.SpecialtyShopId`

### Issue: Shop information not populated
**Cause**: AutoMapper not including SpecialtyShop relationship
**Fix**: Enable SpecialtyShop mapping in MappingProfile.cs

### Issue: 400 Bad Request on timeline creation
**Cause**: Invalid field names or data types
**Fix**: Verify request payload matches DTO structure exactly

### Issue: Timeline items appear in wrong order
**Cause**: SortOrder conflicts or missing values
**Fix**: Ensure unique SortOrder values or let backend auto-assign
