# KỊCH HOẠCH SỬA ĐỔI THIẾT KẾ TOURTEMPLATE SYSTEM

## 📋 **PHÂN CHIA VAI TRÒ RÕ RÀNG**

### **📋 TourDetails (Timeline + Shop Management):**
- **Timeline**: Lịch trình di chuyển (5h, 7h, 8h, 9h, 10h)
- **Shop Selection**: Chọn shop cho từng điểm trong lịch trình
- **Description**: Mô tả về lịch trình này

### **👨‍🏫 TourOperation (Guide + Business Info):**
- **GuideId**: Hướng dẫn viên
- **Price**: Giá tour
- **MaxGuests**: Số ghế/capacity
- **Description**: Mô tả về operation
- **Relationship**: 1:1 với TourDetails (KHÔNG phải TourSlot)

## 🎯 **RELATIONSHIP ĐÚNG:**

```
TourTemplate (1:N) → TourSlot (auto-generated)
TourTemplate (1:N) → TourDetails (lịch trình templates)
TourDetails (1:1) → TourOperation (guide + giá + ghế)
TourDetails (1:N) → TourSlot (auto-assign via TourDetailsId)
```

### **🔄 WORKFLOW LOGIC:**
```
TourTemplate "Tour Núi Bà Đen"
├── Auto-generate TourSlots: 15/6, 22/6, 29/6...
├── TourDetails "Lịch trình VIP":
│   ├── TimelineItem: 5h khởi hành, 7h ăn sáng (shop A), 9h shop bánh tráng (shop B)...
│   ├── TourOperation: Guide A + 500k + 25 ghế
│   └── Auto-assign cho TẤT CẢ TourSlots: 15/6, 22/6, 29/6 (TourDetailsId = VIP)
└── TourDetails "Lịch trình thường":
    ├── TimelineItem: 6h khởi hành, 8h ăn sáng (shop C), 10h shop bánh tráng (shop D)...
    ├── TourOperation: Guide B + 300k + 35 ghế
    └── Tạo template mới hoặc override slots khác
```

### **🎯 LOGIC CHÍNH - ĐÃ TRIỂN KHAI CLONE LOGIC:**
- **1 TourTemplate** → **nhiều TourDetails** (VIP, thường, tiết kiệm)
- **1 TourDetails** → **1 TourOperation** (guide + giá + ghế riêng)
- **1 TourDetails** → **nhiều TimelineItem** (lịch trình chi tiết + shop selection)
- **CLONE Logic**: Khi tạo TourDetails → tự động CLONE tất cả template slots (TourDetailsId = null) thành detail slots (TourDetailsId = X)
- **Reusability**: Template slots luôn giữ nguyên (TourDetailsId = null) để có thể clone vô hạn lần

## 🔄 **WORKFLOW:**

1. **Tạo TourTemplate** → Auto-generate TourSlots (TourDetailsId = null)
2. **Tạo TourDetails** cho template → AUTO-CLONE template slots thành detail slots (TourDetailsId = X)
3. **TourOperation** tự động tạo cho TourDetails
4. **TimelineItem** quản lý lịch trình chi tiết + shop selection

## 🎯 **ENTITIES DESIGN**

### **1. TourTemplate (Không đổi)**
```csharp
public class TourTemplate
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DayOfWeek ScheduleDays { get; set; }
    public TemplateType TemplateType { get; set; }
    public string Images { get; set; }
    public int Month { get; set; }

    // Navigation
    public ICollection<TourSlot> TourSlots { get; set; }
    public ICollection<TourDetails> TourDetails { get; set; }
}
```

### **2. TourSlot (ĐÃ CÓ TourDetailsId) - ĐÃ TRIỂN KHAI**
```csharp
public class TourSlot
{
    public Guid Id { get; set; }
    public Guid TourTemplateId { get; set; }
    public DateTime SlotDate { get; set; }
    public int Status { get; set; }

    // CLONE Logic: null = template slot, có giá trị = detail slot
    public Guid? TourDetailsId { get; set; }

    // Navigation
    public TourTemplate TourTemplate { get; set; }
    public TourDetails? TourDetails { get; set; }
}
```

### **3. TourDetails (Bỏ Guide/Price/Capacity)**
```csharp
public class TourDetails
{
    public Guid Id { get; set; }
    public Guid TourTemplateId { get; set; }

    // Chỉ quản lý lịch trình
    public string Title { get; set; }         // "Lịch trình VIP"
    public string Description { get; set; }   // Mô tả lịch trình

    // Navigation
    public TourTemplate TourTemplate { get; set; }
    public TourOperation TourOperation { get; set; }  // 1:1
    public ICollection<TimelineItem> Timeline { get; set; }
    public ICollection<TourSlot> AssignedSlots { get; set; }
}
```

