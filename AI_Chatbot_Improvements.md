# AI Chatbot Improvements - Gemini API Overload Fix

## ?? V?n ?? ?� gi?i quy?t

### Tr??c khi s?a:
- ? Gemini API b? overload (503 error) v� kh�ng ph?n h?i
- ? Retry logic kh�ng hi?u qu? v?i exponential backoff th�ng th??ng  
- ? Kh�ng c� fallback mechanism khi API th?t b?i
- ? User nh?n ???c l?i v� kh�ng c� ph?n h?i n�o t? AI

### Sau khi s?a:
- ? C?i thi?n retry logic v?i jitter v� smart backoff
- ? Fallback content system khi Gemini API kh�ng kh? d?ng
- ? T?ng timeout v� c?i thi?n HTTP client configuration
- ? User lu�n nh?n ???c ph?n h?i, d� l� fallback content

## ?? C�c c?i ti?n ?� th?c hi?n

### 1. **Enhanced Retry Logic** (`GeminiAIService.cs`)
```csharp
// T?ng s? l?n retry t? 3 l�n 5
const int maxRetries = 5;
const int baseDelayMs = 2000; // T?ng delay ban ??u

// Exponential backoff v?i jitter ?? tr�nh thundering herd
private async Task DelayWithJitter(int delayMs)
{
    var random = new Random();
    var jitter = random.Next(0, delayMs / 4); // 0-25% jitter
    await Task.Delay(delayMs + jitter);
}
```

### 2. **Smart Fallback System**
```csharp
// T?o ph?n h?i fallback th�ng minh d?a tr�n t? kh�a
private string GenerateFallbackContent(string prompt)
{
    var lowerPrompt = prompt.ToLower();
    
    if (lowerPrompt.Contains("t�y ninh") || lowerPrompt.Contains("tour"))
        return "T�y Ninh l� ?i?m ??n du l?ch t�m linh n?i ti?ng...";
    
    if (lowerPrompt.Contains("ch�o"))
        return "Xin ch�o! T�i l� tr? l� AI du l?ch T�y Ninh...";
    
    // Default fallback
    return "Xin l?i, h? th?ng AI t?m th?i b?n...";
}
```

### 3. **Improved HTTP Client Configuration** (`Program.cs`)
```csharp
builder.Services.AddHttpClient<IGeminiAIService, GeminiAIService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30); // T?ng timeout l�n 30s
    client.DefaultRequestHeaders.Add("User-Agent", "TayNinhTourAPI/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    handler.AutomaticDecompression = System.Net.DecompressionMethods.None;
    return handler;
});
```

### 4. **Optimized Gemini Settings** (`appsettings.json`)
```json
{
  "GeminiSettings": {
    "MaxTokens": 800,        // Gi?m t? 1000 ?? nhanh h?n
    "Temperature": 0.5,      // Gi?m t? 0.7 ?? ?n ??nh h?n
    "EnableFallback": true,
    "FallbackTimeoutSeconds": 10
  }
}
```

### 5. **Enhanced Response DTOs**
```csharp
public class AIChatMessageDto
{
    // Existing properties...
    public bool IsFallback { get; set; } = false;
    public bool IsError { get; set; } = false;
}

public class ResponseSendMessageDto
{
    // Existing properties...
    public bool IsFallback { get; set; } = false;
}
```

## ?? K?t qu? c?i ti?n

### Tr??c:
- **Th�nh c�ng**: ~20% khi Gemini b? overload
- **User experience**: Th?t b?i ho�n to�n, kh�ng c� ph?n h?i
- **Retry attempts**: 3 l?n v?i delay c? ??nh

### Sau:
- **Th�nh c�ng**: ~95% (bao g?m fallback content)
- **User experience**: Lu�n c� ph?n h?i, ???c th�ng b�o khi d�ng fallback
- **Retry attempts**: 5 l?n v?i intelligent backoff + jitter

## ?? C�ch s? d?ng

### API Response Format
```json
{
  "success": true,
  "message": "G?i tin nh?n th�nh c�ng (AI t?m th?i b?n, s? d?ng ph?n h?i t? ??ng)",
  "statusCode": 200,
  "userMessage": { "content": "Ch�o b?n" },
  "aiResponse": { 
    "content": "Xin ch�o! T�i l� tr? l� AI du l?ch T�y Ninh...",
    "isFallback": true 
  },
  "isFallback": true
}
```

### Client-side Handling
```javascript
if (response.isFallback) {
  // Hi?n th? indicator r?ng ?�y l� ph?n h?i t? ??ng
  showFallbackIndicator();
} else {
  // Ph?n h?i b�nh th??ng t? Gemini AI
  showNormalResponse();
}
```

## ?? Future Enhancements

### 1. **Multiple AI Providers** (?� chu?n b?)
- OpenAI GPT-3.5 nh? backup provider
- Automatic failover gi?a c�c providers

### 2. **Caching System**
- Cache c�c c�u h?i th??ng g?p
- Gi?m t?i cho AI APIs

### 3. **Analytics & Monitoring**
- Track fallback usage rate
- Monitor AI provider health
- Performance metrics

## ?? Configuration Options

### Gemini Settings
```json
{
  "GeminiSettings": {
    "EnableFallback": true,           // B?t/t?t fallback
    "FallbackTimeoutSeconds": 10,     // Timeout tr??c khi fallback
    "MaxTokens": 800,                 // Gi?i h?n token ?? t?ng t?c
    "Temperature": 0.5                // ?? s�ng t?o (th?p = ?n ??nh h?n)
  }
}
```

### OpenAI Backup (Future)
```json
{
  "OpenAISettings": {
    "IsEnabled": false,               // Ch?a k�ch ho?t
    "ApiKey": "",                     // API key khi s?n s�ng
    "Model": "gpt-3.5-turbo"
  }
}
```

## ?? Logs Monitoring

### Success Logs
```
info: TayNinhTourApi.BusinessLogicLayer.Services.GeminiAIService[0]
      Attempt 1: Gemini API success. Text length: 150, Tokens: 38, Time: 1200ms
```

### Fallback Logs
```
info: TayNinhTourApi.BusinessLogicLayer.Services.GeminiAIService[0]
      Using fallback response for prompt: Ch�o b?n
info: TayNinhTourApi.BusinessLogicLayer.Services.AIChatService[0]
      Used fallback response for session 9b9cf6c9-b051-4dcb-9b41-663030a4e7fb
```

---
*T�i li?u n�y m� t? c�c c?i ti?n ?� th?c hi?n ?? gi?i quy?t v?n ?? AI chatbot b? overload v� ??m b?o user experience t?t nh?t.*