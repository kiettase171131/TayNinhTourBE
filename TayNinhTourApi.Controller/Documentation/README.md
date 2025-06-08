# TayNinhTour API Documentation

## 📚 Overview

Đây là tài liệu API chính thức cho hệ thống TayNinhTour, tập trung vào TourTemplate system và các chức năng liên quan.

---

## 📋 Available Documentation

### 1. **TourTemplate API Documentation**
📄 **File**: [TourTemplate_API_Documentation.md](./TourTemplate_API_Documentation.md)

**Nội dung**:
- ✅ Complete API reference cho TourTemplate endpoints
- ✅ Request/Response examples với real data
- ✅ Authentication và authorization requirements
- ✅ Error handling và status codes
- ✅ Query parameters và filtering options
- ✅ Business rules và validation constraints
- ✅ cURL examples cho testing

**Endpoints covered**:
- `GET /api/TourCompany/template` - List templates
- `GET /api/TourCompany/template/{id}` - Get template by ID
- `POST /api/TourCompany/template` - Create template
- `PATCH /api/TourCompany/template/{id}` - Update template
- `DELETE /api/TourCompany/template/{id}` - Delete template
- `POST /api/TourCompany/template/{id}/copy` - Copy template
- Related APIs: TourSlot, TourDetails, Migration

### 2. **OpenAPI Specification**
📄 **File**: [TourTemplate_OpenAPI.yaml](./TourTemplate_OpenAPI.yaml)

**Nội dung**:
- ✅ Complete OpenAPI 3.0.3 specification
- ✅ Machine-readable API definition
- ✅ Schema definitions cho all data models
- ✅ Security schemes (JWT Bearer)
- ✅ Response examples và error responses
- ✅ Validation rules và constraints
- ✅ Compatible với Swagger UI, Postman, etc.

**Usage**:
```bash
# Import vào Postman
# Load vào Swagger Editor
# Generate client SDKs
# API testing tools
```

### 3. **API Flows & Workflows**
📄 **File**: [TourTemplate_API_Flows.md](./TourTemplate_API_Flows.md)

**Nội dung**:
- ✅ **Detailed API flows** cho tất cả endpoints
- ✅ **Step-by-step workflows** với business logic
- ✅ **Sequence diagrams** (Mermaid) cho visual flows
- ✅ **State transition diagrams** cho entity lifecycle
- ✅ **Business process flows** cho complete workflows
- ✅ **Error handling flows** và recovery patterns
- ✅ **Integration patterns** với frontend/mobile
- ✅ **Performance considerations** và optimizations
- ✅ **Security flows** và authentication patterns

**Key Flows Covered**:
- Create TourTemplate flow (Authentication → Validation → Database)
- Generate Slots flow (Template validation → Date calculation → Creation)
- Update Template flow (Ownership → Dependencies → Partial update)
- Delete Template flow (Dependencies analysis → Conditional delete)
- Timeline Management flow (Shop integration → Order management)
- Migration flow (Preview → Execute → Rollback)

---

## 🚀 Quick Start

### 1. **Authentication**
Tất cả API calls yêu cầu JWT token:

```http
Authorization: Bearer {your-jwt-token}
```

**Required Role**: `TOURCOMPANY` (cho TourTemplate operations)

### 2. **Base URLs**
- **Production**: `https://api.tayninhour.com`
- **Development**: `http://localhost:5267`

### 3. **Content Type**
```http
Content-Type: application/json
```

### 4. **Basic Example**
```bash
curl -X GET "http://localhost:5267/api/TourCompany/template" \
  -H "Authorization: Bearer your-jwt-token" \
  -H "Content-Type: application/json"
```

---

## 📊 Key Business Rules

### **TourTemplateType**
- `FreeScenic` (1) - Tour danh lam thắng cảnh miễn phí
- `PaidAttraction` (2) - Tour khu vui chơi có phí

### **ScheduleDay Constraint**
- ⚠️ **IMPORTANT**: Chỉ được chọn `Saturday` HOẶC `Sunday`
- ❌ Không được chọn cả hai ngày
- ❌ Không được chọn ngày trong tuần

### **Slot Generation**
- 🎯 Tối đa **4 slots per month**
- 📅 Chỉ Saturday OR Sunday
- 🔄 Optimal distribution algorithm

### **Permissions**
- 🔐 TourTemplate: `TOURCOMPANY` role required
- 🔐 Migration: `ADMIN` role required
- 🔐 Owner-only operations: Update, Delete

---

## 🔧 Testing Tools

### **Swagger UI**
```
http://localhost:5267/swagger
```
- Interactive API documentation
- Test endpoints directly
- View request/response schemas

