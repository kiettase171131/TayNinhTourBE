# 📚 Timeline Architecture: Progress Tracking vs Template System

## 🎯 **Tổng Quan Kiến Trúc**

### **Nguyên Tắc Cốt Lõi:**
```
Timeline Template (1) + Progress Tracking (N) = Complete Timeline Experience
```

**Timeline Template**: Định nghĩa CÁI GÌ cần làm (Activity, Time, Order)
**Progress Tracking**: Theo dõi ĐÃ LÀM GÌ (Completed, When, Notes)

## 🏗️ **PHẦN 1: TIMELINE TEMPLATE (KHÔNG DUPLICATE)**

### **1.1 Định Nghĩa Timeline Template**

Timeline Template là **bản thiết kế** của tour, định nghĩa:
- Các hoạt động cần thực hiện
- Thời gian dự kiến cho mỗi hoạt động  
- Thứ tự thực hiện
- Địa điểm/cửa hàng liên quan

```sql
-- TimelineItem Table (TEMPLATE)
CREATE TABLE TimelineItem (
    Id CHAR(36) PRIMARY KEY,
    TourDetailsId CHAR(36) NOT NULL,        -- Thuộc về TourDetails nào
    Activity VARCHAR(500) NOT NULL,         -- Hoạt động gì
    CheckInTime TIME NOT NULL,              -- Thời gian dự kiến
    SortOrder INT NOT NULL,                 -- Thứ tự thực hiện
    SpecialtyShopId CHAR(36) NULL,         -- Cửa hàng liên quan (nếu có)
    CreatedAt DATETIME NOT NULL,
    CreatedById CHAR(36) NOT NULL,
    UpdatedAt DATETIME NULL,
    UpdatedById CHAR(36) NULL,
    IsActive BOOLEAN DEFAULT TRUE,
    
    -- ❌ REMOVED: IsCompleted, CompletedAt, CompletionNotes
    -- → Moved to TourSlotTimelineProgress
    
    FOREIGN KEY (TourDetailsId) REFERENCES TourDetails(Id),
    FOREIGN KEY (SpecialtyShopId) REFERENCES SpecialtyShop(Id)
);
```

### **1.2 Ví Dụ Timeline Template**

```sql
-- Tour "Khám Phá Tây Ninh" có 1 bộ timeline template duy nhất
INSERT INTO TimelineItem VALUES
('tl-1', 'tour-tayninhexplore', 'Tập trung tại điểm đón',           '07:30:00', 1, NULL, ...),
('tl-2', 'tour-tayninhexplore', 'Di chuyển đến Núi Bà Đen',        '08:00:00', 2, NULL, ...),
('tl-3', 'tour-tayninhexplore', 'Leo núi và tham quan',            '09:00:00', 3, NULL, ...),
('tl-4', 'tour-tayninhexplore', 'Thăm cửa hàng đặc sản',          '11:30:00', 4, 'shop-dacsan', ...),
('tl-5', 'tour-tayninhexplore', 'Ăn trưa tại nhà hàng địa phương', '12:30:00', 5, 'shop-nhahang', ...),
('tl-6', 'tour-tayninhexplore', 'Thăm Tòa Thánh Tây Ninh',        '14:00:00', 6, NULL, ...),
('tl-7', 'tour-tayninhexplore', 'Về điểm đón ban đầu',            '16:00:00', 7, NULL, ...);
```

### **1.3 Đặc Điểm Timeline Template**

#### **✅ Centralized (Tập Trung)**
- 1 TourDetails = 1 bộ Timeline Template duy nhất
- Tất cả TourSlots của cùng TourDetails dùng chung template
- Dễ dàng update timeline cho tất cả slots cùng lúc

#### **✅ Immutable During Tour (Bất Biến Khi Tour Diễn Ra)**
- Template không thay đổi khi tour đang diễn ra
- Đảm bảo consistency cho tất cả slots
- Audit trail rõ ràng

#### **✅ Reusable (Tái Sử Dụng)**
- Template có thể được copy cho TourDetails khác
- Standardize tour experience
- Easy template management

### **1.4 Timeline Template Relationships**