### **4. TourOperation (Sửa relationship)**
```csharp
public class TourOperation
{
    public Guid Id { get; set; }
    // OLD: public Guid TourSlotId { get; set; }
    public Guid TourDetailsId { get; set; }  // NEW: 1:1 với TourDetails

    public Guid? GuideId { get; set; }
    public decimal Price { get; set; }
    public int MaxGuests { get; set; }
    public string Description { get; set; }

    // Navigation
    public TourDetails TourDetails { get; set; }
    public User? Guide { get; set; }
}
```

### **5. TimelineItem (Entity mới)**
```csharp
public class TimelineItem
{
    public Guid Id { get; set; }
    public Guid TourDetailsId { get; set; }

    public TimeSpan CheckInTime { get; set; }  // 5h, 7h, 8h, 9h, 10h
    public string Activity { get; set; }       // Khởi hành, Ăn sáng...
    public Guid? ShopId { get; set; }          // Shop cho activity này
    public int SortOrder { get; set; }

    // Navigation
    public TourDetails TourDetails { get; set; }
    public Shop? Shop { get; set; }
}
```

## 📝 **VÍ DỤ THỰC TẾ**

```
Template: "Tour Núi Bà Đen"
├── Auto-generate TourSlots: 15/6, 22/6, 29/6...
├── TourDetails "Lịch trình VIP":
│   ├── Timeline: 5h khởi hành, 7h ăn sáng (shop A), 9h shop bánh tráng (shop B)
│   ├── TourOperation: Guide A + 500k + 25 ghế
│   └── Auto-assign cho TẤT CẢ slots: 15/6, 22/6, 29/6
└── TourDetails "Lịch trình thường":
    ├── Timeline: 6h khởi hành, 8h ăn sáng (shop C), 10h shop bánh tráng (shop D)
    ├── TourOperation: Guide B + 300k + 35 ghế
    └── Tạo template mới hoặc override slots
```

## � **NHỮNG GÌ CẦN SỬA**

### **Database:**
- **TourOperation**: Đổi TourSlotId → TourDetailsId (1:1 với TourDetails)
- **TourSlot**: Thêm TourDetailsId (auto-assign khi tạo TourDetails)
- **TimelineItem**: Entity mới cho timeline + shop selection
- **TourDetails**: Bỏ Guide/Price/Capacity fields (chuyển sang TourOperation)

### **Business Logic - ĐÃ TRIỂN KHAI CLONE LOGIC:**
- **TourOperationService**: Đổi relationship từ TourSlot → TourDetails
- **TourDetailsService**: ĐÃ CÓ clone logic trong CreateTourDetailAsync()
- **Clone Logic**: Khi tạo TourDetails → AUTO-CLONE template slots (TourDetailsId = null) thành detail slots (TourDetailsId = X)
- **Template Reusability**: Template slots luôn được bảo toàn để có thể tái sử dụng

### **API:**
- **TourOperationController**: Sửa endpoints từ slot-based → details-based
- **TourDetailsController**: Timeline + shop endpoints (bỏ guide/price endpoints)

---

## 🎯 **WORKFLOW MỚI THEO HÌNH ẢNH**

### **TourDetails quản lý TẤT CẢ như trong hình:**

#### **1. Timeline Management (Thời gian check-in nhà xe + Lịch trình):**
- **5h**: Khởi hành
- **7h**: Ăn sáng → **Chọn shop**
- **8h**: Ghé tram dừng → **Chọn shop**
- **9h**: Ghé shop bánh tráng → **Chọn shop**
- **10h**: Tới Núi Bà

#### **2. Description (Mô tả lịch trình):**
- Mô tả về lịch trình di chuyển này

### **TourOperation quản lý (đã có sẵn):**

#### **1. Guide Management (Hướng dẫn viên):**
- Assign guide cho slot này

#### **2. Price & Capacity Management:**
- Giá tour và số ghế cho slot này

#### **3. Operation Description:**
- Mô tả về operation này

#### **3. Ví dụ cụ thể:**
```json
// Tour Slot ngày 15/6/2025
[
  {
    "id": "item1",
    "tourSlotId": "slot-15-6",
    "timeSlot": "05:00:00",
    "location": "Nhà xe",
    "description": "Khởi hành",
    "shopId": null,           // Không có shop
    "sortOrder": 1
  },
  {
    "id": "item2",
    "tourSlotId": "slot-15-6",
    "timeSlot": "07:00:00",
    "location": "Quán ăn sáng",
    "description": "Ăn sáng",
    "shopId": "shop-A",       // Chọn shop A
    "sortOrder": 2
  },
  {
    "id": "item3",
    "tourSlotId": "slot-15-6",
    "timeSlot": "09:00:00",
    "location": "Shop bánh tráng",
    "description": "Ghé shop bánh tráng",
    "shopId": "shop-B",       // Chọn shop B
    "sortOrder": 3
  }
]

// Tour Slot ngày 22/6/2025 - Timeline khác
[
  {
    "id": "item4",
    "tourSlotId": "slot-22-6",
    "timeSlot": "05:00:00",
    "location": "Nhà xe",
    "description": "Khởi hành",
    "shopId": null,           // Không có shop
    "sortOrder": 1
  },
  {
    "id": "item5",
    "tourSlotId": "slot-22-6",
    "timeSlot": "07:30:00",   // Thời gian khác
    "location": "Quán ăn sáng",
    "description": "Ăn sáng",
    "shopId": "shop-C",       // Chọn shop C (khác với slot 15/6)
    "sortOrder": 2
  }
]
```

