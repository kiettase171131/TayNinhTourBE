# 🎯 TOUR BOOKING INDIVIDUAL QR CODE IMPLEMENTATION PLAN

## 📋 **TỔNG QUAN**

**Mục tiêu**: Thay đổi hệ thống tour booking từ 1 QR code cho cả booking thành multiple QR codes riêng biệt cho từng guest giúp tourguide có thể checkin từng guest riêng biệt trong tourslot thay vì check in chung toàn bộ guest trong tourslot chỉ với 1 mã qr.

**Phương án**: Tạo entity `TourBookingGuest` mới với relationship 1-N với `TourBooking`

---

## 🏗️ **KIẾN TRÚC THIẾT KẾ**

### **Database Schema Changes**

```sql
-- Tạo bảng TourBookingGuest mới
CREATE TABLE TourBookingGuests (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    TourBookingId UNIQUEIDENTIFIER NOT NULL,
    GuestName NVARCHAR(100) NOT NULL,
    GuestEmail NVARCHAR(100) NOT NULL,
    GuestPhone NVARCHAR(20) NULL,
    QRCodeData NVARCHAR(MAX) NULL,
    IsCheckedIn BIT NOT NULL DEFAULT 0,
    CheckInTime DATETIME2 NULL,
    CheckInNotes NVARCHAR(500) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,
    
    CONSTRAINT FK_TourBookingGuests_TourBooking 
        FOREIGN KEY (TourBookingId) REFERENCES TourBookings(Id),
    CONSTRAINT UQ_TourBookingGuests_Email_Booking 
        UNIQUE (TourBookingId, GuestEmail)
);

-- Index cho performance
CREATE INDEX IX_TourBookingGuests_TourBookingId ON TourBookingGuests(TourBookingId);
CREATE INDEX IX_TourBookingGuests_Email ON TourBookingGuests(GuestEmail);
CREATE INDEX IX_TourBookingGuests_QRCodeData ON TourBookingGuests(QRCodeData);
```

### **Entity Relationship**

```
TourBooking (1) ←→ (N) TourBookingGuest
    ↓                      ↓
- TotalPrice           - GuestName (required)
- BookingCode          - GuestEmail (required, unique per booking)
- ContactPhone         - GuestPhone (optional)
- Status               - QRCodeData (individual)
- QRCodeData (remove)  - IsCheckedIn
                       - CheckInTime
```

---

## 🛠️ **IMPLEMENTATION PHASES**

### **Phase 1: Database & Entity Setup** ⏱️ **2-3 giờ**

#### **1.1 Tạo TourBookingGuest Entity**
```csharp
// File: TayNinhTourApi.DataAccessLayer/Entities/TourBookingGuest.cs
public class TourBookingGuest : BaseEntity
{
    public Guid TourBookingId { get; set; }
    
    [Required, StringLength(100)]
    public string GuestName { get; set; } = null!;
    
    [Required, StringLength(100), EmailAddress]
    public string GuestEmail { get; set; } = null!;
    
    [StringLength(20)]
    public string? GuestPhone { get; set; }
    
    public string? QRCodeData { get; set; }
    
    public bool IsCheckedIn { get; set; } = false;
    public DateTime? CheckInTime { get; set; }
    
    [StringLength(500)]
    public string? CheckInNotes { get; set; }
    
    // Navigation Properties
    public virtual TourBooking TourBooking { get; set; } = null!;
}
```

#### **1.2 Update TourBooking Entity**
```csharp
// Thêm vào TourBooking.cs
public virtual ICollection<TourBookingGuest> Guests { get; set; } = new List<TourBookingGuest>();

// DEPRECATED: Sẽ được remove trong migration tương lai
// public string? QRCodeData { get; set; }
```

#### **1.3 Entity Configuration**
```csharp
// File: TayNinhTourApi.DataAccessLayer/EntityConfigurations/TourBookingGuestConfiguration.cs
public class TourBookingGuestConfiguration : IEntityTypeConfiguration<TourBookingGuest>
{
    public void Configure(EntityTypeBuilder<TourBookingGuest> builder)
    {
        builder.ToTable("TourBookingGuests");
        
        // Unique constraint: Email phải unique trong cùng 1 booking
        builder.HasIndex(g => new { g.TourBookingId, g.GuestEmail })
               .IsUnique()
               .HasDatabaseName("UQ_TourBookingGuests_Email_Booking");
               
        // Foreign Key
        builder.HasOne(g => g.TourBooking)
               .WithMany(b => b.Guests)
               .HasForeignKey(g => g.TourBookingId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

#### **1.4 Database Migration**
```bash
# Tạo migration
dotnet ef migrations add AddTourBookingGuestEntity --project TayNinhTourApi.DataAccessLayer --startup-project TayNinhTourApi.Controller