```
TourDetails (1) ──→ (N) TimelineItem
TimelineItem (N) ──→ (1) SpecialtyShop [Optional]
TimelineItem (1) ──→ (N) TourSlotTimelineProgress
```

## 📊 **PHẦN 2: PROGRESS TRACKING (PER TOURSLOT)**

### **2.1 Định Nghĩa Progress Tracking**

Progress Tracking theo dõi **tiến độ thực tế** của từng TourSlot:
- Timeline item nào đã hoàn thành
- Khi nào hoàn thành
- Ai hoàn thành
- Ghi chú khi hoàn thành

```sql
-- TourSlotTimelineProgress Table (PROGRESS TRACKING)
CREATE TABLE TourSlotTimelineProgress (
    Id CHAR(36) PRIMARY KEY,
    
    -- Liên kết
    TourSlotId CHAR(36) NOT NULL,           -- Slot cụ thể nào
    TimelineItemId CHAR(36) NOT NULL,       -- Timeline item nào (reference to template)
    
    -- Progress Information
    IsCompleted BOOLEAN DEFAULT FALSE,       -- Đã hoàn thành chưa
    CompletedAt DATETIME NULL,              -- Khi nào hoàn thành
    CompletionNotes VARCHAR(500) NULL,      -- Ghi chú khi hoàn thành
    
    -- Audit Trail
    CreatedAt DATETIME NOT NULL,            -- Khi tạo progress record
    CreatedById CHAR(36) NOT NULL,         -- Ai tạo (thường là system)
    UpdatedAt DATETIME NULL,                -- Khi update lần cuối
    UpdatedById CHAR(36) NULL,             -- Ai update (thường là HDV)
    IsActive BOOLEAN DEFAULT TRUE,
    
    -- Constraints
    UNIQUE KEY UK_TourSlotTimeline (TourSlotId, TimelineItemId),
    FOREIGN KEY (TourSlotId) REFERENCES TourSlot(Id) ON DELETE CASCADE,
    FOREIGN KEY (TimelineItemId) REFERENCES TimelineItem(Id) ON DELETE CASCADE,
    FOREIGN KEY (CreatedById) REFERENCES User(Id),
    FOREIGN KEY (UpdatedById) REFERENCES User(Id)
);
```

### **2.2 Ví Dụ Progress Tracking**

```sql
-- Tour "Khám Phá Tây Ninh" có 3 slots khác nhau với progress khác nhau

-- SLOT A: 2025-01-15 (Thứ 2) - HDV Nguyễn Văn A - Progress: 4/7 completed
INSERT INTO TourSlotTimelineProgress VALUES
('pg-1', 'slot-20250115', 'tl-1', TRUE,  '2025-01-15 07:35:00', 'Khách đã tập trung đầy đủ', ...),
('pg-2', 'slot-20250115', 'tl-2', TRUE,  '2025-01-15 08:15:00', 'Di chuyển đúng giờ', ...),
('pg-3', 'slot-20250115', 'tl-3', TRUE,  '2025-01-15 10:30:00', 'Leo núi thành công, thời tiết đẹp', ...),
('pg-4', 'slot-20250115', 'tl-4', TRUE,  '2025-01-15 11:45:00', 'Khách mua nhiều đặc sản', ...),
('pg-5', 'slot-20250115', 'tl-5', FALSE, NULL, NULL, ...),                    -- Chưa ăn trưa
('pg-6', 'slot-20250115', 'tl-6', FALSE, NULL, NULL, ...),                    -- Chưa thăm Tòa Thánh
('pg-7', 'slot-20250115', 'tl-7', FALSE, NULL, NULL, ...);                    -- Chưa về

-- SLOT B: 2025-01-18 (Thứ 5) - HDV Trần Thị B - Progress: 0/7 completed (chưa bắt đầu)
INSERT INTO TourSlotTimelineProgress VALUES
('pg-8',  'slot-20250118', 'tl-1', FALSE, NULL, NULL, ...),
('pg-9',  'slot-20250118', 'tl-2', FALSE, NULL, NULL, ...),
('pg-10', 'slot-20250118', 'tl-3', FALSE, NULL, NULL, ...),
('pg-11', 'slot-20250118', 'tl-4', FALSE, NULL, NULL, ...),
('pg-12', 'slot-20250118', 'tl-5', FALSE, NULL, NULL, ...),
('pg-13', 'slot-20250118', 'tl-6', FALSE, NULL, NULL, ...),
('pg-14', 'slot-20250118', 'tl-7', FALSE, NULL, NULL, ...);

-- SLOT C: 2025-01-22 (Chủ nhật) - HDV Lê Văn C - Progress: 7/7 completed (hoàn thành)
INSERT INTO TourSlotTimelineProgress VALUES
('pg-15', 'slot-20250122', 'tl-1', TRUE, '2025-01-22 07:28:00', 'Khách đến sớm', ...),
('pg-16', 'slot-20250122', 'tl-2', TRUE, '2025-01-22 07:55:00', 'Xuất phát sớm 5 phút', ...),
('pg-17', 'slot-20250122', 'tl-3', TRUE, '2025-01-22 09:45:00', 'Thời tiết lý tưởng', ...),
('pg-18', 'slot-20250122', 'tl-4', TRUE, '2025-01-22 11:20:00', 'Cửa hàng phục vụ tốt', ...),
('pg-19', 'slot-20250122', 'tl-5', TRUE, '2025-01-22 12:25:00', 'Món ăn ngon, khách hài lòng', ...),
('pg-20', 'slot-20250122', 'tl-6', TRUE, '2025-01-22 14:30:00', 'Tham quan trang trọng', ...),
('pg-21', 'slot-20250122', 'tl-7', TRUE, '2025-01-22 15:50:00', 'Hoàn thành tour thành công', ...);
```

