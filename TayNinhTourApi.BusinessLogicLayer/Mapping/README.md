# AutoMapper Configuration Documentation

## Tổng Quan

Project TayNinhTourBE sử dụng AutoMapper để thực hiện object-to-object mapping giữa entities và DTOs. Mapping được tổ chức thành các profile riêng biệt để dễ quản lý và maintain.

## Cấu Trúc Mapping Profiles

### 1. MappingProfile.cs (Main Profile)
Profile chính chứa mapping cho các entities cơ bản:

#### User Mapping
- `RequestRegisterDto` → `User`
- `User` → `UserCmsDto`

#### Tour Mapping (Legacy)
- `RequestCreateTourCmsDto` → `Tour`
- `Tour` → `TourDto` (với Images collection mapping)

#### Blog Mapping
- `Blog` → `BlogDto` (với BlogImages collection mapping)

#### TourDetails Mapping
- `RequestCreateTourDetailDto` → `TourDetails`
- `RequestUpdateTourDetailDto` → `TourDetails` (partial update)
- `TourDetails` → `TourDetailDto` (với Shop relationship)

#### Shop Mapping
- `RequestCreateShopDto` → `Shop`
- `RequestUpdateShopDto` → `Shop` (partial update)
- `Shop` → `ShopDto`
- `Shop` → `ShopSummaryDto`

#### Timeline Mapping
- `TourTemplate` → `TimelineDto` (với TourDetails ordering)

#### TourSlot Mapping
- `RequestUpdateSlotDto` → `TourSlot`
- `TourSlot` → `TourSlotDto` (với enum conversion)

#### TourOperation Mapping
- `RequestCreateOperationDto` → `TourOperation`
- `RequestUpdateOperationDto` → `TourOperation` (partial update)
- `TourOperation` → `TourOperationDto` (với Guide relationship)
- `TourOperation` → `OperationSummaryDto` (với nested properties)

#### Additional Complex Mappings
- `TimelineOrderItem` → `TourDetails` (cho reorder timeline)

### 2. TourTemplateMappingProfile.cs (Dedicated Profile)
Profile riêng cho TourTemplate entity với mapping chi tiết:

#### Request Mappings
- `RequestCreateTourTemplateDto` → `TourTemplate`
- `RequestUpdateTourTemplateDto` → `TourTemplate` (partial update)

#### Response Mappings
- `TourTemplate` → `TourTemplateDto` (standard response)
- `TourTemplate` → `TourTemplateDetailDto` (detailed response)
- `TourTemplate` → `TourTemplateSummaryDto` (summary for listing)

## Tính Năng Đặc Biệt

### 1. Images Collection Mapping
```csharp
.ForMember(dest => dest.Images, opt => opt.MapFrom(src => 
    src.Images != null ? src.Images.Select(i => i.Url).ToList() : new List<string>()))
```

### 2. Enum to String Conversion
```csharp
.ForMember(dest => dest.TemplateType, opt => opt.MapFrom(src => src.TemplateType.ToString()))
.ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => GetTourSlotStatusName(src.Status)))
```

### 3. Partial Update Mapping
```csharp
.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null))
```

### 4. Timeline Ordering
```csharp
.ForMember(dest => dest.TourDetails, opt => opt.MapFrom(src => 
    src.TourDetails.OrderBy(td => td.SortOrder)))
```

### 5. Nested Relationships
```csharp
.ForMember(dest => dest.Shop, opt => opt.MapFrom(src => src.Shop))
.ForMember(dest => dest.Guide, opt => opt.MapFrom(src => src.Guide))
```

## Helper Methods

### GetTourSlotStatusName()
Chuyển đổi `TourSlotStatus` enum sang tên tiếng Việt:
- Available → "Có sẵn"
- FullyBooked → "Đã đầy"
- Cancelled → "Đã hủy"
- Completed → "Đã hoàn thành"
- InProgress → "Đang thực hiện"

### GetTourOperationStatusName()
Chuyển đổi `TourOperationStatus` enum sang tên tiếng Việt:
- Scheduled → "Đã lên lịch"
- InProgress → "Đang thực hiện"
- Completed → "Đã hoàn thành"
- Cancelled → "Đã hủy"
- Postponed → "Đã hoãn"
- PendingConfirmation → "Chờ xác nhận"

## Performance Testing

### MappingPerformanceTest.cs
Class test performance cho large datasets:

#### Available Tests
- `TestTourTemplateMapping()` - Test TourTemplate mapping performance
- `TestTourDetailsMapping()` - Test TourDetails với Shop relationships
- `TestTourOperationMapping()` - Test TourOperation với nested relationships
- `RunAllTests()` - Chạy tất cả tests

#### Usage Example
```csharp
// Inject IMapper vào service
var mapper = serviceProvider.GetService<IMapper>();

// Run performance tests
MappingPerformanceTest.RunAllTests(mapper, 1000);
```

## Configuration trong Program.cs

```csharp
// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
```

AutoMapper sẽ tự động scan và register tất cả Profile classes trong assembly.

## Best Practices

### 1. Conditional Mapping
Sử dụng cho partial updates:
```csharp
.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null))
```

### 2. Null Safety
Luôn check null cho collections:
```csharp
.ForMember(dest => dest.Images, opt => opt.MapFrom(src => 
    src.Images != null ? src.Images.Select(i => i.Url).ToList() : new List<string>()))
```

### 3. Ignore Properties
Ignore các properties không cần map:
```csharp
.ForMember(dest => dest.Id, opt => opt.Ignore())
.ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
```

### 4. Complex Relationships
Map nested objects explicitly:
```csharp
.ForMember(dest => dest.Shop, opt => opt.MapFrom(src => src.Shop))
```

## Troubleshooting

### Common Issues
1. **Missing mappings** - Đảm bảo tất cả DTOs đều có mapping tương ứng
2. **Circular references** - Sử dụng `MaxDepth()` nếu cần
3. **Performance issues** - Sử dụng `MappingPerformanceTest` để identify bottlenecks
4. **Null reference exceptions** - Luôn check null cho navigation properties

### Debug Tips
1. Sử dụng `mapper.ConfigurationProvider.AssertConfigurationIsValid()` để validate configuration
2. Enable AutoMapper's built-in validation trong development environment
3. Sử dụng performance tests để monitor mapping speed với large datasets