# Apply migration
dotnet ef database update --project TayNinhTourApi.DataAccessLayer --startup-project TayNinhTourApi.Controller
```

---

### **Phase 2: DTOs & Request Models** ⏱️ **1-2 giờ**

#### **2.1 Update CreateTourBookingRequest**
```csharp
// File: TayNinhTourApi.BusinessLogicLayer/DTOs/Request/TourBooking/CreateTourBookingRequest.cs
public class CreateTourBookingRequest
{
    [Required]
    public Guid TourSlotId { get; set; }
    
    [Required, Range(1, 50)]
    public int NumberOfGuests { get; set; }
    
    [StringLength(20)]
    public string? ContactPhone { get; set; }
    
    [StringLength(1000)]
    public string? SpecialRequests { get; set; }
    
    // ← THÊM MỚI
    [Required]
    [MinLength(1, ErrorMessage = "Phải có ít nhất 1 guest")]
    public List<GuestInfoRequest> Guests { get; set; } = new();
}

public class GuestInfoRequest
{
    [Required, StringLength(100)]
    public string GuestName { get; set; } = null!;
    
    [Required, StringLength(100), EmailAddress]
    public string GuestEmail { get; set; } = null!;
    
    [StringLength(20)]
    public string? GuestPhone { get; set; }
}
```

#### **2.2 Update Response DTOs**
```csharp
// File: TayNinhTourApi.BusinessLogicLayer/DTOs/Response/TourBooking/TourBookingDto.cs
public class TourBookingDto
{
    // ... existing fields ...
    
    // ← THÊM MỚI
    public List<TourBookingGuestDto> Guests { get; set; } = new();
}