### **2.3 Đặc Điểm Progress Tracking**

#### **✅ Independent (Độc Lập)**
- Mỗi TourSlot có progress riêng biệt
- Không ảnh hưởng lẫn nhau
- HDV có thể làm việc song song

#### **✅ Real-time (Thời Gian Thực)**
- Progress được update ngay khi HDV complete
- Khách hàng thấy tiến độ real-time
- Admin có thể monitor tất cả tours

#### **✅ Auditable (Có Thể Kiểm Tra)**
- Đầy đủ thông tin ai, khi nào, làm gì
- Ghi chú chi tiết cho mỗi completion
- Truy vết được toàn bộ quá trình

### **2.4 Progress Tracking Relationships**

```
TourSlot (1) ──→ (N) TourSlotTimelineProgress
TimelineItem (1) ──→ (N) TourSlotTimelineProgress
User (1) ──→ (N) TourSlotTimelineProgress [CreatedBy, UpdatedBy]
```

## 🔄 **PHẦN 3: CÁCH TEMPLATE VÀ PROGRESS HOẠT ĐỘNG CÙNG NHAU**

### **3.1 Quy Trình Tạo Progress Records**

#### **Bước 1: TourSlot Được Assign TourDetails**
```sql
-- Khi admin assign TourDetails cho TourSlot
UPDATE TourSlot 
SET TourDetailsId = 'tour-tayninhexplore', UpdatedById = 'admin-123'
WHERE Id = 'slot-20250115';

-- Trigger tự động chạy
TRIGGER TR_TourSlot_CreateTimelineProgress FIRES
```

#### **Bước 2: Auto-Create Progress Records**
```sql
-- Stored procedure tự động tạo progress records
CALL CreateTimelineProgressForTourSlot('slot-20250115', 'admin-123');

-- Tạo 1 progress record cho mỗi timeline item của tour
INSERT INTO TourSlotTimelineProgress (Id, TourSlotId, TimelineItemId, IsCompleted, CreatedAt, CreatedById)
SELECT UUID(), 'slot-20250115', ti.Id, FALSE, NOW(), 'admin-123'
FROM TimelineItem ti 
WHERE ti.TourDetailsId = 'tour-tayninhexplore' AND ti.IsActive = TRUE;
```

