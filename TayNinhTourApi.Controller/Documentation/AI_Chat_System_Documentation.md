# H? th?ng AI Chat Chuyên Bi?t - API Documentation

## T?ng Quan
H? th?ng AI Chat ?ã ???c nâng c?p ?? h? tr? **3 lo?i chat chuyên bi?t**:
1. **Tour Chat** - T? v?n v? tours, giá c?, ??t ch?
2. **Product Chat** - T? v?n mua s?m s?n ph?m ??c s?n
3. **TayNinh Chat** - Thông tin v? l?ch s?, v?n hóa, ??a ?i?m Tây Ninh

## Architecture

### Core Components
```
AIChatController
??? AIChatService (Main orchestrator)
??? AISpecializedChatService (Type-specific logic)
??? AITourDataService (Tour database integration)
??? AIProductDataService (Product database integration)
??? GeminiAIService (AI backend integration)
```

### Database Schema
- **AIChatSession**: Phiên chat v?i ChatType enum
- **AIChatMessage**: Tin nh?n v?i metadata
- **AIChatType**: Enum (Tour=1, Product=2, TayNinh=3)

## API Endpoints

### 1. T?o Phiên Chat M?i
```http
POST /api/AiChat/sessions
Authorization: Bearer {token}
Content-Type: application/json

{
  "chatType": 1,  // 1=Tour, 2=Product, 3=TayNinh
  "firstMessage": "Tôi mu?n tìm tour Núi Bà ?en",
  "customTitle": "T? v?n tour Núi Bà ?en" // Optional
}
```

**Response:**
```json
{
  "success": true,
  "message": "T?o phiên chat thành công",
  "statusCode": 201,
  "chatSession": {
    "id": "guid",
    "title": "[Tour] T? v?n tour Núi Bà ?en",
    "status": "Active",
    "chatType": 1,
    "createdAt": "2024-12-15T00:00:00Z",
    "lastMessageAt": "2024-12-15T00:00:00Z",
    "messageCount": 0
  }
}
```

### 2. G?i Tin Nh?n
```http
POST /api/AiChat/messages
Authorization: Bearer {token}
Content-Type: application/json

{
  "sessionId": "guid",
  "message": "Tour nào có giá r? nh?t?",
  "includeContext": true,
  "contextMessageCount": 10
}
```

### 3. L?y Danh Sách Phiên Chat (Có l?c theo lo?i)
```http
GET /api/AiChat/sessions?chatType=1&page=0&pageSize=20
Authorization: Bearer {token}
```

### 4. L?y Tin Nh?n Trong Phiên
```http
GET /api/AiChat/sessions/{sessionId}/messages
Authorization: Bearer {token}
```

### 5. Th?ng Kê Phân Lo?i
```http
GET /api/AiChat/stats
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "totalSessions": 15,
    "activeSession": 12,
    "byType": {
      "tourSessions": 8,
      "productSessions": 4,
      "tayNinhSessions": 3
    }
  }
}
```

## Chat Types Behavior

### 1. Tour Chat (Type = 1)
**Ch?c n?ng:**
- T? v?n tours có s?n t? database
- L?c tours theo status = PUBLIC và có slot tr?ng
- Thông tin giá c?, l?ch trình, ?i?m ??n
- G?i ý tours theo ngân sách

**System Prompt:**
```
B?n là AI t? v?n tour du l?ch Tây Ninh chuyên nghi?p.
- T? v?n tours, giá c?, l?ch trình, ??t ch?
- Ch? gi?i thi?u tours có status PUBLIC và có slot tr?ng
- Không ??a ra thông tin sai l?ch v? tours
```

**Data Integration:**
- Truy c?p `TourSlot`, `TourTemplate`, `TourCompany`
- Filter theo `IsActive`, `Status = Available`, `AvailableSpots > 0`
- Hi?n th? tours FreeScenic vs PaidAttraction

### 2. Product Chat (Type = 2)
**Ch?c n?ng:**
- T? v?n s?n ph?m theo nhu c?u và ngân sách
- G?i ý s?n ph?m trending, sale, best-selling
- So sánh giá và ch?t l??ng
- Thông tin shop, rating, reviews

**System Prompt:**
```
B?n là AI t? v?n mua s?m s?n ph?m ??c s?n Tây Ninh.
- T? v?n s?n ph?m theo nhu c?u và ngân sách
- Ch? g?i ý s?n ph?m còn hàng (QuantityInStock > 0)
- ?u tiên s?n ph?m có rating cao
```

**Data Integration:**
- Truy c?p `Product`, `SpecialtyShop`, `ProductRating`
- Filter theo `IsActive`, `QuantityInStock > 0`
- Categories: Food, Souvenir, Jewelry, Clothing