public class TourBookingGuestDto
{
    public Guid Id { get; set; }
    public string GuestName { get; set; } = null!;
    public string GuestEmail { get; set; } = null!;
    public string? GuestPhone { get; set; }
    public string? QRCodeData { get; set; }
    public bool IsCheckedIn { get; set; }
    public DateTime? CheckInTime { get; set; }
}
```

---

### **Phase 3: Service Layer Updates** ⏱️ **3-4 giờ**

#### **3.1 Update UserTourBookingService**

**Validation Logic:**
```csharp
// Thêm vào UserTourBookingService.cs
private async Task<(bool IsValid, string ErrorMessage)> ValidateGuestInfoAsync(
    List<GuestInfoRequest> guests, int numberOfGuests)
{
    // Validate guest count matches
    if (guests.Count != numberOfGuests)
        return (false, $"Số lượng guest info ({guests.Count}) không khớp với numberOfGuests ({numberOfGuests})");
    
    // Validate unique emails
    var emailGroups = guests.GroupBy(g => g.GuestEmail.ToLower());
    var duplicateEmails = emailGroups.Where(g => g.Count() > 1).Select(g => g.Key);
    
    if (duplicateEmails.Any())
        return (false, $"Email guest phải khác nhau. Email trùng: {string.Join(", ", duplicateEmails)}");
    
    // Validate required fields
    foreach (var guest in guests)
    {
        if (string.IsNullOrWhiteSpace(guest.GuestName))
            return (false, "Tên guest không được để trống");
            
        if (string.IsNullOrWhiteSpace(guest.GuestEmail))
            return (false, "Email guest không được để trống");
            
        if (!IsValidEmail(guest.GuestEmail))
            return (false, $"Email không hợp lệ: {guest.GuestEmail}");
    }
    
    return (true, string.Empty);
}
```

**Create Booking Logic:**
```csharp
// Update CreateTourBookingAsync method
public async Task<CreateBookingResultDto> CreateTourBookingAsync(
    CreateTourBookingRequest request, Guid userId)
{
    // ... existing validation ...
    
    // ← THÊM VALIDATION MỚI
    var guestValidation = await ValidateGuestInfoAsync(request.Guests, request.NumberOfGuests);
    if (!guestValidation.IsValid)
    {
        return new CreateBookingResultDto
        {
            Success = false,
            Message = guestValidation.ErrorMessage
        };
    }
    
    // ... existing booking creation ...
    
    // ← THÊM TẠO GUESTS
    foreach (var guestInfo in request.Guests)
    {
        var guest = new TourBookingGuest
        {
            Id = Guid.NewGuid(),
            TourBookingId = booking.Id,
            GuestName = guestInfo.GuestName,
            GuestEmail = guestInfo.GuestEmail,
            GuestPhone = guestInfo.GuestPhone,
            IsCheckedIn = false
        };
        
        await _unitOfWork.TourBookingGuestRepository.AddAsync(guest);
    }
    
    // ... rest of logic ...
}
```

#### **3.2 Update QRCodeService**

**Individual QR Generation:**
```csharp
// Thêm method mới vào QRCodeService.cs
public string GenerateGuestQRCodeData(TourBookingGuest guest, TourBooking booking)
{
    var qrData = new
    {
        // Guest-specific info
        GuestId = guest.Id,
        GuestName = guest.GuestName,
        GuestEmail = guest.GuestEmail,
        GuestPhone = guest.GuestPhone,
        
        // Booking info
        BookingId = booking.Id,
        BookingCode = booking.BookingCode,
        TourOperationId = booking.TourOperationId,
        TourSlotId = booking.TourSlotId,
        
        // Pricing info
        TotalPrice = booking.TotalPrice,
        NumberOfGuests = booking.NumberOfGuests,
        
        // Tour info
        TourTitle = booking.TourOperation?.TourDetails?.Title,
        TourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue),
        
        // Verification
        GeneratedAt = DateTime.UtcNow,
        Version = "3.0", // New version for individual guests
        Type = "IndividualGuest"
    };
    
    return JsonSerializer.Serialize(qrData, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    });
}
```

#### **3.3 Update EmailSender**

**Individual Email Method:**
```csharp
// Thêm vào EmailSender.cs
public async Task SendIndividualGuestBookingConfirmationAsync(
    TourBookingGuest guest,
    TourBooking booking,
    string tourTitle,
    DateTime tourDate,
    byte[] qrCodeImage)
{
    var message = new MimeMessage();
    message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
    message.To.Add(new MailboxAddress(guest.GuestName, guest.GuestEmail));
    message.Subject = $"Tour Booking Confirmed - QR Ticket for {guest.GuestName}";
    
    var bodyBuilder = new BodyBuilder();
    
    // Add QR code as embedded image
    var qrCodeAttachment = bodyBuilder.Attachments.Add("qr-ticket.png", qrCodeImage, new ContentType("image", "png"));
    qrCodeAttachment.ContentDisposition = new ContentDisposition(ContentDisposition.Inline);
    qrCodeAttachment.ContentId = "guest-qr-code";
    
    bodyBuilder.HtmlBody = $@"
    <div style=""font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;"">
        <h1 style=""color: #2c3e50; text-align: center;"">🎫 Your Personal Tour Ticket</h1>
        
        <div style=""background: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;"">
            <h3 style=""color: #495057; margin-top: 0;"">Guest Information</h3>
            <p><strong>Name:</strong> {guest.GuestName}</p>
            <p><strong>Email:</strong> {guest.GuestEmail}</p>
            {(!string.IsNullOrEmpty(guest.GuestPhone) ? $"<p><strong>Phone:</strong> {guest.GuestPhone}</p>" : "")}
        </div>
        
        <div style=""background: #e3f2fd; padding: 20px; border-radius: 8px; margin: 20px 0;"">
            <h3 style=""color: #1976d2; margin-top: 0;"">Tour Details</h3>
            <p><strong>Tour:</strong> {tourTitle}</p>
            <p><strong>Date:</strong> {tourDate:dd/MM/yyyy HH:mm}</p>
            <p><strong>Booking Code:</strong> {booking.BookingCode}</p>
            <p><strong>Total Guests:</strong> {booking.NumberOfGuests} people</p>
            <p><strong>Total Amount:</strong> {booking.TotalPrice:N0} VNĐ</p>
        </div>
        
        <div style=""text-align: center; margin: 30px 0;"">
            <h3 style=""color: #d32f2f;"">🔍 Your Personal QR Code</h3>
            <p style=""color: #666; margin-bottom: 20px;"">Present this QR code to the tour guide for check-in</p>
            <img src=""cid:guest-qr-code"" alt=""Personal QR Code"" style=""max-width: 300px; border: 2px solid #ddd; border-radius: 8px;"" />
        </div>
        
        <div style=""background: #fff3cd; padding: 15px; border-radius: 8px; border-left: 4px solid #ffc107;"">
            <h4 style=""color: #856404; margin-top: 0;"">⚠️ Important Notes</h4>
            <ul style=""color: #856404; margin: 0;"">
                <li>This QR code is personal and non-transferable</li>
                <li>Please arrive 15 minutes before tour start time</li>
                <li>Bring a valid ID for verification</li>
                <li>Contact us if you have any questions</li>
            </ul>
        </div>
        
        <p style=""text-align: center; margin-top: 30px; color: #666;"">
            Thank you for choosing Tay Ninh Tour!<br/>
            <strong>The Tay Ninh Tour Team</strong>
        </p>
    </div>";
    
    message.Body = bodyBuilder.ToMessageBody();
    await SendEmailAsync(message);
}
```

---

### **Phase 4: Payment Success Handler Update** ⏱️ **2-3 giờ**

#### **4.1 Update HandlePaymentSuccessAsync**
```csharp
// Update trong UserTourBookingService.cs
public async Task<BaseResposeDto> HandlePaymentSuccessAsync(string payOsOrderCode)
{
    // ... existing logic until booking confirmation ...
    
    // ← THAY ĐỔI: Generate QR codes cho từng guest thay vì booking
    var guests = await _unitOfWork.TourBookingGuestRepository
        .GetQueryable()
        .Where(g => g.TourBookingId == booking.Id && !g.IsDeleted)
        .ToListAsync();
    
    if (!guests.Any())
    {
        _logger.LogError("No guests found for booking {BookingId}", booking.Id);
        return new BaseResposeDto
        {
            StatusCode = 500,
            Message = "Không tìm thấy thông tin khách hàng"
        };
    }
    
    // Generate QR code cho từng guest
    foreach (var guest in guests)
    {
        guest.QRCodeData = _qrCodeService.GenerateGuestQRCodeData(guest, booking);
        await _unitOfWork.TourBookingGuestRepository.UpdateAsync(guest);
    }
    
    // ← REMOVE: booking.QRCodeData = _qrCodeService.GenerateQRCodeData(booking);
    
    await _unitOfWork.SaveChangesAsync();
    await transaction.CommitAsync();
    
    // ← THAY ĐỔI: Send individual emails
    await SendIndividualGuestEmailsAsync(booking, guests);
    
    return new BaseResposeDto
    {
        StatusCode = 200,
        Message = "Thanh toán thành công và đã gửi QR code cho từng khách hàng"
    };
}