#### **Bước 3: HDV Complete Timeline Items**
```sql
-- HDV complete timeline item đầu tiên
UPDATE TourSlotTimelineProgress 
SET IsCompleted = TRUE, 
    CompletedAt = '2025-01-15 07:35:00',
    CompletionNotes = 'Khách đã tập trung đầy đủ',
    UpdatedAt = NOW(),
    UpdatedById = 'tourguide-nguyen-van-a'
WHERE TourSlotId = 'slot-20250115' AND TimelineItemId = 'tl-1';
```

### **3.2 Query Timeline Với Progress**

#### **API Endpoint: GET /tour-slot/{tourSlotId}/timeline**
```csharp
public async Task<TimelineProgressResponse> GetTimelineWithProgress(Guid tourSlotId)
{
    // Step 1: Get TourSlot with TourDetails
    var tourSlot = await _context.TourSlots
        .Include(ts => ts.TourDetails)
        .FirstOrDefaultAsync(ts => ts.Id == tourSlotId);
    
    // Step 2: Get Timeline Template
    var timelineTemplate = await _context.TimelineItems
        .Where(ti => ti.TourDetailsId == tourSlot.TourDetailsId)
        .Include(ti => ti.SpecialtyShop)
        .OrderBy(ti => ti.SortOrder)
        .ToListAsync();
    
    // Step 3: Get Progress Data
    var progressData = await _context.TourSlotTimelineProgress
        .Where(tp => tp.TourSlotId == tourSlotId)
        .Include(tp => tp.UpdatedBy)
        .ToDictionaryAsync(tp => tp.TimelineItemId);
    
    // Step 4: Merge Template + Progress
    var timelineWithProgress = timelineTemplate.Select((template, index) => {
        var progress = progressData.GetValueOrDefault(template.Id);
        
        return new TimelineWithProgressDto {
            // Template Data (từ TimelineItem)
            Id = template.Id,
            Activity = template.Activity,
            CheckInTime = template.CheckInTime,
            SortOrder = template.SortOrder,
            SpecialtyShop = template.SpecialtyShop != null 
                ? _mapper.Map<SpecialtyShopDto>(template.SpecialtyShop) 
                : null,
            
            // Progress Data (từ TourSlotTimelineProgress)
            TourSlotId = tourSlotId,
            ProgressId = progress?.Id,
            IsCompleted = progress?.IsCompleted ?? false,
            CompletedAt = progress?.CompletedAt,
            CompletionNotes = progress?.CompletionNotes,
            CompletedByName = progress?.UpdatedBy?.Name,
            
            // Calculated Fields
            Position = index + 1,
            TotalItems = timelineTemplate.Count,
            CanComplete = CanCompleteTimelineItem(tourSlotId, template.Id),
            IsNext = !progress?.IsCompleted == true && CanCompleteTimelineItem(tourSlotId, template.Id)
        };
    }).ToList();
    
    // Step 5: Calculate Summary
    var summary = new TimelineProgressSummaryDto {
        TourSlotId = tourSlotId,
        TotalItems = timelineWithProgress.Count,
        CompletedItems = timelineWithProgress.Count(t => t.IsCompleted),
        NextItem = timelineWithProgress.FirstOrDefault(t => t.IsNext),
        LastCompletedItem = timelineWithProgress.LastOrDefault(t => t.IsCompleted)
    };
    
    return new TimelineProgressResponse {
        Timeline = timelineWithProgress,
        Summary = summary,
        TourSlot = _mapper.Map<TourSlotInfoDto>(tourSlot),
        TourDetails = _mapper.Map<TourDetailsInfoDto>(tourSlot.TourDetails),
        CanModifyProgress = true,
        LastUpdated = DateTime.UtcNow
    };
}
```

### **3.3 Sequential Completion Logic**

```csharp
public async Task<bool> CanCompleteTimelineItem(Guid tourSlotId, Guid timelineItemId)
{
    // Get timeline item để biết SortOrder
    var timelineItem = await _context.TimelineItems
        .FirstOrDefaultAsync(ti => ti.Id == timelineItemId);
    
    if (timelineItem == null) return false;
    
    // Check xem có timeline items nào trước đó chưa complete không
    var incompleteEarlierItems = await _context.TourSlotTimelineProgress
        .Include(tp => tp.TimelineItem)
        .Where(tp => tp.TourSlotId == tourSlotId &&
                    tp.TimelineItem.TourDetailsId == timelineItem.TourDetailsId &&
                    tp.TimelineItem.SortOrder < timelineItem.SortOrder &&
                    !tp.IsCompleted &&
                    tp.IsActive)
        .CountAsync();
    
    return incompleteEarlierItems == 0;
}
```

