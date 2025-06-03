# TourTemplateService Documentation

## Tổng Quan

TayNinhTourBE project có hai implementations của ITourTemplateService:

1. **TourTemplateService.cs** - Implementation cơ bản hiện tại
2. **EnhancedTourTemplateService.cs** - Implementation cải tiến với validation, image handling và proper response DTOs

## EnhancedTourTemplateService Features

### 1. Comprehensive Validation
- **Request Validation**: Validate tất cả input data với business rules
- **Image Validation**: Kiểm tra format, size, và existence của images
- **Business Rules Validation**: Validate logic nghiệp vụ (price consistency, guest capacity, etc.)
- **Permission Validation**: Kiểm tra quyền user trước khi thực hiện actions

### 2. Enhanced Response DTOs
- **ResponseCreateTourTemplateDto**: Response cho create operations
- **ResponseUpdateTourTemplateDto**: Response cho update operations  
- **ResponseDeleteTourTemplateDto**: Response cho delete operations
- **ResponseGetTourTemplateDto**: Response cho get operations
- **ResponseCopyTourTemplateDto**: Response cho copy operations
- **ResponseValidationDto**: Response cho validation checks
- **ResponseCanDeleteDto**: Response cho delete capability checks
- **ResponseTourTemplateStatisticsDto**: Response cho statistics

### 3. Image Handling
- **TourTemplateImageHandler**: Utility class cho image operations
- **Image Validation**: Format checking, duplicate detection, existence verification
- **Image Management**: Add, update, remove images từ templates
- **Image Copying**: Copy images khi duplicate templates
- **Image Cleanup**: Remove unused images (maintenance)

### 4. Advanced Business Logic
- **Permission Checks**: Chỉ owner mới có thể modify templates
- **Delete Validation**: Kiểm tra dependencies trước khi delete
- **Business Rules**: Enforce consistency rules (price, capacity, schedule)
- **Statistics**: Comprehensive statistics với breakdown by type, location, etc.

## API Methods

### Core CRUD Operations

#### CreateTourTemplateAsync
```csharp
Task<ResponseCreateTourTemplateDto> CreateTourTemplateAsync(RequestCreateTourTemplateDto request, Guid createdById)
```
- Validates request data
- Validates images if provided
- Checks business rules
- Creates template with proper audit fields
- Returns detailed response with status codes

#### UpdateTourTemplateAsync
```csharp
Task<ResponseUpdateTourTemplateDto> UpdateTourTemplateAsync(Guid id, RequestUpdateTourTemplateDto request, Guid updatedById)
```
- Validates permissions (only owner can update)
- Validates update data
- Handles image updates
- Checks business rules after update
- Returns detailed response

#### DeleteTourTemplateAsync
```csharp
Task<ResponseDeleteTourTemplateDto> DeleteTourTemplateAsync(Guid id, Guid deletedById)
```
- Validates permissions
- Checks if template can be deleted (no dependencies)
- Performs soft delete
- Returns detailed response

#### GetTourTemplateByIdAsync
```csharp
Task<ResponseGetTourTemplateDto> GetTourTemplateByIdAsync(Guid id)
```
- Returns template with full details
- Includes relationships (Images, TourDetails, etc.)
- Proper error handling for not found

### Advanced Operations

#### CopyTourTemplateAsync
```csharp
Task<ResponseCopyTourTemplateDto> CopyTourTemplateAsync(Guid id, string newTitle, Guid createdById)
```
- Creates copy of existing template
- Copies all properties and relationships
- Copies images safely
- Generates new IDs for copied template

#### GetTourTemplatesPaginatedAsync
```csharp
Task<ResponseGetTourTemplatesDto> GetTourTemplatesPaginatedAsync(...)
```
- Supports pagination
- Multiple filter options (type, price range, location)
- Proper error handling
- Returns total count and page info

#### SetTourTemplateActiveStatusAsync
```csharp
Task<ResponseSetActiveStatusDto> SetTourTemplateActiveStatusAsync(Guid id, bool isActive, Guid updatedById)
```
- Validates permissions
- Updates active status
- Proper audit trail
- Returns new status

### Validation Methods

#### ValidateCreateRequestAsync
```csharp
Task<ResponseValidationDto> ValidateCreateRequestAsync(RequestCreateTourTemplateDto request)
```
- Comprehensive validation for create requests
- Includes image validation
- Returns detailed validation errors

#### ValidateUpdateRequestAsync
```csharp
Task<ResponseValidationDto> ValidateUpdateRequestAsync(Guid id, RequestUpdateTourTemplateDto request)
```
- Validates update requests against existing data
- Partial validation (only validates provided fields)
- Includes image validation

### Statistics and Analytics

#### GetTourTemplateStatisticsAsync
```csharp
Task<ResponseTourTemplateStatisticsDto> GetTourTemplateStatisticsAsync(Guid? createdById = null)
```
- Comprehensive statistics
- Breakdown by type, location
- Price analytics (min, max, average)
- TODO: Booking and revenue statistics