#### **4. User Experience:**
1. **Chọn TourSlot**: "Tour Núi Bà Đen ngày 15/6/2025"
2. **Quản lý Timeline**: Thêm/sửa/xóa các mốc thời gian
3. **Chọn Shop**: Cho từng timeline item, click "Chọn shop" → dropdown shops
4. **Flexibility**: Mỗi tour có thể có timeline và shop selection hoàn toàn khác nhau

---

## 🔧 **IMPLEMENTATION PLAN**

## **PHASE 1: DATABASE MIGRATION** ⚡ (Priority: HIGH) ✅ **COMPLETED**

### **Step 1.1: Backup hiện tại** ✅
```sql
-- Backup tables trước khi migration
CREATE TABLE TourOperation_Backup AS SELECT * FROM TourOperation;
CREATE TABLE TourSlot_Backup AS SELECT * FROM TourSlot;
CREATE TABLE TourDetails_Backup AS SELECT * FROM TourDetails;
```

### **Step 1.2: Tạo migration mới** ✅
```bash
dotnet ef migrations add TourTemplateRedesign_FixRelationships --project TayNinhTourApi.DataAccessLayer --startup-project TayNinhTourApi.Controller
```

### **Step 1.3: Migration script**
```sql
-- 1. Thêm TourDetailsId vào TourSlot
ALTER TABLE TourSlot ADD COLUMN TourDetailsId CHAR(36) NULL;

-- 2. Thêm TourDetailsId vào TourOperation, bỏ TourSlotId
ALTER TABLE TourOperation ADD COLUMN TourDetailsId CHAR(36) NULL;

-- 3. Tạo TimelineItem table
CREATE TABLE TimelineItem (
    Id CHAR(36) PRIMARY KEY,
    TourDetailsId CHAR(36) NOT NULL,
    CheckInTime TIME NOT NULL,
    Activity VARCHAR(255) NOT NULL,
    ShopId CHAR(36) NULL,
    SortOrder INT NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NULL,
    FOREIGN KEY (TourDetailsId) REFERENCES TourDetails(Id) ON DELETE CASCADE,
    FOREIGN KEY (ShopId) REFERENCES Shop(Id) ON DELETE SET NULL
);

-- 4. Migrate TourOperation data (tạm thời để null, sẽ update sau khi có TourDetails)
-- ALTER TABLE TourOperation DROP FOREIGN KEY FK_TourOperation_TourSlot;
-- ALTER TABLE TourOperation DROP COLUMN TourSlotId;

-- 5. Add foreign keys
ALTER TABLE TourSlot ADD CONSTRAINT FK_TourSlot_TourDetails
FOREIGN KEY (TourDetailsId) REFERENCES TourDetails(Id) ON DELETE SET NULL;

ALTER TABLE TourOperation ADD CONSTRAINT FK_TourOperation_TourDetails
FOREIGN KEY (TourDetailsId) REFERENCES TourDetails(Id) ON DELETE CASCADE;
```

---

## **PHASE 2: ENTITY MODELS** 🏗️ (Priority: HIGH) ✅ **COMPLETED**

### **Step 2.1: Update TourDetails Entity**
```csharp
// TayNinhTourApi.DataAccessLayer/Entities/TourDetails.cs
public class TourDetails
{
    public Guid Id { get; set; }

    // OLD: public Guid TourTemplateId { get; set; }
    public Guid TourSlotId { get; set; }  // NEW: Timeline thuộc về slot

    // Timeline properties
    public TimeSpan TimeSlot { get; set; }        // Thời gian (5h, 7h, 9h...)
    public string Location { get; set; }          // Địa điểm (Nhà xe, Shop...)
    public string Description { get; set; }       // Mô tả (Khởi hành, Ăn sáng...)
    public int SortOrder { get; set; }            // Thứ tự sắp xếp

    // Shop selection properties
    public Guid? ShopId { get; set; }             // Shop được chọn cho timeline item này

    // Audit properties
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    // OLD: public TourTemplate TourTemplate { get; set; }
    public TourSlot TourSlot { get; set; }        // NEW: Thuộc về slot cụ thể
    public Shop? Shop { get; set; }               // Shop được chọn (nullable)
}
```