### 3. TayNinh Chat (Type = 3)
**Ch?c n?ng:**
- Thông tin l?ch s?, v?n hóa, ??a lý Tây Ninh
- Gi?i thi?u các ??a ?i?m du l?ch
- ?m th?c ??c s?n vùng ??t này
- **GI?I H?N**: Ch? tr? l?i câu h?i v? Tây Ninh

**System Prompt:**
```
B?n là AI chuyên gia v? Tây Ninh - l?ch s?, v?n hóa, ??a ?i?m, ?m th?c.
- CH?N tr? l?i câu h?i KHÔNG LIÊN QUAN ??n Tây Ninh
- N?u h?i v? ch? ?? khác: 'Tôi ch? chuyên t? v?n v? Tây Ninh...'
```

**No Data Integration:** Ch? d?a trên AI knowledge

## Advanced Features

### 1. Context-Aware Responses
- AI nh? l?i conversation history
- Contextual suggestions based on previous messages
- Session-specific preferences

### 2. Real-time Data Integration
- Tour availability realtime t? database
- Product stock levels
- Pricing updates
- Seasonal recommendations

### 3. Intelligent Fallback
- Khi Gemini API unavailable ? Fallback responses
- Cached responses ?? gi?m API calls
- Graceful degradation

### 4. Smart Filtering
```sql
-- Tour Chat Query Example
SELECT ts.*, tt.Title, tt.StartLocation 
FROM TourSlots ts
JOIN TourTemplates tt ON ts.TourTemplateId = tt.Id
WHERE ts.IsActive = 1 
  AND ts.Status = 'Available'
  AND ts.AvailableSpots > 0
  AND ts.TourDate >= CURDATE()
  AND tt.IsActive = 1
```

## Error Handling

### Chat Type Validation
```json
{
  "success": false,
  "message": "ChatType không h?p l?. Ch? ch?p nh?n: 1=Tour, 2=Product, 3=TayNinh",
  "statusCode": 400
}
```

### Session Not Found
```json
{
  "success": false,
  "message": "Không tìm th?y phiên chat",
  "statusCode": 404
}
```

### AI Service Unavailable
```json
{
  "success": true,
  "message": "G?i tin nh?n thành công (s? d?ng ph?n h?i t? ??ng)",
  "aiResponse": {
    "content": "Xin l?i, h? th?ng t? v?n tour hi?n ?ang g?p khó kh?n...",
    "isFallback": true
  }
}
```

## Usage Examples

### Tour Chat Session
```javascript
// 1. T?o session tour
const tourSession = await createChatSession({
  chatType: 1,
  firstMessage: "Có tour nào ?i Núi Bà ?en không?"
});

// 2. AI response s? include realtime tour data:
"Hi?n t?i chúng tôi có các tour Núi Bà ?en:
?? Tour Núi Bà ?en - Chùa Linh Thiêng - 200,000 VN?
   • T?: TP.HCM ? Núi Bà ?en
   • Ch? tr?ng: 8/20
   • Lo?i: Danh lam th?ng c?nh (mi?n phí vé vào c?a)"
```

### Product Chat Session
```javascript
// 1. T?o session s?n ph?m
const productSession = await createChatSession({
  chatType: 2,
  firstMessage: "Tôi mu?n mua bánh tráng làm quà"
});

// 2. AI response v?i product recommendations:
"?? Bánh Tráng Tr?ng Bàng Premium - 25,000 VN?
   • ?? ?ANG SALE 20%!
   • T?n kho: 50
   • Shop: ??c S?n Tây Ninh
   • Rating: 4.8? (124 ?ánh giá)"
```

### TayNinh Chat Session
```javascript
// 1. Valid question
"Núi Bà ?en có gì ??c bi?t?" 
? "Núi Bà ?en cao nh?t Nam B? (986m), có cáp treo và chùa linh thiêng..."

// 2. Invalid question
"Th?i ti?t Hà N?i hôm nay th? nào?"
? "Tôi ch? chuyên t? v?n v? Tây Ninh. B?n có câu h?i nào v? l?ch s?, v?n hóa, ??a ?i?m hay ?m th?c Tây Ninh không?"
```

## Performance Considerations

### Caching Strategy
- Response caching ?? gi?m Gemini API calls
- Database query result caching
- Session-level context caching

### Database Optimization
- Indexed queries cho tour/product lookup
- Efficient JOIN operations
- Pagination support

### Rate Limiting
- Gemini API quota management
- Per-user message rate limiting
- Fallback mechanisms

## Migration Notes
Existing chat sessions s? ???c:
- **Migration script**: Set ChatType = TayNinh (default)
- **Backward compatibility**: Old APIs continue working
- **Gradual transition**: Clients can update to use new ChatType parameter

## Security
- JWT Authentication required
- User-specific session isolation
- Input validation và sanitization
- SQL injection prevention
- Rate limiting protection

---

**Version**: 1.0  
**Last Updated**: December 2024  
**Author**: AI Development Team