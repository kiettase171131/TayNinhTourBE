# SPECIALTY SHOP IMPLEMENTATION PLAN
## Triển khai bảng SpecialtyShop và cập nhật flow Special Shop system

### 📋 TỔNG QUAN
Hiện tại hệ thống chỉ thay đổi User.RoleId khi approve ShopApplication, nhưng không có bảng riêng để lưu extended data của Special Shop owner. Plan này sẽ tạo bảng SpecialtyShop để lưu thông tin mở rộng.

### 🎯 MỤC TIÊU
1. Tạo bảng SpecialtyShop để lưu extended shop owner data
2. Cập nhật flow approval để tạo SpecialtyShop record
3. Thiết lập relationship 1:1 giữa User và SpecialtyShop
4. Cung cấp API để quản lý SpecialtyShop data

---

## 🗂️ PHASE 1: DATABASE SCHEMA & ENTITIES

### 1.1 Tạo SpecialtyShop Entity
**File:** `TayNinhTourApi.DataAccessLayer/Entities/SpecialtyShop.cs`

```csharp
public class SpecialtyShop : BaseEntity
{
    // Primary Key & Foreign Key
    public Guid UserId { get; set; }  // 1:1 với User
    
    // Shop Information
    public string ShopName { get; set; } = null!;
    public string? Description { get; set; }
    public string Location { get; set; } = null!;
    public string RepresentativeName { get; set; } = null!;
    
    // Contact Information
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    
    // Business Information
    public string? BusinessLicense { get; set; }
    public string? BusinessLicenseUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? ShopType { get; set; }
    
    // Operational Information
    public string? OpeningHours { get; set; }
    public decimal? Rating { get; set; }
    public bool IsShopActive { get; set; } = true;
    
    // Navigation Properties
    public virtual User User { get; set; } = null!;
}
```

### 1.2 Cập nhật User Entity
**File:** `TayNinhTourApi.DataAccessLayer/Entities/User.cs`

Thêm navigation property:
```csharp
public virtual SpecialtyShop? SpecialtyShop { get; set; }
```

### 1.3 Tạo Entity Configuration
**File:** `TayNinhTourApi.DataAccessLayer/EntityConfigurations/SpecialtyShopConfiguration.cs`

```csharp
public class SpecialtyShopConfiguration : IEntityTypeConfiguration<SpecialtyShop>
{
    public void Configure(EntityTypeBuilder<SpecialtyShop> builder)
    {
        // Primary Key
        builder.HasKey(s => s.Id);
        
        // Unique constraint on UserId (1:1 relationship)
        builder.HasIndex(s => s.UserId).IsUnique();
        
        // Required fields
        builder.Property(s => s.ShopName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Location).IsRequired().HasMaxLength(500);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(100);
        builder.Property(s => s.RepresentativeName).IsRequired().HasMaxLength(100);
        
        // Optional fields
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.PhoneNumber).HasMaxLength(20);
        builder.Property(s => s.Website).HasMaxLength(200);
        builder.Property(s => s.BusinessLicense).HasMaxLength(100);
        builder.Property(s => s.BusinessLicenseUrl).HasMaxLength(500);
        builder.Property(s => s.LogoUrl).HasMaxLength(500);
        builder.Property(s => s.ShopType).HasMaxLength(50);
        builder.Property(s => s.OpeningHours).HasMaxLength(100);
        builder.Property(s => s.Rating).HasColumnType("decimal(3,2)");
        
        // 1:1 Relationship with User
        builder.HasOne(s => s.User)
               .WithOne(u => u.SpecialtyShop)
               .HasForeignKey<SpecialtyShop>(s => s.UserId)
               .OnDelete(DeleteBehavior.Cascade);
               
        // Indexes
        builder.HasIndex(s => s.ShopName);
        builder.HasIndex(s => s.Email);
        builder.HasIndex(s => s.IsShopActive);
    }
}
```

### 1.4 Cập nhật DbContext
**File:** `TayNinhTourApi.DataAccessLayer/Contexts/TayNinhTouApiDbContext.cs`

Thêm DbSet:
```csharp
public DbSet<SpecialtyShop> SpecialtyShops { get; set; } = null!;
```