**Ý nghĩa:**
- **TourSlotId**: Timeline thuộc về tour cụ thể vào ngày cụ thể
- **ShopId**: Mỗi timeline item có thể chọn shop riêng (hoặc không chọn)
- **Flexibility**: Mỗi slot có timeline và shop selection độc lập

### **Step 2.2: Update TourSlot Entity**
```csharp
// TayNinhTourApi.DataAccessLayer/Entities/TourSlot.cs
public class TourSlot
{
    public Guid Id { get; set; }
    public Guid TourTemplateId { get; set; }
    public DateTime Date { get; set; }
    // ... other properties
    
    // Navigation properties
    public TourTemplate TourTemplate { get; set; }
    public ICollection<TourDetails> TourDetails { get; set; } = new List<TourDetails>();  // NEW
    public ICollection<TourOperation> TourOperations { get; set; } = new List<TourOperation>();
}
```

### **Step 2.3: Update TourTemplate Entity**
```csharp
// TayNinhTourApi.DataAccessLayer/Entities/TourTemplate.cs
public class TourTemplate
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    // ... other template properties
    
    // Navigation properties
    public ICollection<TourSlot> TourSlots { get; set; } = new List<TourSlot>();
    // REMOVE: public ICollection<TourDetails> TourDetails { get; set; }
}
```

---

## **PHASE 3: DATABASE CONFIGURATION** ⚙️ (Priority: HIGH) ✅ **COMPLETED**

### **Step 3.1: Update TourDetailsConfiguration**
```csharp
// TayNinhTourApi.DataAccessLayer/Configurations/TourDetailsConfiguration.cs
public void Configure(EntityTypeBuilder<TourDetails> builder)
{
    // OLD relationship
    // builder.HasOne(td => td.TourTemplate)
    //        .WithMany(tt => tt.TourDetails)
    //        .HasForeignKey(td => td.TourTemplateId);

    // NEW relationship
    builder.HasOne(td => td.TourSlot)
           .WithMany(ts => ts.TourDetails)
           .HasForeignKey(td => td.TourSlotId)
           .OnDelete(DeleteBehavior.Cascade);

    builder.HasOne(td => td.Shop)
           .WithMany()
           .HasForeignKey(td => td.ShopId)
           .OnDelete(DeleteBehavior.SetNull);
}
```

---

## **PHASE 4: REPOSITORY UPDATES** 📚 (Priority: MEDIUM)

### **Step 4.1: Update ITourDetailsRepository**
```csharp
// TayNinhTourApi.DataAccessLayer/Repositories/Interfaces/ITourDetailsRepository.cs
public interface ITourDetailsRepository : IGenericRepository<TourDetails>
{
    // OLD: Task<IEnumerable<TourDetails>> GetTimelineByTemplateIdAsync(Guid templateId);
    Task<IEnumerable<TourDetails>> GetTimelineBySlotIdAsync(Guid slotId);  // NEW
    
    Task<TourDetails?> GetBySlotAndTimeAsync(Guid slotId, TimeSpan timeSlot);
    Task<IEnumerable<TourDetails>> GetBySlotIdOrderedAsync(Guid slotId);
}
```

### **Step 4.2: Update TourDetailsRepository Implementation**
```csharp
// TayNinhTourApi.DataAccessLayer/Repositories/TourDetailsRepository.cs
public async Task<IEnumerable<TourDetails>> GetTimelineBySlotIdAsync(Guid slotId)
{
    return await _context.TourDetails
        .Where(td => td.TourSlotId == slotId)
        .Include(td => td.Shop)
        .OrderBy(td => td.SortOrder)
        .ThenBy(td => td.TimeSlot)
        .ToListAsync();
}

public async Task<IEnumerable<TourDetails>> GetBySlotIdOrderedAsync(Guid slotId)
{
    return await _context.TourDetails
        .Where(td => td.TourSlotId == slotId)
        .Include(td => td.Shop)
        .Include(td => td.TourSlot)
            .ThenInclude(ts => ts.TourTemplate)
        .OrderBy(td => td.SortOrder)
        .ThenBy(td => td.TimeSlot)
        .ToListAsync();
}
```

---

## **PHASE 5: DTO UPDATES** 📝 (Priority: MEDIUM)

### **Step 5.1: Update Request DTOs**
```csharp
// TayNinhTourApi.BusinessLogicLayer/DTOs/Request/TourCompany/RequestCreateTourDetailDto.cs
public class RequestCreateTourDetailDto
{
    // OLD: public Guid TourTemplateId { get; set; }
    public Guid TourSlotId { get; set; }  // NEW
    
    public string TimeSlot { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public Guid? ShopId { get; set; }
}
```