private async Task SendIndividualGuestEmailsAsync(TourBooking booking, List<TourBookingGuest> guests)
{
    var tourTitle = booking.TourOperation?.TourDetails?.Title ?? "Tour Experience";
    var tourDate = booking.TourSlot?.TourDate.ToDateTime(TimeOnly.MinValue) ?? VietnamTimeZoneUtility.GetVietnamNow();
    
    var emailTasks = guests.Select(async guest =>
    {
        try
        {
            var qrCodeImage = await _qrCodeService.GenerateQRCodeImageFromDataAsync(guest.QRCodeData!, 300);
            
            await _emailSender.SendIndividualGuestBookingConfirmationAsync(
                guest, booking, tourTitle, tourDate, qrCodeImage);
                
            _logger.LogInformation("Sent individual email to guest {GuestName} ({GuestEmail}) for booking {BookingCode}",
                guest.GuestName, guest.GuestEmail, booking.BookingCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to guest {GuestName} ({GuestEmail}) for booking {BookingCode}",
                guest.GuestName, guest.GuestEmail, booking.BookingCode);
        }
    });
    
    await Task.WhenAll(emailTasks);
}
```

---

### **Phase 5: Repository Updates** ⏱️ **1-2 giờ**

#### **5.1 Tạo TourBookingGuestRepository**
```csharp
// File: TayNinhTourApi.DataAccessLayer/Repositories/Interface/ITourBookingGuestRepository.cs
public interface ITourBookingGuestRepository : IGenericRepository<TourBookingGuest>
{
    Task<List<TourBookingGuest>> GetGuestsByBookingIdAsync(Guid bookingId);
    Task<TourBookingGuest?> GetGuestByQRCodeAsync(string qrCodeData);
    Task<bool> IsEmailUniqueInBookingAsync(Guid bookingId, string email, Guid? excludeGuestId = null);
}