### **Postman Collection**
Import OpenAPI spec:
1. Open Postman
2. Import → Link → Paste OpenAPI YAML URL
3. Generate collection automatically

### **cURL Examples**
Xem trong [TourTemplate_API_Documentation.md](./TourTemplate_API_Documentation.md)

---

## 📈 API Versioning

**Current Version**: `v1.0`

**Versioning Strategy**:
- URL path versioning: `/api/v1/...`
- Backward compatibility maintained
- Deprecation notices for breaking changes

---

## 🔍 Error Handling

### **Standard Error Response**
```json
{
  "statusCode": 400,
  "message": "Dữ liệu không hợp lệ",
  "validationErrors": [
    "Chỉ được chọn Saturday hoặc Sunday"
  ],
  "fieldErrors": {
    "scheduleDays": ["Chỉ được chọn một ngày duy nhất"]
  }
}
```

### **Common Status Codes**
- `200` - Success
- `201` - Created
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (invalid token)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `409` - Conflict (dependencies exist)
- `500` - Internal Server Error

---

## 📊 Rate Limiting

- **Limit**: 100 requests per minute per user
- **Headers**:
  - `X-RateLimit-Limit`: Request limit
  - `X-RateLimit-Remaining`: Remaining requests
  - `X-RateLimit-Reset`: Reset time

---

## 🔗 Related Documentation

### **Business Logic**
- [TourTemplate Tasks](../../BusinessLogicLayer/README_TourTemplate_Tasks.md)
- [TourTemplate Quick Start](../../BusinessLogicLayer/README_TourTemplate_QuickStart.md)
- [Tour System Overhaul](../../BusinessLogicLayer/README_TourSystemOverhaul.md)

### **Database**
- [Entity Relationships](../../DataAccessLayer/README.md)
- [Migration Scripts](../../DataAccessLayer/Migrations/)

### **Development**
- [Setup Guide](../README.md)
- [Configuration](../appsettings.json)

---

## 📞 Support

### **Issues & Questions**
- 📧 **Email**: support@tayninhour.com
- 🐛 **Bug Reports**: Create issue in repository
- 💬 **Discussions**: Team chat channels

### **Development Support**
- 📖 **Swagger UI**: `http://localhost:5267/swagger`
- 🔍 **Logs**: Check application logs for detailed errors
- 🧪 **Testing**: Use provided examples và test data

---

## 📝 Changelog

### **v2.0.0** (June 7, 2025) - Simplified TourTemplate API
- ✅ **BREAKING CHANGE**: Đơn giản hóa RequestCreateTourTemplateDto (chỉ còn 9 fields)
- ✅ **Xóa fields**: Price, MaxGuests, MinGuests, Duration, ChildPrice, ChildMaxAge, Transportation, MealsIncluded, AccommodationInfo, IncludedServices, ExcludedServices, CancellationPolicy, SpecialRequirements
- ✅ **Thêm fields**: Month, Year cho template scheduling
- ✅ **Tự động generate slots**: Sau khi tạo template thành công
- ✅ **Cập nhật Response DTOs**: TourTemplateDto, TourTemplateDetailDto, TourTemplateSummaryDto
- ✅ **Xóa Price filters**: Từ GET templates API
- ✅ **Database migration**: SimplifyTourTemplate migration
- ✅ **Cập nhật documentation**: API docs, flows, examples

### **v1.0.0** (June 3, 2025)
- ✅ Initial TourTemplate API release
- ✅ Saturday OR Sunday constraint implementation
- ✅ 2 tour types (FreeScenic, PaidAttraction)
- ✅ Slot generation (4 per month)
- ✅ Timeline với shop integration
- ✅ Migration system từ Tour sang TourTemplate
- ✅ Complete API documentation
- ✅ OpenAPI specification

---

## 🎯 Next Steps

### **For Developers**
1. 📖 Read [TourTemplate_API_Documentation.md](./TourTemplate_API_Documentation.md)
2. 🔧 Import OpenAPI spec vào testing tools
3. 🧪 Test endpoints với Swagger UI
4. 💻 Implement client integration

### **For QA**
1. 🧪 Test all endpoints với provided examples
2. ✅ Verify business rules enforcement
3. 🔍 Test error scenarios
4. 📊 Validate response schemas

### **For Product**
1. 📋 Review API capabilities
2. 🎯 Plan feature enhancements
3. 📈 Monitor API usage metrics
4. 💡 Gather user feedback

---

**Last Updated**: June 3, 2025  
**Documentation Version**: 1.0  
**API Version**: v1.0