### **Step 5.2: Update Response DTOs**
```csharp
// TayNinhTourApi.BusinessLogicLayer/DTOs/Response/TourCompany/ResponseTourDetailDto.cs
public class ResponseTourDetailDto
{
    public Guid Id { get; set; }
    // OLD: public Guid TourTemplateId { get; set; }
    public Guid TourSlotId { get; set; }  // NEW
    
    public string TimeSlot { get; set; }
    public string Location { get; set; }
    public string Description { get; set; }
    public int SortOrder { get; set; }
    
    public ResponseShopDto? Shop { get; set; }
    public ResponseTourSlotDto? TourSlot { get; set; }  // NEW: include slot info
}
```

---

## **PHASE 6: SERVICE LAYER UPDATES** 🔧 (Priority: MEDIUM) ✅ **CLONE LOGIC ĐÃ TRIỂN KHAI**

### **🆕 Clone Logic đã implement trong `TourDetailsService.CreateTourDetailAsync()`:**

```csharp
// AUTO-CLONE LOGIC khi tạo TourDetails mới
// 1. Lấy tất cả template slots (TourDetailsId = null)
var templateSlots = await _unitOfWork.TourSlotRepository
    .GetByTemplateIdAsync(request.TourTemplateId);
var unassignedSlots = templateSlots.Where(slot => slot.TourDetailsId == null).ToList();

// 2. Tạo cloned slots cho TourDetails mới
foreach (var templateSlot in unassignedSlots)
{
    var clonedSlot = new TourSlot
    {
        Id = Guid.NewGuid(),
        TourTemplateId = templateSlot.TourTemplateId,
        SlotDate = templateSlot.SlotDate,
        Status = templateSlot.Status,
        TourDetailsId = tourDetail.Id, // LINK VỚI TOURDETAILS MỚI
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    await _unitOfWork.TourSlotRepository.AddAsync(clonedSlot);
}
```

**✅ Kết quả:**
- Template slots (TourDetailsId = null) được BẢO TOÀN 
- Detail slots (TourDetailsId = X) được TẠO MỚI
- Template có thể TÁI SỬ DỤNG vô hạn lần
- Mỗi TourDetails có bộ slots RIÊNG BIỆT

---

## **PHASE 6: SERVICE LAYER UPDATES** 🔧 (Priority: MEDIUM)

### **Step 6.1: Update ITourDetailsService**
```csharp
// TayNinhTourApi.BusinessLogicLayer/Services/Interfaces/ITourDetailsService.cs
public interface ITourDetailsService
{
    // OLD: Task<BaseResponse> GetTimelineByTemplateIdAsync(Guid templateId);
    Task<BaseResponse> GetTimelineBySlotIdAsync(Guid slotId);  // NEW
    
    Task<BaseResponse> CreateTimelineItemAsync(RequestCreateTourDetailDto request);
    Task<BaseResponse> UpdateTimelineItemAsync(Guid id, RequestUpdateTourDetailDto request);
    Task<BaseResponse> DeleteTimelineItemAsync(Guid id);
    Task<BaseResponse> ReorderTimelineAsync(RequestReorderTimelineDto request);
}
```

### **Step 6.2: Update TourDetailsService Implementation**
```csharp
// TayNinhTourApi.BusinessLogicLayer/Services/TourDetailsService.cs
public async Task<BaseResponse> GetTimelineBySlotIdAsync(Guid slotId)
{
    try
    {
        // Validate slot exists
        var slot = await _unitOfWork.TourSlotRepository.GetByIdAsync(slotId);
        if (slot == null)
        {
            return new BaseResponse
            {
                Success = false,
                Message = "TourSlot không tồn tại",
                StatusCode = 404
            };
        }

        var timeline = await _tourDetailsRepository.GetTimelineBySlotIdAsync(slotId);
        var timelineDto = _mapper.Map<IEnumerable<ResponseTourDetailDto>>(timeline);

        return new BaseResponse
        {
            Success = true,
            Message = "Lấy timeline thành công",
            Data = timelineDto,
            StatusCode = 200
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting timeline for slot {SlotId}", slotId);
        return new BaseResponse
        {
            Success = false,
            Message = "Có lỗi xảy ra khi lấy timeline",
            StatusCode = 500
        };
    }
}

public async Task<BaseResponse> CreateTimelineItemAsync(RequestCreateTourDetailDto request)
{
    try
    {
        // Validate slot exists
        var slot = await _unitOfWork.TourSlotRepository.GetByIdAsync(request.TourSlotId);
        if (slot == null)
        {
            return new BaseResponse
            {
                Success = false,
                Message = "TourSlot không tồn tại",
                StatusCode = 404
            };
        }

        // Parse time slot
        if (!TimeSpan.TryParse(request.TimeSlot, out var timeSlot))
        {
            return new BaseResponse
            {
                Success = false,
                Message = "Định dạng thời gian không hợp lệ",
                StatusCode = 400
            };
        }

        // Get next sort order
        var existingItems = await _tourDetailsRepository.GetTimelineBySlotIdAsync(request.TourSlotId);
        var nextSortOrder = existingItems.Any() ? existingItems.Max(x => x.SortOrder) + 1 : 1;

        var tourDetail = new TourDetails
        {
            Id = Guid.NewGuid(),
            TourSlotId = request.TourSlotId,
            TimeSlot = timeSlot,
            Location = request.Location,
            Description = request.Description,
            ShopId = request.ShopId,
            SortOrder = nextSortOrder,
            CreatedAt = DateTime.UtcNow
        };

        await _tourDetailsRepository.AddAsync(tourDetail);
        await _unitOfWork.SaveChangesAsync();

        var result = _mapper.Map<ResponseTourDetailDto>(tourDetail);
        return new BaseResponse
        {
            Success = true,
            Message = "Tạo timeline item thành công",
            Data = result,
            StatusCode = 201
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating timeline item");
        return new BaseResponse
        {
            Success = false,
            Message = "Có lỗi xảy ra khi tạo timeline item",
            StatusCode = 500
        };
    }
}
```

