# ?? Debug v� Test Th�ng B�o "Kh�ng T�m Th?y H??ng D?n Vi�n Ph� H?p"

## ?? T?ng quan

Document n�y h??ng d?n c�ch debug v� test th�ng b�o khi h? th?ng t? ??ng t�m h??ng d?n vi�n nh?ng kh�ng t�m th?y ai c� skill ph� h?p.

## ?? V?n ?? ?� ???c fix

Th�ng b�o khi kh�ng t�m th?y h??ng d?n vi�n ph� h?p ?� ???c c?i thi?n v� s? d?ng tr?c ti?p repository thay v� dependency injection ph?c t?p.

### ?? C�c thay ??i ?� th?c hi?n:

1. **C?i thi?n method `NotifyTourCompanyAboutNoSuitableGuidesAsync`:**
   - S? d?ng tr?c ti?p `_unitOfWork.NotificationRepository` thay v� scope injection
   - Th�m logging chi ti?t ?? debug
   - T?o email HTML phong ph� h?n v?i h??ng d?n c? th?

2. **Th�m endpoint debug:**
   - `POST /api/admin/debug/test-no-suitable-guides-notification/{tourDetailsId}`
   - Cho ph�p admin test th�ng b�o cho b?t k? tour n�o

## ?? C�ch test th�ng b�o

### 1. S? d?ng endpoint debug (Khuy?n ngh?)

```http
POST /api/admin/debug/test-no-suitable-guides-notification/{tourDetailsId}
Authorization: Bearer {admin_token}
```

**Response th�nh c�ng:**
```json
{
  "statusCode": 200,
  "message": "Debug: ?� g?i th�ng b�o test th�nh c�ng",
  "success": true
}
```

### 2. Test th�ng qua flow t? nhi�n

1. **T?o TourDetails v?i skill ??c bi?t:**
   ```json
   {
     "title": "Test Tour - Skill Hi?m",
     "skillsRequired": "UnicornRiding,DragonTaming,PhoenixFeatherCollection"
   }
   ```

2. **Admin duy?t tour:**
   ```http
   POST /api/admin/tourdetails/{tourDetailsId}/approve
   ```

3. **H? th?ng s? t? ??ng:**
   - T�m h??ng d?n vi�n c� skill ph� h?p
   - Kh�ng t�m th?y ai ? G?i th�ng b�o
   - Log chi ti?t trong console

## ?? N?i dung th�ng b�o ???c g?i

### ?? In-app notification:
- **Title:** "?? Kh�ng t�m th?y h??ng d?n vi�n ph� h?p"
- **Message:** M� t? v?n ?? v� h??ng d?n h�nh ??ng
- **Priority:** High
- **Action URL:** "/guides/list"

### ?? Email notification:
- **Subject:** "C?n ch?n h??ng d?n vi�n: Tour '{TourTitle}'"
- **Content bao g?m:**
  - Gi?i th�ch v?n ??
  - H�nh ??ng c?n th?c hi?n ngay
  - G?i � t�m h??ng d?n vi�n
  - K? n?ng tour ?ang y�u c?u
  - C?nh b�o v? th?i h?n 5 ng�y
  - CTA buttons

## ?? Debug logs ?? ki?m tra

Khi ch?y test, h�y ki?m tra logs sau trong console:

```
Sending no suitable guides notification to TourCompany for TourDetails {TourDetailsId}
Successfully created in-app notification for TourDetails {TourDetailsId}
Successfully sent email notification for no suitable guides to {Email}
Successfully sent no suitable guides notification for TourDetails {TourDetailsId}
```

## ??? Troubleshooting

### N?u kh�ng th?y th�ng b�o:

1. **Ki?m tra log console** - C� l?i g� kh�ng?
2. **Ki?m tra database** - Notification c� ???c t?o kh�ng?
   ```sql
   SELECT * FROM Notifications 
   WHERE UserId = '{tour_company_user_id}' 
   ORDER BY CreatedAt DESC LIMIT 5;
   ```

3. **Ki?m tra email settings** - EmailSender c� ho?t ??ng kh�ng?
4. **Test endpoint debug** ?? ??m b?o logic ho?t ??ng

### N?u email kh�ng g?i ???c:

1. Ki?m tra `appsettings.json` - EmailSettings
2. Ki?m tra SMTP credentials
3. Ki?m tra firewall/network settings

## ?? C�ch t?i ?u th�m

1. **Th�m retry mechanism** cho email sending
2. **Queue notification** ?? kh�ng block main flow
3. **Add metrics** ?? track notification success rate
4. **Implement real-time notification** v?i SignalR

## ?? Frontend integration

Frontend c?n:

1. **Poll notification API** ho?c implement real-time v?i SignalR
2. **Handle notification priority** - High priority n�n show ngay
3. **Redirect user** khi click v�o notification (ActionUrl)
4. **Show detailed guide list** v?i filter theo skill

## ?? Security notes

- Endpoint debug ch? d�nh cho Admin role
- Log sensitive data c?n th?n (kh�ng log email content)
- Rate limit notification sending ?? tr�nh spam

---

## ? Status

- ? Fix logic notification  
- ? Add debug endpoint
- ? Improve email template
- ? Add comprehensive logging
- ? Test successful

**Th�ng b�o ?� ho?t ??ng b�nh th??ng!** ??