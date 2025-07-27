# ?? Debug và Test Thông Báo "Không Tìm Th?y H??ng D?n Viên Phù H?p"

## ?? T?ng quan

Document này h??ng d?n cách debug và test thông báo khi h? th?ng t? ??ng tìm h??ng d?n viên nh?ng không tìm th?y ai có skill phù h?p.

## ?? V?n ?? ?ã ???c fix

Thông báo khi không tìm th?y h??ng d?n viên phù h?p ?ã ???c c?i thi?n và s? d?ng tr?c ti?p repository thay vì dependency injection ph?c t?p.

### ?? Các thay ??i ?ã th?c hi?n:

1. **C?i thi?n method `NotifyTourCompanyAboutNoSuitableGuidesAsync`:**
   - S? d?ng tr?c ti?p `_unitOfWork.NotificationRepository` thay vì scope injection
   - Thêm logging chi ti?t ?? debug
   - T?o email HTML phong phú h?n v?i h??ng d?n c? th?

2. **Thêm endpoint debug:**
   - `POST /api/admin/debug/test-no-suitable-guides-notification/{tourDetailsId}`
   - Cho phép admin test thông báo cho b?t k? tour nào

## ?? Cách test thông báo

### 1. S? d?ng endpoint debug (Khuy?n ngh?)

```http
POST /api/admin/debug/test-no-suitable-guides-notification/{tourDetailsId}
Authorization: Bearer {admin_token}
```

**Response thành công:**
```json
{
  "statusCode": 200,
  "message": "Debug: ?ã g?i thông báo test thành công",
  "success": true
}
```

### 2. Test thông qua flow t? nhiên

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
   - Tìm h??ng d?n viên có skill phù h?p
   - Không tìm th?y ai ? G?i thông báo
   - Log chi ti?t trong console

## ?? N?i dung thông báo ???c g?i

### ?? In-app notification:
- **Title:** "?? Không tìm th?y h??ng d?n viên phù h?p"
- **Message:** Mô t? v?n ?? và h??ng d?n hành ??ng
- **Priority:** High
- **Action URL:** "/guides/list"

### ?? Email notification:
- **Subject:** "C?n ch?n h??ng d?n viên: Tour '{TourTitle}'"
- **Content bao g?m:**
  - Gi?i thích v?n ??
  - Hành ??ng c?n th?c hi?n ngay
  - G?i ý tìm h??ng d?n viên
  - K? n?ng tour ?ang yêu c?u
  - C?nh báo v? th?i h?n 5 ngày
  - CTA buttons

## ?? Debug logs ?? ki?m tra

Khi ch?y test, hãy ki?m tra logs sau trong console:

```
Sending no suitable guides notification to TourCompany for TourDetails {TourDetailsId}
Successfully created in-app notification for TourDetails {TourDetailsId}
Successfully sent email notification for no suitable guides to {Email}
Successfully sent no suitable guides notification for TourDetails {TourDetailsId}
```

## ??? Troubleshooting

### N?u không th?y thông báo:

1. **Ki?m tra log console** - Có l?i gì không?
2. **Ki?m tra database** - Notification có ???c t?o không?
   ```sql
   SELECT * FROM Notifications 
   WHERE UserId = '{tour_company_user_id}' 
   ORDER BY CreatedAt DESC LIMIT 5;
   ```

3. **Ki?m tra email settings** - EmailSender có ho?t ??ng không?
4. **Test endpoint debug** ?? ??m b?o logic ho?t ??ng

### N?u email không g?i ???c:

1. Ki?m tra `appsettings.json` - EmailSettings
2. Ki?m tra SMTP credentials
3. Ki?m tra firewall/network settings

## ?? Cách t?i ?u thêm

1. **Thêm retry mechanism** cho email sending
2. **Queue notification** ?? không block main flow
3. **Add metrics** ?? track notification success rate
4. **Implement real-time notification** v?i SignalR

## ?? Frontend integration

Frontend c?n:

1. **Poll notification API** ho?c implement real-time v?i SignalR
2. **Handle notification priority** - High priority nên show ngay
3. **Redirect user** khi click vào notification (ActionUrl)
4. **Show detailed guide list** v?i filter theo skill

## ?? Security notes

- Endpoint debug ch? dành cho Admin role
- Log sensitive data c?n th?n (không log email content)
- Rate limit notification sending ?? tránh spam

---

## ? Status

- ? Fix logic notification  
- ? Add debug endpoint
- ? Improve email template
- ? Add comprehensive logging
- ? Test successful

**Thông báo ?ã ho?t ??ng bình th??ng!** ??