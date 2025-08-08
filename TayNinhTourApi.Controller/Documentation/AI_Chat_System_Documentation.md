# H? th?ng AI Chat Chuy�n Bi?t - API Documentation

## T?ng Quan
H? th?ng AI Chat ?� ???c n�ng c?p ?? h? tr? **3 lo?i chat chuy�n bi?t**:
1. **Tour Chat** - T? v?n v? tours, gi� c?, ??t ch?
2. **Product Chat** - T? v?n mua s?m s?n ph?m ??c s?n
3. **TayNinh Chat** - Th�ng tin v? l?ch s?, v?n h�a, ??a ?i?m T�y Ninh

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
- **AIChatSession**: Phi�n chat v?i ChatType enum
- **AIChatMessage**: Tin nh?n v?i metadata
- **AIChatType**: Enum (Tour=1, Product=2, TayNinh=3)

## API Endpoints

### 1. T?o Phi�n Chat M?i
```http
POST /api/AiChat/sessions
Authorization: Bearer {token}
Content-Type: application/json

{
  "chatType": 1,  // 1=Tour, 2=Product, 3=TayNinh
  "firstMessage": "T�i mu?n t�m tour N�i B� ?en",
  "customTitle": "T? v?n tour N�i B� ?en" // Optional
}
```

**Response:**
```json
{
  "success": true,
  "message": "T?o phi�n chat th�nh c�ng",
  "statusCode": 201,
  "chatSession": {
    "id": "guid",
    "title": "[Tour] T? v?n tour N�i B� ?en",
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
  "message": "Tour n�o c� gi� r? nh?t?",
  "includeContext": true,
  "contextMessageCount": 10
}
```

### 3. L?y Danh S�ch Phi�n Chat (C� l?c theo lo?i)
```http
GET /api/AiChat/sessions?chatType=1&page=0&pageSize=20
Authorization: Bearer {token}
```

### 4. L?y Tin Nh?n Trong Phi�n
```http
GET /api/AiChat/sessions/{sessionId}/messages
Authorization: Bearer {token}
```

### 5. Th?ng K� Ph�n Lo?i
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
- T? v?n tours c� s?n t? database
- L?c tours theo status = PUBLIC v� c� slot tr?ng
- Th�ng tin gi� c?, l?ch tr�nh, ?i?m ??n
- G?i � tours theo ng�n s�ch

**System Prompt:**
```
B?n l� AI t? v?n tour du l?ch T�y Ninh chuy�n nghi?p.
- T? v?n tours, gi� c?, l?ch tr�nh, ??t ch?
- Ch? gi?i thi?u tours c� status PUBLIC v� c� slot tr?ng
- Kh�ng ??a ra th�ng tin sai l?ch v? tours
```

**Data Integration:**
- Truy c?p `TourSlot`, `TourTemplate`, `TourCompany`
- Filter theo `IsActive`, `Status = Available`, `AvailableSpots > 0`
- Hi?n th? tours FreeScenic vs PaidAttraction

### 2. Product Chat (Type = 2)
**Ch?c n?ng:**
- T? v?n s?n ph?m theo nhu c?u v� ng�n s�ch
- G?i � s?n ph?m trending, sale, best-selling
- So s�nh gi� v� ch?t l??ng
- Th�ng tin shop, rating, reviews

**System Prompt:**
```
B?n l� AI t? v?n mua s?m s?n ph?m ??c s?n T�y Ninh.
- T? v?n s?n ph?m theo nhu c?u v� ng�n s�ch
- Ch? g?i � s?n ph?m c�n h�ng (QuantityInStock > 0)
- ?u ti�n s?n ph?m c� rating cao
```

**Data Integration:**
- Truy c?p `Product`, `SpecialtyShop`, `ProductRating`
- Filter theo `IsActive`, `QuantityInStock > 0`
- Categories: Food, Souvenir, Jewelry, Clothing

### 3. TayNinh Chat (Type = 3)
**Ch?c n?ng:**
- Th�ng tin l?ch s?, v?n h�a, ??a l� T�y Ninh
- Gi?i thi?u c�c ??a ?i?m du l?ch
- ?m th?c ??c s?n v�ng ??t n�y
- **GI?I H?N**: Ch? tr? l?i c�u h?i v? T�y Ninh

**System Prompt:**
```
B?n l� AI chuy�n gia v? T�y Ninh - l?ch s?, v?n h�a, ??a ?i?m, ?m th?c.
- CH?N tr? l?i c�u h?i KH�NG LI�N QUAN ??n T�y Ninh
- N?u h?i v? ch? ?? kh�c: 'T�i ch? chuy�n t? v?n v? T�y Ninh...'
```

**No Data Integration:** Ch? d?a tr�n AI knowledge

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
  "message": "ChatType kh�ng h?p l?. Ch? ch?p nh?n: 1=Tour, 2=Product, 3=TayNinh",
  "statusCode": 400
}
```

### Session Not Found
```json
{
  "success": false,
  "message": "Kh�ng t�m th?y phi�n chat",
  "statusCode": 404
}
```

### AI Service Unavailable
```json
{
  "success": true,
  "message": "G?i tin nh?n th�nh c�ng (s? d?ng ph?n h?i t? ??ng)",
  "aiResponse": {
    "content": "Xin l?i, h? th?ng t? v?n tour hi?n ?ang g?p kh� kh?n...",
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
  firstMessage: "C� tour n�o ?i N�i B� ?en kh�ng?"
});

// 2. AI response s? include realtime tour data:
"Hi?n t?i ch�ng t�i c� c�c tour N�i B� ?en:
?? Tour N�i B� ?en - Ch�a Linh Thi�ng - 200,000 VN?
   � T?: TP.HCM ? N�i B� ?en
   � Ch? tr?ng: 8/20
   � Lo?i: Danh lam th?ng c?nh (mi?n ph� v� v�o c?a)"
```

### Product Chat Session
```javascript
// 1. T?o session s?n ph?m
const productSession = await createChatSession({
  chatType: 2,
  firstMessage: "T�i mu?n mua b�nh tr�ng l�m qu�"
});

// 2. AI response v?i product recommendations:
"?? B�nh Tr�ng Tr?ng B�ng Premium - 25,000 VN?
   � ?? ?ANG SALE 20%!
   � T?n kho: 50
   � Shop: ??c S?n T�y Ninh
   � Rating: 4.8? (124 ?�nh gi�)"
```

### TayNinh Chat Session
```javascript
// 1. Valid question
"N�i B� ?en c� g� ??c bi?t?" 
? "N�i B� ?en cao nh?t Nam B? (986m), c� c�p treo v� ch�a linh thi�ng..."

// 2. Invalid question
"Th?i ti?t H� N?i h�m nay th? n�o?"
? "T�i ch? chuy�n t? v?n v? T�y Ninh. B?n c� c�u h?i n�o v? l?ch s?, v?n h�a, ??a ?i?m hay ?m th?c T�y Ninh kh�ng?"
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
- Input validation v� sanitization
- SQL injection prevention
- Rate limiting protection

---

**Version**: 1.0  
**Last Updated**: December 2024  
**Author**: AI Development Team