### 1.5 Tạo Migration
**Command:**
```bash
dotnet ef migrations add AddSpecialtyShopTable --project TayNinhTourApi.DataAccessLayer --startup-project TayNinhTourApi.Controller
```

---

## 🔧 PHASE 2: REPOSITORY & SERVICES

### 2.1 Tạo SpecialtyShop Repository Interface
**File:** `TayNinhTourApi.DataAccessLayer/Repositories/Interface/ISpecialtyShopRepository.cs`

```csharp
public interface ISpecialtyShopRepository : IGenericRepository<SpecialtyShop>
{
    Task<SpecialtyShop?> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<SpecialtyShop>> GetActiveShopsAsync();
    Task<IEnumerable<SpecialtyShop>> GetShopsByTypeAsync(string shopType);
    Task<bool> ExistsByUserIdAsync(Guid userId);
}
```

### 2.2 Implement SpecialtyShop Repository
**File:** `TayNinhTourApi.DataAccessLayer/Repositories/SpecialtyShopRepository.cs`

```csharp
public class SpecialtyShopRepository : GenericRepository<SpecialtyShop>, ISpecialtyShopRepository
{
    public SpecialtyShopRepository(TayNinhTouApiDbContext context) : base(context) { }

    public async Task<SpecialtyShop?> GetByUserIdAsync(Guid userId)
    {
        return await _context.SpecialtyShops
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<IEnumerable<SpecialtyShop>> GetActiveShopsAsync()
    {
        return await _context.SpecialtyShops
            .Where(s => s.IsShopActive && s.IsActive)
            .Include(s => s.User)
            .ToListAsync();
    }

    public async Task<IEnumerable<SpecialtyShop>> GetShopsByTypeAsync(string shopType)
    {
        return await _context.SpecialtyShops
            .Where(s => s.ShopType == shopType && s.IsShopActive && s.IsActive)
            .Include(s => s.User)
            .ToListAsync();
    }

    public async Task<bool> ExistsByUserIdAsync(Guid userId)
    {
        return await _context.SpecialtyShops.AnyAsync(s => s.UserId == userId);
    }
}
```

### 2.3 Cập nhật UnitOfWork
**File:** `TayNinhTourApi.DataAccessLayer/UnitOfWork/Interface/IUnitOfWork.cs`

Thêm property:
```csharp
ISpecialtyShopRepository? SpecialtyShopRepository { get; }
```

**File:** `TayNinhTourApi.DataAccessLayer/UnitOfWork/UnitOfWork.cs`

Thêm implementation:
```csharp
public ISpecialtyShopRepository? SpecialtyShopRepository { get; private set; }

// Trong constructor
SpecialtyShopRepository = new SpecialtyShopRepository(_context);
```

---

## 🎯 PHASE 3: BUSINESS LOGIC & DTOs

### 3.1 Tạo DTOs cho SpecialtyShop
**File:** `TayNinhTourApi.BusinessLogicLayer/DTOs/Request/SpecialtyShop/UpdateSpecialtyShopDto.cs`

```csharp
public class UpdateSpecialtyShopDto
{
    public string? ShopName { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public string? ShopType { get; set; }
    public string? OpeningHours { get; set; }
    public bool? IsShopActive { get; set; }
}
```

**File:** `TayNinhTourApi.BusinessLogicLayer/DTOs/Response/SpecialtyShop/SpecialtyShopResponseDto.cs`

```csharp
public class SpecialtyShopResponseDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string ShopName { get; set; } = null!;
    public string? Description { get; set; }
    public string Location { get; set; } = null!;
    public string RepresentativeName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public string? Website { get; set; }
    public string? BusinessLicense { get; set; }
    public string? BusinessLicenseUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? ShopType { get; set; }
    public string? OpeningHours { get; set; }
    public decimal? Rating { get; set; }
    public bool IsShopActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // User info
    public string UserName { get; set; } = null!;
    public string UserEmail { get; set; } = null!;
}
```

### 3.2 Tạo SpecialtyShop Service Interface
**File:** `TayNinhTourApi.BusinessLogicLayer/Services/Interface/ISpecialtyShopService.cs`