---

## **PHASE 7: CONTROLLER UPDATES** 🎮 (Priority: HIGH)

### **Step 7.1: Update TourDetailsController**
```csharp
// TayNinhTourApi.Controller/Controllers/TourDetailsController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Tour Company")]
public class TourDetailsController : ControllerBase
{
    private readonly ITourDetailsService _tourDetailsService;

    public TourDetailsController(ITourDetailsService tourDetailsService)
    {
        _tourDetailsService = tourDetailsService;
    }

    /// <summary>
    /// Lấy timeline của một TourSlot cụ thể
    /// </summary>
    /// <param name="slotId">ID của TourSlot</param>
    [HttpGet("timeline/{slotId}")]
    public async Task<IActionResult> GetTimelineBySlotId(Guid slotId)
    {
        var result = await _tourDetailsService.GetTimelineBySlotIdAsync(slotId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Tạo timeline item mới cho TourSlot
    /// </summary>
    [HttpPost("timeline")]
    public async Task<IActionResult> CreateTimelineItem([FromBody] RequestCreateTourDetailDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _tourDetailsService.CreateTimelineItemAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Cập nhật timeline item
    /// </summary>
    [HttpPatch("timeline/{id}")]
    public async Task<IActionResult> UpdateTimelineItem(Guid id, [FromBody] RequestUpdateTourDetailDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _tourDetailsService.UpdateTimelineItemAsync(id, request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Xóa timeline item
    /// </summary>
    [HttpDelete("timeline/{id}")]
    public async Task<IActionResult> DeleteTimelineItem(Guid id)
    {
        var result = await _tourDetailsService.DeleteTimelineItemAsync(id);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Sắp xếp lại thứ tự timeline
    /// </summary>
    [HttpPost("timeline/reorder")]
    public async Task<IActionResult> ReorderTimeline([FromBody] RequestReorderTimelineDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _tourDetailsService.ReorderTimelineAsync(request);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Lấy danh sách shops để chọn cho timeline item
    /// </summary>
    [HttpGet("shops")]
    public async Task<IActionResult> GetShopsForTimeline([FromQuery] string? location = null, [FromQuery] string? search = null)
    {
        // Lấy active shops để chọn cho timeline
        // Filter theo location và search term
        var result = await _shopService.GetActiveShopsAsync(location, search);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Cập nhật shop cho timeline item cụ thể
    /// </summary>
    [HttpPatch("timeline/{id}/shop")]
    public async Task<IActionResult> UpdateTimelineShop(Guid id, [FromBody] RequestUpdateTimelineShopDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _tourDetailsService.UpdateTimelineShopAsync(id, request.ShopId);
        return StatusCode(result.StatusCode, result);
    }

    /// <summary>
    /// Xóa shop khỏi timeline item
    /// </summary>
    [HttpDelete("timeline/{id}/shop")]
    public async Task<IActionResult> RemoveTimelineShop(Guid id)
    {
        var result = await _tourDetailsService.UpdateTimelineShopAsync(id, null);
        return StatusCode(result.StatusCode, result);
    }
}
```

---

## **PHASE 8: AUTOMAPPER UPDATES** 🗺️ (Priority: MEDIUM)