## 📊 **PHẦN 4: SO SÁNH TRƯỚC VÀ SAU**

### **4.1 Kiến Trúc Cũ (Có Vấn Đề)**

```sql
-- TimelineItem (OLD) - Progress data mixed with template
TimelineItem:
├── Id, TourDetailsId, Activity, CheckInTime, SortOrder  ← Template data
├── IsCompleted, CompletedAt, CompletionNotes           ← Progress data (PROBLEM!)
└── CreatedBy, UpdatedBy, CreatedAt, UpdatedAt

-- VẤN ĐỀ:
-- TourSlot A complete → TourSlot B, C cũng thấy completed
-- Không thể track progress riêng biệt cho từng slot
-- Dữ liệu progress bị overwrite lẫn nhau
```

### **4.2 Kiến Trúc Mới (Giải Quyết Vấn Đề)**

```sql
-- TimelineItem (NEW) - Pure template
TimelineItem:
├── Id, TourDetailsId, Activity, CheckInTime, SortOrder  ← Template data only
└── CreatedBy, UpdatedBy, CreatedAt, UpdatedAt

-- TourSlotTimelineProgress (NEW) - Pure progress tracking
TourSlotTimelineProgress:
├── Id, TourSlotId, TimelineItemId                      ← Linking
├── IsCompleted, CompletedAt, CompletionNotes           ← Progress data
└── CreatedBy, UpdatedBy, CreatedAt, UpdatedAt

-- GIẢI PHÁP:
-- Mỗi TourSlot có progress records riêng biệt
-- Template và progress tách biệt hoàn toàn
-- Không có data conflict giữa các slots
```

## 🎯 **PHẦN 5: LỢI ÍCH CỦA KIẾN TRÚC MỚI**

### **5.1 Data Integrity (Tính Toàn Vẹn Dữ Liệu)**
- ✅ Template không bị modify bởi progress updates
- ✅ Progress data isolated per TourSlot
- ✅ No data conflicts between slots
- ✅ Consistent timeline experience

### **5.2 Scalability (Khả Năng Mở Rộng)**
- ✅ Easy to add more progress fields
- ✅ Support for complex progress workflows
- ✅ Analytics and reporting capabilities
- ✅ Audit trail for compliance

### **5.3 Maintainability (Khả Năng Bảo Trì)**
- ✅ Clear separation of concerns
- ✅ Easy to update templates
- ✅ Simple progress management
- ✅ Backward compatibility maintained

### **5.4 User Experience (Trải Nghiệm Người Dùng)**
- ✅ Real-time progress updates
- ✅ Independent tour experiences
- ✅ Rich progress information
- ✅ Better tour guide workflow

## 📈 **PHẦN 6: VÍ DỤ THỰC TẾ HOÀN CHỈNH**

### **6.1 Scenario: Tour "Khám Phá Tây Ninh" - 3 Ngày Khác Nhau**

#### **Timeline Template (1 bộ duy nhất):**
```sql
-- TourDetails: "Khám Phá Tây Ninh" (tour-tayninhexplore)
TimelineItem Table:
┌─────────────┬─────────────────────┬─────────────────────────────┬───────────┬───────────┐
│ Id          │ TourDetailsId       │ Activity                    │ CheckInTime│ SortOrder │
├─────────────┼─────────────────────┼─────────────────────────────┼───────────┼───────────┤
│ tl-1        │ tour-tayninhexplore │ Tập trung tại điểm đón      │ 07:30:00  │ 1         │
│ tl-2        │ tour-tayninhexplore │ Di chuyển đến Núi Bà Đen    │ 08:00:00  │ 2         │
│ tl-3        │ tour-tayninhexplore │ Leo núi và tham quan        │ 09:00:00  │ 3         │
│ tl-4        │ tour-tayninhexplore │ Thăm cửa hàng đặc sản       │ 11:30:00  │ 4         │
│ tl-5        │ tour-tayninhexplore │ Ăn trưa tại nhà hàng        │ 12:30:00  │ 5         │
│ tl-6        │ tour-tayninhexplore │ Thăm Tòa Thánh Tây Ninh    │ 14:00:00  │ 6         │
│ tl-7        │ tour-tayninhexplore │ Về điểm đón ban đầu         │ 16:00:00  │ 7         │
└─────────────┴─────────────────────┴─────────────────────────────┴───────────┴───────────┘
```