```csharp
public interface ISpecialtyShopService
{
    Task<ApiResponse<SpecialtyShopResponseDto>> GetMyShopAsync(CurrentUserObject currentUser);
    Task<ApiResponse<SpecialtyShopResponseDto>> UpdateMyShopAsync(UpdateSpecialtyShopDto updateDto, CurrentUserObject currentUser);
    Task<ApiResponse<List<SpecialtyShopResponseDto>>> GetAllActiveShopsAsync();
    Task<ApiResponse<List<SpecialtyShopResponseDto>>> GetShopsByTypeAsync(string shopType);
    Task<ApiResponse<SpecialtyShopResponseDto>> GetShopByIdAsync(Guid shopId);
}
```

---

## 🔄 PHASE 4: CẬP NHẬT SHOP APPLICATION FLOW

### 4.1 Cập nhật ShopApplicationService.ApproveAsync
**File:** `TayNinhTourApi.BusinessLogicLayer/Services/ShopApplicationService.cs`

Sửa method ApproveAsync để tạo SpecialtyShop record:

```csharp
public async Task<BaseResposeDto> ApproveAsync(Guid applicationId)
{
    using var transaction = _unitOfWork.BeginTransaction();
    try
    {
        var app = await _shopApplicationRepository.GetByIdAsync(applicationId);
        if (app.Status != ShopStatus.Pending)
        {
            return new BaseResposeDto
            {
                StatusCode = 400,
                Message = "Application is not pending, cannot approve!"
            };
        }

        // Get or create Specialty Shop role
        var shopRole = await _roleRepo.GetRoleByNameAsync(Constants.RoleShopName);
        if (shopRole == null)
        {
            shopRole = new Role
            {
                Id = Guid.NewGuid(),
                Name = Constants.RoleShopName,
                CreatedAt = DateTime.UtcNow
            };
            await _roleRepo.AddAsync(shopRole);
            await _roleRepo.SaveChangesAsync();
        }

        // Update user role
        var user = await _userRepo.GetByIdAsync(app.UserId);
        if (user == null)
        {
            return new BaseResposeDto
            {
                StatusCode = 404,
                Message = "User not found"
            };
        }

        user.RoleId = shopRole.Id;
        user.UpdatedAt = DateTime.UtcNow;

        // Create SpecialtyShop record
        var specialtyShop = new SpecialtyShop
        {
            Id = Guid.NewGuid(),
            UserId = app.UserId,
            ShopName = app.Name,
            Description = app.Description,
            Location = app.Location,
            RepresentativeName = app.RepresentativeName,
            Email = app.Email,
            Website = app.Website,
            BusinessLicenseUrl = app.BusinessLicenseUrl,
            LogoUrl = app.LogoUrl,
            ShopType = app.ShopType,
            IsShopActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedById = app.UserId
        };

        await _unitOfWork.SpecialtyShopRepository!.AddAsync(specialtyShop);

        // Update application status
        app.Status = ShopStatus.Approved;
        app.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();

        // Send approval email
        await _emailSender.SendShopApprovalNotificationAsync(app.Email, user.Name);

        return new BaseResposeDto
        {
            StatusCode = 200,
            Message = "Application approved successfully"
        };
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

## 🌐 PHASE 5: API ENDPOINTS

### 5.1 Tạo SpecialtyShop Controller
**File:** `TayNinhTourApi.Controller/Controllers/SpecialtyShopController.cs`

```csharp
[Route("api/[controller]")]
[ApiController]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class SpecialtyShopController : ControllerBase
{
    private readonly ISpecialtyShopService _specialtyShopService;

    public SpecialtyShopController(ISpecialtyShopService specialtyShopService)
    {
        _specialtyShopService = specialtyShopService;
    }

    [HttpGet("my-shop")]
    [Authorize(Roles = "Specialty Shop")]
    public async Task<IActionResult> GetMyShop()
    {
        var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
        var result = await _specialtyShopService.GetMyShopAsync(currentUser);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("my-shop")]
    [Authorize(Roles = "Specialty Shop")]
    public async Task<IActionResult> UpdateMyShop([FromBody] UpdateSpecialtyShopDto updateDto)
    {
        var currentUser = await TokenHelper.Instance.GetThisUserInfo(HttpContext);
        var result = await _specialtyShopService.UpdateMyShopAsync(updateDto, currentUser);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActiveShops()
    {
        var result = await _specialtyShopService.GetAllActiveShopsAsync();
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("by-type/{shopType}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetShopsByType(string shopType)
    {
        var result = await _specialtyShopService.GetShopsByTypeAsync(shopType);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("{shopId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetShopById(Guid shopId)
    {
        var result = await _specialtyShopService.GetShopByIdAsync(shopId);
        return StatusCode(result.StatusCode, result);
    }
}
```

---

## ⚙️ PHASE 6: DEPENDENCY INJECTION & CONFIGURATION

### 6.1 Cập nhật Program.cs
**File:** `TayNinhTourApi.Controller/Program.cs`

Thêm service registrations:
```csharp
// Repository registrations
builder.Services.AddScoped<ISpecialtyShopRepository, SpecialtyShopRepository>();

// Service registrations  
builder.Services.AddScoped<ISpecialtyShopService, SpecialtyShopService>();
```

---

## 🧪 PHASE 7: TESTING PLAN

### 7.1 Database Migration Test
1. Chạy migration để tạo bảng SpecialtyShops
2. Verify foreign key constraints
3. Test unique constraint trên UserId

### 7.2 API Testing Sequence
1. **Test Shop Application Flow:**
   - User register → Login → Apply shop → Admin approve
   - Verify SpecialtyShop record được tạo
   - Verify User role thành "Specialty Shop"

2. **Test SpecialtyShop APIs:**
   - GET /api/SpecialtyShop/my-shop
   - PUT /api/SpecialtyShop/my-shop
   - GET /api/SpecialtyShop/active
   - GET /api/SpecialtyShop/by-type/{type}
   - GET /api/SpecialtyShop/{id}

### 7.3 Authorization Testing
1. Test role-based access cho "Specialty Shop" endpoints
2. Test anonymous access cho public endpoints
3. Test user không có SpecialtyShop record

---

## 📋 IMPLEMENTATION CHECKLIST

### Phase 1: Database & Entities
- [ ] Tạo SpecialtyShop entity
- [ ] Cập nhật User entity (navigation property)
- [ ] Tạo SpecialtyShopConfiguration
- [ ] Cập nhật DbContext
- [ ] Tạo và chạy migration

### Phase 2: Repository Layer
- [ ] Tạo ISpecialtyShopRepository interface
- [ ] Implement SpecialtyShopRepository
- [ ] Cập nhật IUnitOfWork interface
- [ ] Cập nhật UnitOfWork implementation

### Phase 3: Business Logic
- [ ] Tạo DTOs (Request/Response)
- [ ] Tạo ISpecialtyShopService interface
- [ ] Implement SpecialtyShopService
- [ ] Cập nhật AutoMapper profiles

### Phase 4: Update Application Flow
- [ ] Sửa ShopApplicationService.ApproveAsync
- [ ] Test approval flow tạo SpecialtyShop record
- [ ] Test transaction rollback

### Phase 5: API Layer
- [ ] Tạo SpecialtyShopController
- [ ] Test authorization attributes
- [ ] Test endpoint responses

### Phase 6: Configuration
- [ ] Cập nhật Program.cs DI registrations
- [ ] Test application startup

### Phase 7: Testing
- [ ] Test migration
- [ ] Test API endpoints
- [ ] Test authorization
- [ ] Test complete flow

---

## 🚀 DEPLOYMENT NOTES

1. **Database Migration:** Chạy migration trên production database
2. **Backward Compatibility:** Existing users với role "Specialty Shop" sẽ cần tạo SpecialtyShop record
3. **Data Migration Script:** Có thể cần script để migrate existing approved ShopApplications
4. **Monitoring:** Monitor performance của 1:1 relationship queries

---

## 📝 ADDITIONAL CONSIDERATIONS

1. **Data Consistency:** Ensure SpecialtyShop record luôn sync với approved ShopApplication
2. **Soft Delete:** SpecialtyShop inherit soft delete từ BaseEntity
3. **Audit Trail:** Track changes trong SpecialtyShop data
4. **Performance:** Index optimization cho frequent queries
5. **Security:** Validate user chỉ có thể access own shop data