// File: TayNinhTourApi.DataAccessLayer/Repositories/TourBookingGuestRepository.cs
public class TourBookingGuestRepository : GenericRepository<TourBookingGuest>, ITourBookingGuestRepository
{
    public async Task<List<TourBookingGuest>> GetGuestsByBookingIdAsync(Guid bookingId)
    {
        return await _context.TourBookingGuests
            .Where(g => g.TourBookingId == bookingId && !g.IsDeleted)
            .OrderBy(g => g.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<TourBookingGuest?> GetGuestByQRCodeAsync(string qrCodeData)
    {
        return await _context.TourBookingGuests
            .Include(g => g.TourBooking)
                .ThenInclude(b => b.TourSlot)
            .FirstOrDefaultAsync(g => g.QRCodeData == qrCodeData && !g.IsDeleted);
    }
    
    public async Task<bool> IsEmailUniqueInBookingAsync(Guid bookingId, string email, Guid? excludeGuestId = null)
    {
        var query = _context.TourBookingGuests
            .Where(g => g.TourBookingId == bookingId && 
                       g.GuestEmail.ToLower() == email.ToLower() && 
                       !g.IsDeleted);
                       
        if (excludeGuestId.HasValue)
            query = query.Where(g => g.Id != excludeGuestId.Value);
            
        return !await query.AnyAsync();
    }
}
```

#### **5.2 Update UnitOfWork**
```csharp
// Thêm vào IUnitOfWork.cs
public interface IUnitOfWork
{
    // ... existing repositories ...
    ITourBookingGuestRepository TourBookingGuestRepository { get; }
}

// Thêm vào UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    // ... existing repositories ...
    public ITourBookingGuestRepository TourBookingGuestRepository { get; private set; }
    
    public UnitOfWork(TayNinhTouApiDbContext context)
    {
        // ... existing initialization ...
        TourBookingGuestRepository = new TourBookingGuestRepository(context);
    }
}
```

---

### **Phase 6: Tour Guide Check-in Updates** ⏱️ **2-3 giờ**

#### **6.1 Update TourGuideController**
```csharp
// Update check-in logic trong TourGuideController.cs
[HttpPost("check-in-guest")]
public async Task<IActionResult> CheckInGuest([FromBody] CheckInGuestRequest request)
{
    try
    {
        // ← THAY ĐỔI: Tìm guest thay vì booking
        var guest = await _unitOfWork.TourBookingGuestRepository.GetGuestByQRCodeAsync(request.QRCodeData);
        
        if (guest == null)
        {
            return NotFound(new BaseResposeDto
            {
                StatusCode = 404,
                Message = "Không tìm thấy thông tin khách hàng với QR code này"
            });
        }
        
        if (guest.IsCheckedIn)
        {
            return BadRequest(new BaseResposeDto
            {
                StatusCode = 400,
                Message = $"Khách hàng {guest.GuestName} đã được check-in trước đó"
            });
        }
        
        // Validate tour guide có quyền check-in guest này không
        var tourGuide = await GetCurrentTourGuideAsync();
        var hasPermission = await ValidateTourGuidePermissionAsync(tourGuide.Id, guest.TourBooking.TourSlotId);
        
        if (!hasPermission)
        {
            return Forbid("Bạn không có quyền check-in cho tour slot này");
        }
        
        // Perform check-in
        guest.IsCheckedIn = true;
        guest.CheckInTime = DateTime.UtcNow;
        guest.CheckInNotes = request.Notes;
        
        await _unitOfWork.TourBookingGuestRepository.UpdateAsync(guest);
        await _unitOfWork.SaveChangesAsync();
        
        return Ok(new BaseResposeDto
        {
            StatusCode = 200,
            Message = $"Check-in thành công cho khách hàng {guest.GuestName}"
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error checking in guest with QR: {QRCode}", request.QRCodeData);
        return StatusCode(500, new BaseResposeDto
        {
            StatusCode = 500,
            Message = "Có lỗi xảy ra khi check-in"
        });
    }
}
```

---

### **Phase 7: API Endpoints Updates** ⏱️ **1-2 giờ**

#### **7.1 Update UserTourBookingController**
```csharp
// Update existing endpoints để include guests
[HttpGet("my-bookings")]
public async Task<IActionResult> GetMyBookings([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
{
    // ... existing logic ...
    
    // ← THÊM: Include guests trong response
    var bookingsWithGuests = await _unitOfWork.TourBookingRepository
        .GetQueryable()
        .Where(b => b.UserId == userId && !b.IsDeleted)
        .Include(b => b.Guests.Where(g => !g.IsDeleted))
        .Include(b => b.TourOperation)
            .ThenInclude(to => to.TourDetails)
        .OrderByDescending(b => b.BookingDate)
        .Skip(pageIndex * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    // Map to DTOs including guests
    var bookingDtos = bookingsWithGuests.Select(MapToBookingDtoWithGuests).ToList();
    
    return Ok(new ApiResponse<PagedResult<TourBookingDto>>
    {
        Success = true,
        Data = new PagedResult<TourBookingDto>
        {
            Items = bookingDtos,
            TotalCount = totalCount,
            PageIndex = pageIndex,
            PageSize = pageSize
        }
    });
}
```

---

## 🧪 **TESTING STRATEGY**

### **Unit Tests**
```csharp
// File: TayNinhTourApi.Tests/Services/UserTourBookingServiceTests.cs
[Test]
public async Task CreateTourBookingAsync_WithMultipleGuests_ShouldCreateGuestRecords()
{
    // Arrange
    var request = new CreateTourBookingRequest
    {
        TourSlotId = Guid.NewGuid(),
        NumberOfGuests = 3,
        Guests = new List<GuestInfoRequest>
        {
            new() { GuestName = "Guest 1", GuestEmail = "guest1@test.com" },
            new() { GuestName = "Guest 2", GuestEmail = "guest2@test.com" },
            new() { GuestName = "Guest 3", GuestEmail = "guest3@test.com" }
        }
    };
    
    // Act
    var result = await _service.CreateTourBookingAsync(request, userId);
    
    // Assert
    Assert.IsTrue(result.Success);
    Assert.AreEqual(3, result.BookingData.Guests.Count);
}

[Test]
public async Task CreateTourBookingAsync_WithDuplicateEmails_ShouldReturnError()
{
    // Test duplicate email validation
}

[Test]
public async Task HandlePaymentSuccessAsync_ShouldGenerateIndividualQRCodes()
{
    // Test QR generation cho từng guest
}
```

### **Integration Tests**
```bash
# Test API endpoints
curl -X POST "http://localhost:5267/api/user-tour-booking/create-booking" \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "tourSlotId": "guid",
    "numberOfGuests": 2,
    "contactPhone": "0123456789",
    "guests": [
      {
        "guestName": "Nguyen Van A",
        "guestEmail": "a@test.com",
        "guestPhone": "0123456789"
      },
      {
        "guestName": "Tran Thi B", 
        "guestEmail": "b@test.com"
      }
    ]
  }'
```

---

## 📋 **MIGRATION CHECKLIST**



### **Implementation Steps**
- [ ] **Phase 1**: Database & Entity Setup
- [ ] **Phase 2**: DTOs & Request Models  
- [ ] **Phase 3**: Service Layer Updates
- [ ] **Phase 4**: Payment Success Handler
- [ ] **Phase 5**: Repository Updates
- [ ] **Phase 6**: Tour Guide Check-in Updates
- [ ] **Phase 7**: API Endpoints Updates

### **Testing & Validation**
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing với multiple guests
- [ ] Email delivery testing
- [ ] QR code generation testing
- [ ] Tour guide check-in testing

### **Deployment**
- [ ] Database migration applied
- [ ] Backend deployed
- [ ] Frontend updated
- [ ] Documentation updated

---

## ⚠️ **RISKS & MITIGATION**

### **Potential Risks**
1. **Email Overload**: Gửi nhiều email cùng lúc có thể bị rate limit
2. **QR Code Conflicts**: Cần đảm bảo QR codes unique

### **Mitigation Strategies**
1. **Backward Compatibility**: Giữ logic cũ cho bookings không có guests
2. **Email Throttling**: Delay giữa các email sends
3. **QR Validation**: Thêm timestamp và random salt vào QR data

---



## ✅ **DEFINITION OF DONE**

<input disabled="" type="checkbox"> TourBookingGuest entity được tạo và migration applied
<input disabled="" type="checkbox"> Multiple QR codes được generate cho từng guest
<input disabled="" type="checkbox"> Individual emails được gửi thành công
<input disabled="" type="checkbox"> Tour guide có thể check-in từng guest riêng biệt
<input disabled="" type="checkbox"> Validation email unique trong booking hoạt động
<input disabled="" type="checkbox"> Backward compatibility với bookings cũ
<input disabled="" type="checkbox"> Unit và integration tests pass
<input disabled="" type="checkbox"> Documentation được update

---

**🎯 Plan này đảm bảo implementation clean, scalable và không ảnh hưởng đến payment system hiện tại.**