#### **TourSlots (3 ngày khác nhau):**
```sql
TourSlot Table:
┌─────────────────┬─────────────────────┬────────────┬─────────────────┬────────────────┐
│ Id              │ TourDetailsId       │ TourDate   │ CurrentBookings │ MaxGuests      │
├─────────────────┼─────────────────────┼────────────┼─────────────────┼────────────────┤
│ slot-20250115   │ tour-tayninhexplore │ 2025-01-15 │ 12              │ 15             │
│ slot-20250118   │ tour-tayninhexplore │ 2025-01-18 │ 8               │ 15             │
│ slot-20250122   │ tour-tayninhexplore │ 2025-01-22 │ 15              │ 15             │
└─────────────────┴─────────────────────┴────────────┴─────────────────┴────────────────┘
```

#### **Progress Tracking (21 records = 3 slots × 7 timeline items):**
```sql
TourSlotTimelineProgress Table:
┌─────────────┬─────────────────┬──────────────┬─────────────┬─────────────────────┬──────────────────────────┐
│ Id          │ TourSlotId      │ TimelineItemId│ IsCompleted │ CompletedAt         │ CompletionNotes          │
├─────────────┼─────────────────┼──────────────┼─────────────┼─────────────────────┼──────────────────────────┤
│ pg-1        │ slot-20250115   │ tl-1         │ TRUE        │ 2025-01-15 07:35:00 │ Khách đến đầy đủ         │
│ pg-2        │ slot-20250115   │ tl-2         │ TRUE        │ 2025-01-15 08:15:00 │ Di chuyển đúng giờ       │
│ pg-3        │ slot-20250115   │ tl-3         │ TRUE        │ 2025-01-15 10:30:00 │ Leo núi thành công       │
│ pg-4        │ slot-20250115   │ tl-4         │ FALSE       │ NULL                │ NULL                     │
│ pg-5        │ slot-20250115   │ tl-5         │ FALSE       │ NULL                │ NULL                     │
│ pg-6        │ slot-20250115   │ tl-6         │ FALSE       │ NULL                │ NULL                     │
│ pg-7        │ slot-20250115   │ tl-7         │ FALSE       │ NULL                │ NULL                     │
├─────────────┼─────────────────┼──────────────┼─────────────┼─────────────────────┼──────────────────────────┤
│ pg-8        │ slot-20250118   │ tl-1         │ FALSE       │ NULL                │ NULL                     │
│ pg-9        │ slot-20250118   │ tl-2         │ FALSE       │ NULL                │ NULL                     │
│ pg-10       │ slot-20250118   │ tl-3         │ FALSE       │ NULL                │ NULL                     │
│ pg-11       │ slot-20250118   │ tl-4         │ FALSE       │ NULL                │ NULL                     │
│ pg-12       │ slot-20250118   │ tl-5         │ FALSE       │ NULL                │ NULL                     │
│ pg-13       │ slot-20250118   │ tl-6         │ FALSE       │ NULL                │ NULL                     │
│ pg-14       │ slot-20250118   │ tl-7         │ FALSE       │ NULL                │ NULL                     │
├─────────────┼─────────────────┼──────────────┼─────────────┼─────────────────────┼──────────────────────────┤
│ pg-15       │ slot-20250122   │ tl-1         │ TRUE        │ 2025-01-22 07:28:00 │ Khách đến sớm            │
│ pg-16       │ slot-20250122   │ tl-2         │ TRUE        │ 2025-01-22 07:55:00 │ Xuất phát sớm            │
│ pg-17       │ slot-20250122   │ tl-3         │ TRUE        │ 2025-01-22 09:45:00 │ Thời tiết lý tưởng       │
│ pg-18       │ slot-20250122   │ tl-4         │ TRUE        │ 2025-01-22 11:20:00 │ Cửa hàng phục vụ tốt    │
│ pg-19       │ slot-20250122   │ tl-5         │ TRUE        │ 2025-01-22 12:25:00 │ Món ăn ngon              │
│ pg-20       │ slot-20250122   │ tl-6         │ TRUE        │ 2025-01-22 14:30:00 │ Tham quan trang trọng    │
│ pg-21       │ slot-20250122   │ tl-7         │ TRUE        │ 2025-01-22 15:50:00 │ Hoàn thành thành công    │
└─────────────┴─────────────────┴──────────────┴─────────────┴─────────────────────┴──────────────────────────┘
```