### **Step 8.1: Update TourDetailsMappingProfile**
```csharp
// TayNinhTourApi.BusinessLogicLayer/Mapping/TourDetailsMappingProfile.cs
public class TourDetailsMappingProfile : Profile
{
    public TourDetailsMappingProfile()
    {
        // Entity to Response DTO
        CreateMap<TourDetails, ResponseTourDetailDto>()
            .ForMember(dest => dest.TimeSlot, opt => opt.MapFrom(src => src.TimeSlot.ToString(@"hh\:mm")))
            .ForMember(dest => dest.Shop, opt => opt.MapFrom(src => src.Shop))
            .ForMember(dest => dest.TourSlot, opt => opt.MapFrom(src => src.TourSlot));

        // Request DTO to Entity
        CreateMap<RequestCreateTourDetailDto, TourDetails>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.TimeSlot, opt => opt.MapFrom(src => TimeSpan.Parse(src.TimeSlot)))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.TourSlot, opt => opt.Ignore())
            .ForMember(dest => dest.Shop, opt => opt.Ignore());

        CreateMap<RequestUpdateTourDetailDto, TourDetails>()
            .ForMember(dest => dest.TimeSlot, opt => opt.MapFrom(src => TimeSpan.Parse(src.TimeSlot)))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.TourSlotId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TourSlot, opt => opt.Ignore())
            .ForMember(dest => dest.Shop, opt => opt.Ignore());
    }
}
```

---

## **PHASE 9: TESTING & VALIDATION** 🧪 (Priority: HIGH)

### **Step 9.1: Unit Tests Update**
```csharp
// Tests/TourDetailsServiceTests.cs
[Test]
public async Task GetTimelineBySlotId_ValidSlotId_ReturnsTimeline()
{
    // Arrange
    var slotId = Guid.NewGuid();
    var mockTimeline = new List<TourDetails>
    {
        new TourDetails { Id = Guid.NewGuid(), TourSlotId = slotId, TimeSlot = TimeSpan.FromHours(5) }
    };

    _mockTourDetailsRepository.Setup(x => x.GetTimelineBySlotIdAsync(slotId))
        .ReturnsAsync(mockTimeline);
    _mockUnitOfWork.Setup(x => x.TourSlotRepository.GetByIdAsync(slotId))
        .ReturnsAsync(new TourSlot { Id = slotId });

    // Act
    var result = await _tourDetailsService.GetTimelineBySlotIdAsync(slotId);

    // Assert
    Assert.IsTrue(result.Success);
    Assert.AreEqual(200, result.StatusCode);
}

[Test]
public async Task GetTimelineBySlotId_InvalidSlotId_ReturnsNotFound()
{
    // Arrange
    var slotId = Guid.NewGuid();
    _mockUnitOfWork.Setup(x => x.TourSlotRepository.GetByIdAsync(slotId))
        .ReturnsAsync((TourSlot)null);

    // Act
    var result = await _tourDetailsService.GetTimelineBySlotIdAsync(slotId);

    // Assert
    Assert.IsFalse(result.Success);
    Assert.AreEqual(404, result.StatusCode);
    Assert.AreEqual("TourSlot không tồn tại", result.Message);
}
```

### **Step 9.2: Integration Tests**
```csharp
// Tests/TourDetailsControllerIntegrationTests.cs
[Test]
public async Task GetTimelineBySlotId_ValidRequest_ReturnsOk()
{
    // Arrange
    var template = await CreateTestTourTemplate();
    var slot = await CreateTestTourSlot(template.Id);
    await CreateTestTimelineItems(slot.Id);

    // Act
    var response = await _client.GetAsync($"/api/TourDetails/timeline/{slot.Id}");

    // Assert
    Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<BaseResponse>(content);
    Assert.IsTrue(result.Success);
}
```

---

## **PHASE 10: DOCUMENTATION UPDATES** 📚 (Priority: LOW)

### **Step 10.1: API Documentation**
- Update OpenAPI specs
- Update endpoint descriptions
- Update example requests/responses
- Update Postman collections

### **Step 10.2: Code Documentation**
- Update XML comments
- Update README files
- Update architecture diagrams
- Update flow documentation

---

## **PHASE 11: DEPLOYMENT PLAN** 🚀 (Priority: MEDIUM)

### **Step 11.1: Pre-deployment Checklist**
- [ ] Database backup completed
- [ ] Migration scripts tested
- [ ] Unit tests passing
- [ ] Integration tests passing
- [ ] Code review completed
- [ ] Documentation updated

### **Step 11.2: Deployment Steps**
1. **Stop application**
2. **Backup database**
3. **Apply migration**
4. **Deploy new code**
5. **Verify functionality**
6. **Monitor for issues**

### **Step 11.3: Rollback Plan**
- Restore database from backup
- Deploy previous version
- Verify system stability

---

## **PHASE 12: POST-DEPLOYMENT** ✅ (Priority: LOW)

### **Step 12.1: Monitoring**
- Monitor API performance
- Check error logs
- Verify data integrity
- User acceptance testing

### **Step 12.2: Cleanup**
- Remove backup tables (after verification)
- Clean up old code comments
- Update team documentation
- Archive old migration files