#### CanDeleteTourTemplateAsync
```csharp
Task<ResponseCanDeleteDto> CanDeleteTourTemplateAsync(Guid id)
```
- Checks if template can be safely deleted
- Lists blocking reasons
- Checks dependencies (tour slots, bookings)

## Validation Rules

### Create Validation
- **Title**: Required, max 200 characters
- **Price**: >= 0, <= 100,000,000 VND
- **MaxGuests**: > 0, <= 1000
- **MinGuests**: >= 1, <= MaxGuests
- **Duration**: > 0, <= 30 days
- **StartLocation**: Required
- **EndLocation**: Required
- **ChildPrice**: >= 0, <= adult price (if provided)
- **ChildMaxAge**: 1-17 (if child price provided)

### Update Validation
- Same rules as create but only for provided fields
- Validates against existing template data
- Ensures consistency after partial updates

### Business Rules
- Group tours must have weekend schedule days
- Child price cannot exceed adult price
- Min guests cannot exceed max guests
- Template type must be valid for schedule days

### Image Validation
- Max 10 images per template
- Supported formats: .png, .jpg, .jpeg, .webp
- No duplicate URLs
- Images must exist in database
- Proper error messages for each validation failure

## Error Handling

### Status Codes
- **200**: Success
- **201**: Created successfully
- **400**: Bad request (validation errors)
- **403**: Forbidden (permission denied)
- **404**: Not found
- **500**: Internal server error

### Error Response Format
```json
{
  "statusCode": 400,
  "message": "Dữ liệu không hợp lệ",
  "data": null,
  "validationErrors": [
    "Tiêu đề tour template là bắt buộc",
    "Giá tour phải lớn hơn hoặc bằng 0"
  ],
  "fieldErrors": {
    "Title": ["Tiêu đề tour template là bắt buộc"],
    "Price": ["Giá tour phải lớn hơn hoặc bằng 0"]
  }
}
```

## Usage Examples

### Creating a Tour Template
```csharp
var request = new RequestCreateTourTemplateDto
{
    Title = "Tour Núi Bà Đen",
    Description = "Tour khám phá núi Bà Đen",
    Price = 500000,
    MaxGuests = 20,
    MinGuests = 5,
    Duration = 1,
    StartLocation = "TP.HCM",
    EndLocation = "Tây Ninh",
    ImageUrls = new List<string> { "image1.jpg", "image2.jpg" }
};

var result = await tourTemplateService.CreateTourTemplateAsync(request, userId);
if (result.StatusCode == 201)
{
    // Success
    var createdTemplate = result.Data;
}
else
{
    // Handle errors
    var errors = result.ValidationErrors;
}
```

### Updating a Tour Template
```csharp
var updateRequest = new RequestUpdateTourTemplateDto
{
    Title = "Tour Núi Bà Đen - Cập nhật",
    Price = 550000
};

var result = await tourTemplateService.UpdateTourTemplateAsync(templateId, updateRequest, userId);
```

### Validating Before Create
```csharp
var validationResult = await tourTemplateService.ValidateCreateRequestAsync(request);
if (!validationResult.IsValid)
{
    // Show validation errors to user
    foreach (var error in validationResult.ValidationErrors)
    {
        Console.WriteLine(error);
    }
}
```

## Migration from TourTemplateService to EnhancedTourTemplateService

### Step 1: Update Service Registration
```csharp
// In Program.cs
// Replace:
builder.Services.AddScoped<ITourTemplateService, TourTemplateService>();
// With:
builder.Services.AddScoped<ITourTemplateService, EnhancedTourTemplateService>();
```

### Step 2: Update Controller Methods
Controllers need to handle new response DTOs instead of direct entities:

```csharp
// Old way:
var template = await _tourTemplateService.CreateTourTemplateAsync(request, userId);
return Ok(template);

// New way:
var response = await _tourTemplateService.CreateTourTemplateAsync(request, userId);
return StatusCode(response.StatusCode, response);
```

### Step 3: Update Error Handling
Controllers can now use proper status codes from responses:

```csharp
var response = await _tourTemplateService.GetTourTemplateByIdAsync(id);
if (response.StatusCode == 404)
{
    return NotFound(response);
}
return Ok(response);
```

## Performance Considerations

### Image Handling
- Images are validated asynchronously
- Bulk image operations are optimized
- Unused image cleanup is available for maintenance

### Database Queries
- Proper use of Include() for relationships
- Pagination for large datasets
- Efficient filtering with LINQ

### Caching Opportunities
- Statistics can be cached
- Popular templates can be cached
- Image metadata can be cached

## Future Enhancements

### TODO Items
1. **Booking Statistics**: Add real booking and revenue data to statistics
2. **Image Optimization**: Add image resizing and optimization
3. **Template Versioning**: Track template changes over time
4. **Bulk Operations**: Support bulk create/update/delete
5. **Template Templates**: Create templates from other templates
6. **Advanced Search**: Full-text search with Elasticsearch
7. **Audit Logging**: Detailed audit trail for all operations
8. **Performance Monitoring**: Add performance metrics and monitoring