### **6.2 API Response Examples**

#### **GET /tour-slot/slot-20250115/timeline (Slot đang diễn ra - 3/7 completed)**
```json
{
  "statusCode": 200,
  "message": "Lấy timeline với progress thành công",
  "data": {
    "timeline": [
      {
        "id": "tl-1",
        "tourSlotId": "slot-20250115",
        "progressId": "pg-1",
        "activity": "Tập trung tại điểm đón",
        "checkInTime": "07:30:00",
        "sortOrder": 1,
        "isCompleted": true,
        "completedAt": "2025-01-15T07:35:00Z",
        "completionNotes": "Khách đến đầy đủ",
        "canComplete": false,
        "isNext": false,
        "position": 1,
        "totalItems": 7,
        "statusText": "Completed at 2025-01-15 07:35"
      },
      {
        "id": "tl-4",
        "tourSlotId": "slot-20250115",
        "progressId": "pg-4",
        "activity": "Thăm cửa hàng đặc sản",
        "checkInTime": "11:30:00",
        "sortOrder": 4,
        "isCompleted": false,
        "completedAt": null,
        "completionNotes": null,
        "canComplete": true,
        "isNext": true,
        "position": 4,
        "totalItems": 7,
        "statusText": "Pending"
      }
    ],
    "summary": {
      "tourSlotId": "slot-20250115",
      "totalItems": 7,
      "completedItems": 3,
      "progressPercentage": 43,
      "isFullyCompleted": false,
      "statusText": "3/7 hoàn thành"
    }
  }
}
```

#### **GET /tour-slot/slot-20250118/timeline (Slot chưa bắt đầu - 0/7 completed)**
```json
{
  "data": {
    "summary": {
      "tourSlotId": "slot-20250118",
      "totalItems": 7,
      "completedItems": 0,
      "progressPercentage": 0,
      "isFullyCompleted": false,
      "statusText": "0/7 hoàn thành"
    }
  }
}
```

#### **GET /tour-slot/slot-20250122/timeline (Slot đã hoàn thành - 7/7 completed)**
```json
{
  "data": {
    "summary": {
      "tourSlotId": "slot-20250122",
      "totalItems": 7,
      "completedItems": 7,
      "progressPercentage": 100,
      "isFullyCompleted": true,
      "statusText": "Hoàn thành"
    }
  }
}
```

### **6.3 Mobile App UI Examples**

#### **Timeline Progress Screen cho slot-20250115:**
```
┌─────────────────────────────────────────────────────────┐
│ 🏔️ Khám Phá Tây Ninh - 15/01/2025                      │
│ ████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ │
│ 3/7 hoàn thành (43%)                                   │
├─────────────────────────────────────────────────────────┤
│ ✅ 07:35 Tập trung tại điểm đón                        │
│      "Khách đến đầy đủ"                                │
│ ✅ 08:15 Di chuyển đến Núi Bà Đen                      │
│      "Di chuyển đúng giờ"                              │
│ ✅ 10:30 Leo núi và tham quan                          │
│      "Leo núi thành công"                              │
│ ▶️  11:30 Thăm cửa hàng đặc sản        [HOÀN THÀNH]    │
│ ⏳ 12:30 Ăn trưa tại nhà hàng                          │
│ ⏳ 14:00 Thăm Tòa Thánh Tây Ninh                       │
│ ⏳ 16:00 Về điểm đón ban đầu                           │
└─────────────────────────────────────────────────────────┘
```