---


## 🎯 **SUCCESS CRITERIA**

### **Technical:**
- [ ] All migrations applied successfully
- [ ] All tests passing (unit + integration)
- [ ] API endpoints working correctly
- [ ] Performance maintained or improved

### **Business:**
- [ ] Timeline management works per slot (không phải per template)
- [ ] Shop selection works per timeline item
- [ ] Each tour slot can have custom timeline + shop selection
- [ ] Template reusability maintained (chỉ thông tin cơ bản)
- [ ] User workflow improved (template → slot → timeline/shop)

### **Quality:**
- [ ] Code review completed
- [ ] Documentation updated
- [ ] No regression bugs
- [ ] System stability maintained

---

---

## 🆕 **CẬP NHẬT: CLONE LOGIC ĐÃ TRIỂN KHAI**

### **📊 Ví dụ thực tế với Clone Logic:**

**Trước khi tạo TourDetails:**
```sql
-- Template slots (có thể tái sử dụng)
TourSlot: ID=slot1, TourTemplateId=template1, SlotDate=15/6, TourDetailsId=NULL
TourSlot: ID=slot2, TourTemplateId=template1, SlotDate=22/6, TourDetailsId=NULL  
TourSlot: ID=slot3, TourTemplateId=template1, SlotDate=29/6, TourDetailsId=NULL
```

**Sau khi tạo TourDetails "VIP":**
```sql
-- Template slots (vẫn giữ nguyên để tái sử dụng)
TourSlot: ID=slot1, TourTemplateId=template1, SlotDate=15/6, TourDetailsId=NULL
TourSlot: ID=slot2, TourTemplateId=template1, SlotDate=22/6, TourDetailsId=NULL
TourSlot: ID=slot3, TourTemplateId=template1, SlotDate=29/6, TourDetailsId=NULL

-- Detail slots cho VIP (bản sao riêng)  
TourSlot: ID=new1, TourTemplateId=template1, SlotDate=15/6, TourDetailsId=VIP_123
TourSlot: ID=new2, TourTemplateId=template1, SlotDate=22/6, TourDetailsId=VIP_123
TourSlot: ID=new3, TourTemplateId=template1, SlotDate=29/6, TourDetailsId=VIP_123
```

**Sau khi tạo TourDetails "Thường":**
```sql
-- Template slots (vẫn giữ nguyên để tái sử dụng)
TourSlot: ID=slot1, TourTemplateId=template1, SlotDate=15/6, TourDetailsId=NULL
TourSlot: ID=slot2, TourTemplateId=template1, SlotDate=22/6, TourDetailsId=NULL
TourSlot: ID=slot3, TourTemplateId=template1, SlotDate=29/6, TourDetailsId=NULL

-- Detail slots cho VIP (không thay đổi)
TourSlot: ID=new1, TourTemplateId=template1, SlotDate=15/6, TourDetailsId=VIP_123
TourSlot: ID=new2, TourTemplateId=template1, SlotDate=22/6, TourDetailsId=VIP_123
TourSlot: ID=new3, TourTemplateId=template1, SlotDate=29/6, TourDetailsId=VIP_123

-- Detail slots cho Thường (bản sao mới)
TourSlot: ID=new4, TourTemplateId=template1, SlotDate=15/6, TourDetailsId=THUONG_456
TourSlot: ID=new5, TourTemplateId=template1, SlotDate=22/6, TourDetailsId=THUONG_456  
TourSlot: ID=new6, TourTemplateId=template1, SlotDate=29/6, TourDetailsId=THUONG_456
```

### **🚀 Tại sao CLONE logic giải quyết vấn đề reusability:**

1. **Template Reusability**: Template slots luôn có `TourDetailsId = null` → có thể tạo TourDetails mới vô hạn lần
2. **Data Independence**: Mỗi TourDetails có bộ slots riêng → không ảnh hưởng lẫn nhau  
3. **Scalability**: Có thể tạo "VIP", "Thường", "Tiết kiệm", "Premium"... từ cùng 1 template
4. **Data Integrity**: Template gốc không bao giờ bị thay đổi → đảm bảo tính nhất quán

### **📋 Status các PHASE:**
- ✅ **PHASE 1-5**: Database, Entity, Configuration đã có sẵn và hỗ trợ clone logic
- ✅ **PHASE 6**: Service Layer - Clone logic đã được triển khai
- ⏳ **PHASE 7-12**: Các phase còn lại cần tiếp tục theo plan

---

**Ngày tạo**: 07/06/2025  
**Ngày cập nhật**: 10/06/2025 - Thêm clone logic đã triển khai  
**Tác giả**: Phân tích dựa trên yêu cầu sửa đổi thiết kế  
**Trạng thái**: Clone logic đã triển khai (Phase 6) - Tiếp tục các phase 7-12
```