## 🔧 **PHẦN 7: IMPLEMENTATION DETAILS**

### **7.1 Auto-Create Progress Records**
```sql
-- Trigger khi TourSlot được assign TourDetails
DELIMITER //
CREATE TRIGGER TR_TourSlot_CreateTimelineProgress
    AFTER UPDATE ON TourSlot
    FOR EACH ROW
BEGIN
    IF OLD.TourDetailsId IS NULL AND NEW.TourDetailsId IS NOT NULL THEN
        -- Tự động tạo progress records
        INSERT INTO TourSlotTimelineProgress (
            Id, TourSlotId, TimelineItemId, IsCompleted,
            CreatedAt, CreatedById, IsActive
        )
        SELECT
            UUID(),
            NEW.Id,
            ti.Id,
            FALSE,
            NOW(),
            NEW.UpdatedById,
            TRUE
        FROM TimelineItem ti
        WHERE ti.TourDetailsId = NEW.TourDetailsId
          AND ti.IsActive = TRUE;
    END IF;
END //
DELIMITER ;
```

### **7.2 Sequential Completion Validation**
```csharp
private async Task<bool> ValidateSequentialCompletion(Guid tourSlotId, Guid timelineItemId)
{
    var timelineItem = await _context.TimelineItems
        .FirstOrDefaultAsync(ti => ti.Id == timelineItemId);

    if (timelineItem == null) return false;

    // Đếm số timeline items trước đó chưa complete
    var incompleteEarlierCount = await _context.TourSlotTimelineProgress
        .Include(tp => tp.TimelineItem)
        .Where(tp => tp.TourSlotId == tourSlotId &&
                    tp.TimelineItem.TourDetailsId == timelineItem.TourDetailsId &&
                    tp.TimelineItem.SortOrder < timelineItem.SortOrder &&
                    !tp.IsCompleted &&
                    tp.IsActive)
        .CountAsync();

    return incompleteEarlierCount == 0;
}
```

### **7.3 Progress Analytics**
```sql
-- Query để tính toán statistics
SELECT
    tp.TourSlotId,
    COUNT(*) as TotalItems,
    COUNT(CASE WHEN tp.IsCompleted THEN 1 END) as CompletedItems,
    ROUND(COUNT(CASE WHEN tp.IsCompleted THEN 1 END) * 100.0 / COUNT(*), 2) as ProgressPercentage,
    AVG(CASE
        WHEN tp.IsCompleted AND tp.CompletedAt IS NOT NULL
        THEN TIMESTAMPDIFF(MINUTE, tp.CreatedAt, tp.CompletedAt)
        ELSE NULL
    END) as AvgCompletionTimeMinutes,
    MIN(tp.CompletedAt) as FirstCompletedAt,
    MAX(tp.CompletedAt) as LastCompletedAt
FROM TourSlotTimelineProgress tp
INNER JOIN TimelineItem ti ON tp.TimelineItemId = ti.Id
WHERE tp.TourSlotId = 'slot-20250115'
  AND tp.IsActive = TRUE
GROUP BY tp.TourSlotId;
```

## 🚀 **KẾT LUẬN**

Kiến trúc **Timeline Template + Progress Tracking** giải quyết hoàn toàn vấn đề shared progress giữa các TourSlots bằng cách:

1. **Tách biệt Template và Progress**: Template định nghĩa "cái gì", Progress track "đã làm gì"
2. **Không Duplicate Data**: Template chỉ có 1 bộ, Progress tạo riêng per slot
3. **Independent Tracking**: Mỗi slot có progress hoàn toàn độc lập
4. **Rich Analytics**: Đầy đủ thông tin cho reporting và monitoring
5. **Backward Compatible**: Không breaking changes cho existing systems

**Kết quả**: Mỗi TourSlot có timeline progress riêng biệt, HDV có thể làm việc độc lập, khách hàng thấy tiến độ chính xác cho tour của mình! 🎉
