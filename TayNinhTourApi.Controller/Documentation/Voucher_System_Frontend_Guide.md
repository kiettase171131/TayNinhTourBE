# ?? H? Th?ng Voucher - H??ng D?n Frontend Integration

## ?? M?c l?c
1. [T?ng quan h? th?ng](#t?ng-quan-h?-th?ng)
2. [Lu?ng ho?t ??ng](#lu?ng-ho?t-??ng)
3. [Chi ti?t v?n hành h? th?ng](#chi-ti?t-v?n-hành-h?-th?ng)
4. [API Endpoints](#api-endpoints)
5. [Models & DTOs](#models--dtos)
6. [Frontend Implementation](#frontend-implementation)
7. [UI/UX Guidelines](#uiux-guidelines)
8. [Testing & Examples](#testing--examples)
9. [Error Handling](#error-handling)
10. [Performance & Optimization](#performance--optimization)
11. [Security Considerations](#security-considerations)

---

## ?? T?ng quan h? th?ng

H? th?ng voucher ?ã ???c **??n gi?n hóa** t? mô hình ph?c t?p v?i VoucherCode sang mô hình tr?c ti?p. Thay vì ph?i claim voucher code tr??c khi s? d?ng, ng??i dùng gi? có th? áp d?ng voucher tr?c ti?p khi checkout.

### ??c ?i?m chính:
- ? **Không c?n claim voucher** - Áp d?ng tr?c ti?p b?ng VoucherId
- ? **H? th?ng ??n gi?n** - Qu?n lý b?ng Quantity & UsedCount
- ? **T? ??ng thông báo** - G?i thông báo ??n t?t c? user khi có voucher m?i
- ? **Validation thông minh** - Không cho dùng voucher trên s?n ph?m ?ang sale
- ? **Real-time tracking** - Theo dõi usage count theo th?i gian th?c
- ? **Auto-expire** - T? ??ng vô hi?u hóa voucher h?t h?n

---

## ?? Lu?ng ho?t ??ng

### 1. Admin t?o vouchergraph TD
    A[Admin t?o voucher] --> B[Validate input data]
    B --> C[L?u vào database]
    C --> D[T?o notification cho all users]
    D --> E[G?i push notification]
    E --> F[Log audit trail]
    F --> G[Return success response]
### 2. User s? d?ng vouchergraph TD
    A[User xem voucher kh? d?ng] --> B[Load t? API]
    B --> C[Filter expired/used vouchers]
    C --> D[Display available vouchers]
    D --> E[User ch?n voucher]
    E --> F[Preview discount calculation]
    F --> G[User confirm checkout]
    G --> H{Validate voucher}
    H -->|Valid| I[Apply discount]
    H -->|Invalid| J[Show error message]
    I --> K[Create PayOS payment]
    K --> L[User thanh toán]
    L --> M{Payment success?}
    M -->|Yes| N[Update voucher UsedCount]
    M -->|No| O[Rollback voucher state]
    N --> P[Clear cart items]
    P --> Q[Send confirmation]
### 3. Background processesgraph TD
    A[Scheduled Jobs] --> B[Check expired vouchers]
    B --> C[Deactivate expired vouchers]
    C --> D[Cleanup unused notifications]
    D --> E[Generate usage reports]
    E --> F[Send admin analytics]
---

## ?? Chi ti?t v?n hành h? th?ng

### ?? **1. Qu?n lý Voucher Lifecycle**

#### **1.1 T?o Voucher**// Quy trình t?o voucher hoàn ch?nh
const createVoucherProcess = async (voucherData: CreateVoucherDto) => {
    // Step 1: Validate business rules
    validateVoucherRules(voucherData);
    
    // Step 2: Create voucher entity
    const voucher = await createVoucherEntity(voucherData);
    
    // Step 3: Generate notification content
    const notification = generateVoucherNotification(voucher);
    
    // Step 4: Send to all active users
    await broadcastNotificationToUsers(notification);
    
    // Step 5: Log admin action
    await logAdminAction('CREATE_VOUCHER', voucher.id);
    
    // Step 6: Schedule expiry job
    await scheduleVoucherExpiryCheck(voucher.id, voucher.endDate);
    
    return voucher;
};
#### **1.2 Voucher State Management**enum VoucherState {
    DRAFT = 'draft',           // Ch?a activate
    ACTIVE = 'active',         // ?ang ho?t ??ng
    PAUSED = 'paused',         // T?m d?ng
    EXPIRED = 'expired',       // H?t h?n
    EXHAUSTED = 'exhausted',   // H?t l??t s? d?ng
    CANCELLED = 'cancelled'    // ?ã h?y
}

const getVoucherState = (voucher: VoucherDto): VoucherState => {
    if (!voucher.isActive) return VoucherState.PAUSED;
    if (new Date() > new Date(voucher.endDate)) return VoucherState.EXPIRED;
    if (voucher.remainingCount <= 0) return VoucherState.EXHAUSTED;
    return VoucherState.ACTIVE;
};
### ?? **2. Real-time Usage Tracking**

#### **2.1 Concurrent Usage Handling**// X? lý concurrent voucher usage
const atomicVoucherUsage = async (voucherId: string, orderId: string) => {
    const transaction = await beginTransaction();
    
    try {
        // Lock voucher record for update
        const voucher = await lockVoucherForUpdate(voucherId, transaction);
        
        // Check availability again after lock
        if (voucher.remainingCount <= 0) {
            throw new Error('Voucher ?ã h?t l??t s? d?ng');
        }
        
        // Update usage count
        await updateVoucherUsage(voucherId, transaction);
        
        // Link voucher to order
        await linkVoucherToOrder(voucherId, orderId, transaction);
        
        await commitTransaction(transaction);
        
        // Notify about usage update
        await notifyVoucherUsageUpdate(voucherId);
        
    } catch (error) {
        await rollbackTransaction(transaction);
        throw error;
    }
};
#### **2.2 Usage Analytics**interface VoucherAnalytics {
    totalVouchers: number;
    activeVouchers: number;
    totalUsage: number;
    usageToday: number;
    topVouchers: Array<{
        id: string;
        name: string;
        usageCount: number;
        conversionRate: number;
    }>;
    usageByHour: Array<{
        hour: number;
        count: number;
    }>;
}

const generateVoucherAnalytics = async (): Promise<VoucherAnalytics> => {
    // Implementation for comprehensive analytics
};
### ?? **3. Business Rules & Validations**

#### **3.1 Voucher Creation Rules**const validateVoucherRules = (data: CreateVoucherDto): ValidationResult => {
    const errors: string[] = [];
    
    // Rule 1: Start date must be <= End date
    if (new Date(data.startDate) >= new Date(data.endDate)) {
        errors.push('Ngày b?t ??u ph?i nh? h?n ngày k?t thúc');
    }
    
    // Rule 2: Must have either discount amount OR percentage
    if (data.discountAmount <= 0 && (!data.discountPercent || data.discountPercent <= 0)) {
        errors.push('Ph?i có s? ti?n gi?m ho?c ph?n tr?m gi?m');
    }
    
    // Rule 3: Cannot have both discount types
    if (data.discountAmount > 0 && data.discountPercent && data.discountPercent > 0) {
        errors.push('Ch? ???c ch?n m?t lo?i gi?m giá');
    }
    
    // Rule 4: Reasonable quantity limits
    if (data.quantity > 10000) {
        errors.push('S? l??ng voucher không ???c v??t quá 10,000');
    }
    
    // Rule 5: Reasonable discount limits
    if (data.discountPercent && data.discountPercent > 100) {
        errors.push('Ph?n tr?m gi?m không ???c v??t quá 100%');
    }
    
    return { isValid: errors.length === 0, errors };
};
#### **3.2 Checkout Validation Rules**const validateVoucherForCheckout = async (
    voucherId: string, 
    cartItems: CartItem[]
): Promise<ValidationResult> => {
    const voucher = await getVoucherById(voucherId);
    const errors: string[] = [];
    
    // Rule 1: Voucher exists and is active
    if (!voucher || !voucher.isActive) {
        errors.push('Voucher không t?n t?i ho?c không ho?t ??ng');
    }
    
    // Rule 2: Within validity period
    const now = new Date();
    if (now < new Date(voucher.startDate) || now > new Date(voucher.endDate)) {
        errors.push('Voucher ngoài th?i gian s? d?ng');
    }
    
    // Rule 3: Has remaining uses
    if (voucher.remainingCount <= 0) {
        errors.push('Voucher ?ã h?t l??t s? d?ng');
    }
    
    // Rule 4: No products on sale in cart
    const productsOnSale = cartItems.filter(item => item.isSale);
    if (productsOnSale.length > 0) {
        errors.push('Không th? s? d?ng voucher cho s?n ph?m ?ang sale');
    }
    
    return { isValid: errors.length === 0, errors };
};
### ?? **4. Notification System**

#### **4.1 Notification Types**enum VoucherNotificationType {
    NEW_VOUCHER = 'new_voucher',
    VOUCHER_UPDATED = 'voucher_updated',
    VOUCHER_EXPIRING = 'voucher_expiring',
    VOUCHER_USED = 'voucher_used',
    VOUCHER_EXPIRED = 'voucher_expired'
}

interface VoucherNotification {
    type: VoucherNotificationType;
    voucherId: string;
    title: string;
    message: string;
    actionUrl: string;
    metadata: Record<string, any>;
}
#### **4.2 Notification Broadcasting**const broadcastVoucherNotification = async (
    notification: VoucherNotification,
    targetUsers?: string[]
) => {
    // Get target users (all active users if not specified)
    const users = targetUsers || await getAllActiveUsers();
    
    // Create notification records
    const notifications = users.map(userId => ({
        userId,
        type: notification.type,
        title: notification.title,
        message: notification.message,
        actionUrl: notification.actionUrl,
        metadata: notification.metadata,
        createdAt: new Date()
    }));
    
    // Batch insert notifications
    await batchCreateNotifications(notifications);
    
    // Send real-time notifications (WebSocket/SignalR)
    await sendRealTimeNotifications(notifications);
    
    // Optional: Send push notifications to mobile
    if (shouldSendPushNotification(notification.type)) {
        await sendPushNotifications(notifications);
    }
};
### ? **5. Performance Optimizations**

#### **5.1 Caching Strategy**interface VoucherCache {
    // Cache available vouchers for users
    availableVouchers: Map<string, AvailableVoucherDto[]>; // key: userId
    
    // Cache voucher details
    voucherDetails: Map<string, VoucherDto>; // key: voucherId
    
    // Cache validation results
    validationResults: Map<string, ValidationResult>; // key: voucherId_userId
}

const cacheManager = {
    // Cache available vouchers for 5 minutes
    async getAvailableVouchers(userId: string): Promise<AvailableVoucherDto[]> {
        const cacheKey = `available_vouchers_${userId}`;
        let vouchers = await redis.get(cacheKey);
        
        if (!vouchers) {
            vouchers = await fetchAvailableVouchersFromDB(userId);
            await redis.setex(cacheKey, 300, JSON.stringify(vouchers)); // 5 min TTL
        }
        
        return JSON.parse(vouchers);
    },
    
    // Invalidate cache when voucher is used
    async invalidateVoucherCache(voucherId: string): Promise<void> {
        const keys = await redis.keys(`*voucher*${voucherId}*`);
        if (keys.length > 0) {
            await redis.del(...keys);
        }
    }
};
#### **5.2 Database Optimization**-- Indexes for voucher queries
CREATE INDEX idx_vouchers_active_dates ON Vouchers(IsActive, StartDate, EndDate) WHERE IsDeleted = 0;
CREATE INDEX idx_vouchers_remaining_count ON Vouchers(Quantity, UsedCount) WHERE IsActive = 1;
CREATE INDEX idx_orders_voucher_status ON Orders(VoucherId, Status) WHERE VoucherId IS NOT NULL;

-- Composite index for efficient filtering
CREATE INDEX idx_vouchers_composite ON Vouchers(IsActive, StartDate, EndDate, Quantity, UsedCount) 
INCLUDE (Id, Name, DiscountAmount, DiscountPercent)
WHERE IsDeleted = 0;
### ?? **6. Security Measures**

#### **6.1 Rate Limiting**const voucherRateLimiter = {
    // Limit voucher usage attempts
    checkUsageRateLimit: async (userId: string): Promise<boolean> => {
        const key = `voucher_usage_${userId}`;
        const current = await redis.get(key) || 0;
        
        if (parseInt(current) >= 5) { // Max 5 attempts per minute
            return false;
        }
        
        await redis.incr(key);
        await redis.expire(key, 60); // 1 minute TTL
        return true;
    },
    
    // Limit voucher creation for admin
    checkCreationRateLimit: async (adminId: string): Promise<boolean> => {
        const key = `voucher_creation_${adminId}`;
        const current = await redis.get(key) || 0;
        
        if (parseInt(current) >= 10) { // Max 10 vouchers per hour
            return false;
        }
        
        await redis.incr(key);
        await redis.expire(key, 3600); // 1 hour TTL
        return true;
    }
};
#### **6.2 Fraud Detection**interface FraudDetectionRule {
    name: string;
    check: (usage: VoucherUsage) => Promise<boolean>;
    action: 'log' | 'block' | 'alert';
}

const fraudDetectionRules: FraudDetectionRule[] = [
    {
        name: 'Multiple rapid usage',
        check: async (usage) => {
            const recentUsage = await getRecentVoucherUsage(usage.userId, 300); // 5 minutes
            return recentUsage.length > 3;
        },
        action: 'block'
    },
    {
        name: 'Suspicious discount amount',
        check: async (usage) => {
            return usage.discountAmount > usage.orderTotal; // Discount exceeds total
        },
        action: 'alert'
    },
    {
        name: 'Unusual usage pattern',
        check: async (usage) => {
            const userHistory = await getUserVoucherHistory(usage.userId);
            return detectAnomalousPattern(userHistory);
        },
        action: 'log'
    }
];
### ?? **7. Monitoring & Alerting**

#### **7.1 Health Checks**interface VoucherSystemHealth {
    totalVouchers: number;
    activeVouchers: number;
    usageRate: number;
    errorRate: number;
    avgResponseTime: number;
    cacheHitRate: number;
}

const healthCheck = async (): Promise<VoucherSystemHealth> => {
    const [total, active, usage, errors, responseTime, cacheHits] = await Promise.all([
        getTotalVouchersCount(),
        getActiveVouchersCount(),
        getUsageRate(),
        getErrorRate(),
        getAvgResponseTime(),
        getCacheHitRate()
    ]);
    
    return {
        totalVouchers: total,
        activeVouchers: active,
        usageRate: usage,
        errorRate: errors,
        avgResponseTime: responseTime,
        cacheHitRate: cacheHits
    };
};
#### **7.2 Business Metrics**interface VoucherBusinessMetrics {
    dailyUsage: number;
    conversionRate: number; // voucher views to usage
    averageDiscount: number;
    popularVouchers: VoucherDto[];
    userEngagement: number;
    revenueImpact: number;
}

const generateBusinessMetrics = async (
    startDate: Date, 
    endDate: Date
): Promise<VoucherBusinessMetrics> => {
    // Implementation for business metrics calculation
};
### ?? **8. Deployment & DevOps**

#### **8.1 Environment Configuration**interface VoucherConfig {
    maxVouchersPerUser: number;
    maxUsagePerDay: number;
    cacheExpiryTime: number;
    notificationBatchSize: number;
    analyticsRetentionDays: number;
}

const config = {
    development: {
        maxVouchersPerUser: 10,
        maxUsagePerDay: 5,
        cacheExpiryTime: 60, // 1 minute for testing
        notificationBatchSize: 10,
        analyticsRetentionDays: 30
    },
    production: {
        maxVouchersPerUser: 100,
        maxUsagePerDay: 20,
        cacheExpiryTime: 300, // 5 minutes
        notificationBatchSize: 1000,
        analyticsRetentionDays: 365
    }
};
#### **8.2 Backup & Recovery**const backupVoucherData = async (): Promise<void> => {
    // Daily backup of voucher data
    const vouchers = await exportAllVouchers();
    const orders = await exportVoucherOrders();
    const usage = await exportUsageHistory();
    
    const backup = {
        timestamp: new Date(),
        vouchers,
        orders,
        usage
    };
    
    await saveToBackupStorage(backup);
};

const restoreVoucherData = async (backupId: string): Promise<void> => {
    const backup = await loadFromBackupStorage(backupId);
    
    await restoreVouchers(backup.vouchers);
    await restoreOrders(backup.orders);
    await restoreUsageHistory(backup.usage);
};
---

## ?? API Endpoints

### Base URL: `https://localhost:7205/api/Product`

### ?? **Admin Endpoints**

#### 1. T?o voucher m?iPOST /api/Product/Create-Voucher
Authorization: Bearer {admin_token}
Content-Type: application/json
**Request Body:**{
    "name": "Flash Sale Cu?i Tu?n",
    "description": "Gi?m giá s?n ph?m cu?i tu?n",
    "quantity": 100,
    "discountAmount": 50000,
    "discountPercent": null,
    "startDate": "2024-01-15T00:00:00Z",
    "endDate": "2024-01-21T23:59:59Z"
}
**Response:**
{
    "statusCode": 200,
    "message": "Voucher ?ã ???c t?o thành công và thông báo ?ã ???c g?i ??n t?t c? ng??i dùng",
    "success": true,
    "voucherId": "550e8400-e29b-41d4-a716-446655440000",
    "voucherName": "Flash Sale Cu?i Tu?n",
    "quantity": 100
}
#### 2. Xem t?t c? voucher (Admin)GET /api/Product/GetAll-Voucher?pageIndex=1&pageSize=10&textSearch=&status=true
Authorization: Bearer {admin_token}
**Response:**
{
    "statusCode": 200,
    "message": "L?y danh sách voucher thành công",
    "success": true,
    "data": [
        {
            "id": "550e8400-e29b-41d4-a716-446655440000",
            "name": "Flash Sale Cu?i Tu?n",
            "description": "Gi?m giá s?n ph?m cu?i tu?n",
            "quantity": 100,
            "usedCount": 15,
            "remainingCount": 85,
            "discountAmount": 50000,
            "discountPercent": null,
            "startDate": "2024-01-15T00:00:00Z",
            "endDate": "2024-01-21T23:59:59Z",
            "isActive": true,
            "isExpired": false,
            "isAvailable": true,
            "createdAt": "2024-01-14T10:00:00Z"
        }
    ],
    "totalRecord": 1,
    "totalPages": 1
}
#### 3. C?p nh?t voucherPUT /api/Product/Update-Voucher/{voucherId}
Authorization: Bearer {admin_token}
Content-Type: application/json
#### 4. Xóa voucherDELETE /api/Product/Voucher/{voucherId}
Authorization: Bearer {admin_token}
### ?? **User Endpoints**

#### 1. Xem voucher kh? d?ngGET /api/Product/GetAvailable-Vouchers?pageIndex=1&pageSize=10
Authorization: Bearer {user_token}
**Response:**
{
    "statusCode": 200,
    "message": "L?y danh sách voucher kh? d?ng thành công",
    "success": true,
    "data": [
        {
            "id": "550e8400-e29b-41d4-a716-446655440000",
            "name": "Flash Sale Cu?i Tu?n",
            "description": "Gi?m giá s?n ph?m cu?i tu?n",
            "discountAmount": 50000,
            "discountPercent": null,
            "remainingCount": 85,
            "startDate": "2024-01-15T00:00:00Z",
            "endDate": "2024-01-21T23:59:59Z",
            "isExpiringSoon": false
        }
    ],
    "totalRecord": 1,
    "totalPages": 1
}
#### 2. Áp d?ng voucher khi checkoutPOST /api/Product/checkout
Authorization: Bearer {user_token}
Content-Type: application/json
**Request Body:**{
    "cartItemIds": [
        "123e4567-e89b-12d3-a456-426614174001",
        "123e4567-e89b-12d3-a456-426614174002"
    ],
    "voucherId": "550e8400-e29b-41d4-a716-446655440000"
}
**Response:**
{
    "checkoutUrl": "https://checkout.payos.vn/web/...",
    "orderId": "789e0123-e45f-67g8-h901-234567890123",
    "totalOriginal": 60000,
    "discountAmount": 50000,
    "totalAfterDiscount": 10000
}
---

## ?? Models & DTOs

### VoucherDtointerface VoucherDto {
    id: string;
    name: string;
    description?: string;
    quantity: number;
    usedCount: number;
    remainingCount: number;
    discountAmount: number;
    discountPercent?: number;
    startDate: string;
    endDate: string;
    isActive: boolean;
    isExpired: boolean;
    isAvailable: boolean;
    createdAt: string;
}
### AvailableVoucherDtointerface AvailableVoucherDto {
    id: string;
    name: string;
    description?: string;
    discountAmount: number;
    discountPercent?: number;
    remainingCount: number;
    startDate: string;
    endDate: string;
    isExpiringSoon: boolean;
}
### CreateVoucherDtointerface CreateVoucherDto {
    name: string;
    description?: string;
    quantity: number;
    discountAmount: number;
    discountPercent?: number;
    startDate: string;
    endDate: string;
}
### CheckoutDtointerface CheckoutSelectedCartItemsDto {
    cartItemIds: string[];
    voucherId?: string;
}
---

## ?? Frontend Implementation

### 1. Voucher Serviceclass VoucherService {
    private apiUrl = 'https://localhost:7205/api/Product';
    
    // L?y danh sách voucher kh? d?ng cho user
    async getAvailableVouchers(page = 1, pageSize = 10): Promise<AvailableVoucherDto[]> {
        const response = await fetch(
            `${this.apiUrl}/GetAvailable-Vouchers?pageIndex=${page}&pageSize=${pageSize}`,
            {
                headers: {
                    'Authorization': `Bearer ${getToken()}`,
                    'Content-Type': 'application/json'
                }
            }
        );
        
        const result = await response.json();
        return result.success ? result.data : [];
    }
    
    // Admin: T?o voucher m?i
    async createVoucher(voucher: CreateVoucherDto): Promise<any> {
        const response = await fetch(`${this.apiUrl}/Create-Voucher`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${getAdminToken()}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(voucher)
        });
        
        return await response.json();
    }
    
    // Admin: L?y t?t c? voucher
    async getAllVouchers(page = 1, pageSize = 10, search = '', status?: boolean): Promise<VoucherDto[]> {
        const params = new URLSearchParams({
            pageIndex: page.toString(),
            pageSize: pageSize.toString(),
            textSearch: search,
            ...(status !== undefined && { status: status.toString() })
        });
        
        const response = await fetch(`${this.apiUrl}/GetAll-Voucher?${params}`, {
            headers: {
                'Authorization': `Bearer ${getAdminToken()}`,
                'Content-Type': 'application/json'
            }
        });
        
        const result = await response.json();
        return result.success ? result.data : [];
    }
}
### 2. Checkout v?i Voucherclass CheckoutService {
    private apiUrl = 'https://localhost:7205/api/Product';
    
    async checkoutWithVoucher(cartItemIds: string[], voucherId?: string): Promise<any> {
        const payload: CheckoutSelectedCartItemsDto = {
            cartItemIds,
            ...(voucherId && { voucherId })
        };
        
        const response = await fetch(`${this.apiUrl}/checkout`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${getToken()}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(payload)
        });
        
        const result = await response.json();
        
        if (!response.ok) {
            throw new Error(result.message || 'Checkout failed');
        }
        
        return result;
    }
    
    // Tính toán preview v?i voucher
    calculateDiscountPreview(total: number, voucher: AvailableVoucherDto): {
        discountAmount: number;
        finalTotal: number;
    } {
        let discountAmount = 0;
        
        if (voucher.discountAmount > 0) {
            discountAmount = voucher.discountAmount;
        } else if (voucher.discountPercent && voucher.discountPercent > 0) {
            discountAmount = total * voucher.discountPercent / 100;
        }
        
        if (discountAmount > total) {
            discountAmount = total;
        }
        
        return {
            discountAmount,
            finalTotal: Math.max(total - discountAmount, 1) // PayOS minimum 1 VN?
        };
    }
}
### 3. React Components

#### Voucher Selector Componentinterface VoucherSelectorProps {
    selectedVoucherId?: string;
    onVoucherSelect: (voucherId: string | undefined) => void;
    cartTotal: number;
}

const VoucherSelector: React.FC<VoucherSelectorProps> = ({
    selectedVoucherId,
    onVoucherSelect,
    cartTotal
}) => {
    const [vouchers, setVouchers] = useState<AvailableVoucherDto[]>([]);
    const [loading, setLoading] = useState(false);
    
    const voucherService = new VoucherService();
    const checkoutService = new CheckoutService();
    
    useEffect(() => {
        loadVouchers();
    }, []);
    
    const loadVouchers = async () => {
        setLoading(true);
        try {
            const data = await voucherService.getAvailableVouchers();
            setVouchers(data);
        } catch (error) {
            console.error('Error loading vouchers:', error);
        } finally {
            setLoading(false);
        }
    };
    
    const calculateDiscount = (voucher: AvailableVoucherDto) => {
        return checkoutService.calculateDiscountPreview(cartTotal, voucher);
    };
    
    return (
        <div className="voucher-selector">
            <h3>?? Ch?n Voucher</h3>
            
            {loading ? (
                <div>?ang t?i voucher...</div>
            ) : (
                <div className="voucher-list">
                    <div 
                        className={`voucher-item ${!selectedVoucherId ? 'selected' : ''}`}
                        onClick={() => onVoucherSelect(undefined)}
                    >
                        <div className="voucher-content">
                            <h4>Không s? d?ng voucher</h4>
                            <p>Thanh toán: {cartTotal.toLocaleString()} VN?</p>
                        </div>
                    </div>
                    
                    {vouchers.map(voucher => {
                        const { discountAmount, finalTotal } = calculateDiscount(voucher);
                        const isSelected = selectedVoucherId === voucher.id;
                        
                        return (
                            <div 
                                key={voucher.id}
                                className={`voucher-item ${isSelected ? 'selected' : ''}`}
                                onClick={() => onVoucherSelect(voucher.id)}
                            >
                                <div className="voucher-content">
                                    <h4>{voucher.name}</h4>
                                    <p>{voucher.description}</p>
                                    <div className="voucher-discount">
                                        {voucher.discountAmount > 0 ? (
                                            <span>Gi?m {voucher.discountAmount.toLocaleString()} VN?</span>
                                        ) : (
                                            <span>Gi?m {voucher.discountPercent}%</span>
                                        )}
                                    </div>
                                    <div className="price-calculation">
                                        <div className="original-price">
                                            T?ng: <span className="strikethrough">{cartTotal.toLocaleString()} VN?</span>
                                        </div>
                                        <div className="discount-amount">
                                            Gi?m: -{discountAmount.toLocaleString()} VN?
                                        </div>
                                        <div className="final-price">
                                            <strong>Thanh toán: {finalTotal.toLocaleString()} VN?</strong>
                                        </div>
                                    </div>
                                    <div className="voucher-meta">
                                        <span>Còn l?i: {voucher.remainingCount}</span>
                                        {voucher.isExpiringSoon && (
                                            <span className="expiring-soon">?? S?p h?t h?n</span>
                                        )}
                                    </div>
                                </div>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
};
#### Admin Voucher Managementconst VoucherManagement: React.FC = () => {
    const [vouchers, setVouchers] = useState<VoucherDto[]>([]);
    const [showCreateModal, setShowCreateModal] = useState(false);
    const [loading, setLoading] = useState(false);
    
    const voucherService = new VoucherService();
    
    useEffect(() => {
        loadVouchers();
    }, []);
    
    const loadVouchers = async () => {
        setLoading(true);
        try {
            const data = await voucherService.getAllVouchers();
            setVouchers(data);
        } catch (error) {
            console.error('Error loading vouchers:', error);
        } finally {
            setLoading(false);
        }
    };
    
    const handleCreateVoucher = async (voucherData: CreateVoucherDto) => {
        try {
            const result = await voucherService.createVoucher(voucherData);
            if (result.success) {
                toast.success(result.message);
                loadVouchers(); // Reload list
                setShowCreateModal(false);
            } else {
                toast.error(result.message);
            }
        } catch (error) {
            toast.error('Có l?i x?y ra khi t?o voucher');
        }
    };
    
    return (
        <div className="voucher-management">
            <div className="header">
                <h2>Qu?n lý Voucher</h2>
                <button 
                    className="btn btn-primary"
                    onClick={() => setShowCreateModal(true)}
                >
                    + T?o Voucher M?i
                </button>
            </div>
            
            <div className="voucher-table">
                {loading ? (
                    <div>?ang t?i...</div>
                ) : (
                    <table>
                        <thead>
                            <tr>
                                <th>Tên Voucher</th>
                                <th>Lo?i Gi?m Giá</th>
                                <th>S? L??ng</th>
                                <th>?ã Dùng</th>
                                <th>Còn L?i</th>
                                <th>Tr?ng Thái</th>
                                <th>Hành ??ng</th>
                            </tr>
                        </thead>
                        <tbody>
                            {vouchers.map(voucher => (
                                <tr key={voucher.id}>
                                    <td>
                                        <div>
                                            <strong>{voucher.name}</strong>
                                            <br />
                                            <small>{voucher.description}</small>
                                        </div>
                                    </td>
                                    <td>
                                        {voucher.discountAmount > 0 ? (
                                            <span>-{voucher.discountAmount.toLocaleString()} VN?</span>
                                        ) : (
                                            <span>-{voucher.discountPercent}%</span>
                                        )}
                                    </td>
                                    <td>{voucher.quantity}</td>
                                    <td>{voucher.usedCount}</td>
                                    <td>{voucher.remainingCount}</td>
                                    <td>
                                        <span className={`status ${voucher.isActive ? 'active' : 'inactive'}`}>
                                            {voucher.isActive ? 'Ho?t ??ng' : 'T?m d?ng'}
                                        </span>
                                        {voucher.isExpired && (
                                            <span className="status expired">H?t h?n</span>
                                        )}
                                    </td>
                                    <td>
                                        <button className="btn btn-sm btn-secondary">S?a</button>
                                        <button className="btn btn-sm btn-danger">Xóa</button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                )}
            </div>
            
            {showCreateModal && (
                <CreateVoucherModal
                    onClose={() => setShowCreateModal(false)}
                    onSubmit={handleCreateVoucher}
                />
            )}
        </div>
    );
};

---

## ?? UI/UX Guidelines

### 1. Voucher Card Design.voucher-item {
    border: 2px solid #e1e5e9;
    border-radius: 12px;
    padding: 16px;
    margin-bottom: 12px;
    cursor: pointer;
    transition: all 0.3s ease;
    position: relative;
}

.voucher-item:hover {
    border-color: #007bff;
    box-shadow: 0 4px 12px rgba(0,123,255,0.15);
}

.voucher-item.selected {
    border-color: #28a745;
    background-color: #f8fff9;
}

.voucher-item.selected::after {
    content: "?";
    position: absolute;
    top: 8px;
    right: 8px;
    background: #28a745;
    color: white;
    border-radius: 50%;
    width: 24px;
    height: 24px;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 14px;
}

.voucher-discount {
    background: linear-gradient(45deg, #ff6b6b, #ffa500);
    color: white;
    padding: 4px 8px;
    border-radius: 16px;
    font-size: 12px;
    font-weight: bold;
    display: inline-block;
    margin: 8px 0;
}

.price-calculation {
    border-top: 1px dashed #ccc;
    padding-top: 8px;
    margin-top: 8px;
}

.original-price .strikethrough {
    text-decoration: line-through;
    color: #666;
}

.discount-amount {
    color: #dc3545;
    font-weight: 500;
}

.final-price {
    color: #28a745;
    font-size: 16px;
    margin-top: 4px;
}

.expiring-soon {
    color: #ffa500;
    font-weight: bold;
    font-size: 11px;
}
### 2. Checkout Summary v?i Voucherconst CheckoutSummary: React.FC = ({ cartItems, selectedVoucher, total }) => {
    const discount = selectedVoucher ? calculateDiscount(total, selectedVoucher) : 0;
    const finalTotal = total - discount;
    
    return (
        <div className="checkout-summary">
            <h3>Tóm t?t ??n hàng</h3>
            
            <div className="summary-line">
                <span>T?m tính ({cartItems.length} s?n ph?m)</span>
                <span>{total.toLocaleString()} VN?</span>
            </div>
            
            {selectedVoucher && (
                <div className="summary-line discount-line">
                    <span>
                        ?? {selectedVoucher.name}
                        <small className="voucher-desc">{selectedVoucher.description}</small>
                    </span>
                    <span className="discount-amount">-{discount.toLocaleString()} VN?</span>
                </div>
            )}
            
            <div className="summary-line total-line">
                <span><strong>T?ng c?ng</strong></span>
                <span className="final-total"><strong>{finalTotal.toLocaleString()} VN?</strong></span>
            </div>
            
            {discount > 0 && (
                <div className="savings-highlight">
                    ?? B?n ti?t ki?m ???c {discount.toLocaleString()} VN?!
                </div>
            )}
        </div>
    );
};
### 3. Thông báo Voucher m?iconst VoucherNotification: React.FC = ({ notification }) => {
    return (
        <div className="voucher-notification">
            <div className="notification-icon">??</div>
            <div className="notification-content">
                <h4>{notification.title}</h4>
                <p>{notification.message}</p>
                <button className="btn-view-vouchers">
                    Xem voucher ngay
                </button>
            </div>
        </div>
    );
};
---

## ?? Testing & Examples

### 1. Test Data// Mock voucher data for testing
const mockVouchers: AvailableVoucherDto[] = [
    {
        id: "voucher-1",
        name: "Gi?m 50K cho ??n t? 100K",
        description: "Áp d?ng cho t?t c? s?n ph?m",
        discountAmount: 50000,
        discountPercent: null,
        remainingCount: 45,
        startDate: "2024-01-01T00:00:00Z",
        endDate: "2024-01-31T23:59:59Z",
        isExpiringSoon: false
    },
    {
        id: "voucher-2", 
        name: "Gi?m 20% t?i ?a 100K",
        description: "Flash sale cu?i tu?n",
        discountAmount: 0,
        discountPercent: 20,
        remainingCount: 12,
        startDate: "2024-01-15T00:00:00Z",
        endDate: "2024-01-21T23:59:59Z",
        isExpiringSoon: true
    }
];
### 2. Test Scenariosdescribe('Voucher System Tests', () => {
    test('should calculate fixed amount discount correctly', () => {
        const voucher = mockVouchers[0]; // 50K discount
        const cartTotal = 120000;
        
        const result = calculateDiscount(cartTotal, voucher);
        
        expect(result.discountAmount).toBe(50000);
        expect(result.finalTotal).toBe(70000);
    });
    
    test('should calculate percentage discount correctly', () => {
        const voucher = mockVouchers[1]; // 20% discount
        const cartTotal = 100000;
        
        const result = calculateDiscount(cartTotal, voucher);
        
        expect(result.discountAmount).toBe(20000);
        expect(result.finalTotal).toBe(80000);
    });
    
    test('should not exceed cart total for discount', () => {
        const voucher = mockVouchers[0]; // 50K discount
        const cartTotal = 30000; // Less than discount
        
        const result = calculateDiscount(cartTotal, voucher);
        
        expect(result.discountAmount).toBe(30000);
        expect(result.finalTotal).toBe(1); // PayOS minimum
    });
});
---

## ?? Error Handling

### 1. Common Error Responsesinterface ApiError {
    statusCode: number;
    message: string;
    success: false;
    validationErrors?: string[];
}

// Example error responses:
const errorResponses = {
    VOUCHER_NOT_FOUND: {
        statusCode: 404,
        message: "Voucher không t?n t?i ho?c không kh? d?ng",
        success: false
    },
    PRODUCT_ON_SALE: {
        statusCode: 400,
        message: "S?n ph?m \"Tên s?n ph?m\" ?ang ???c gi?m giá, không th? áp d?ng voucher",
        success: false
    },
    VOUCHER_EXPIRED: {
        statusCode: 400,
        message: "Voucher ?ã h?t h?n",
        success: false
    },
    NO_REMAINING_USES: {
        statusCode: 400,
        message: "Voucher ?ã h?t l??t s? d?ng",
        success: false
    }
};
### 2. Error Handling Implementationclass ErrorHandler {
    static handleVoucherError(error: ApiError): string {
        switch (error.statusCode) {
            case 404:
                return "Voucher không t?n t?i ho?c ?ã b? xóa";
            case 400:
                if (error.message.includes("?ang ???c gi?m giá")) {
                    return "Không th? s? d?ng voucher cho s?n ph?m ?ang sale";
                }
                if (error.message.includes("h?t h?n")) {
                    return "Voucher ?ã h?t h?n s? d?ng";
                }
                if (error.message.includes("h?t l??t")) {
                    return "Voucher ?ã h?t l??t s? d?ng";
                }
                return error.message;
            case 401:
                return "B?n c?n ??ng nh?p ?? s? d?ng voucher";
            case 403:
                return "B?n không có quy?n s? d?ng voucher này";
            default:
                return "Có l?i x?y ra khi áp d?ng voucher";
        }
    }
    
    static showVoucherError(error: ApiError) {
        const message = this.handleVoucherError(error);
        toast.error(message);
    }
}
### 3. Validation tr??c khi g?i APIconst validateVoucherSelection = (
    voucher: AvailableVoucherDto, 
    cartItems: CartItem[] 
): string | null => {
    const now = new Date();
    const endDate = new Date(voucher.endDate);
    
    // Check expiry
    if (endDate < now) {
        return "Voucher ?ã h?t h?n";
    }
    
    // Check remaining uses
    if (voucher.remainingCount <= 0) {
        return "Voucher ?ã h?t l??t s? d?ng";
    }
    
    // Check if any product is on sale (client-side pre-check)
    const hasProductOnSale = cartItems.some(item => item.isSale);
    if (hasProductOnSale) {
        return "Không th? s? d?ng voucher khi có s?n ph?m ?ang sale trong gi? hàng";
    }
    
    return null; // Valid
};
---

## ?? Mobile Considerations

### 1. Responsive Voucher Cards@media (max-width: 768px) {
    .voucher-item {
        padding: 12px;
        margin-bottom: 8px;
    }
    
    .voucher-content h4 {
        font-size: 14px;
    }
    
    .price-calculation {
        font-size: 13px;
    }
    
    .voucher-list {
        max-height: 60vh;
        overflow-y: auto;
    }
}
### 2. Touch-friendly interactionsconst MobileVoucherSelector: React.FC = () => {
    return (
        <div className="mobile-voucher-selector">
            <button 
                className="voucher-toggle-btn"
                onClick={() => setShowVouchers(!showVouchers)}
            >
                ?? {selectedVoucher ? selectedVoucher.name : 'Ch?n voucher'}
                <span className="chevron">{showVouchers ? '?' : '?'}</span>
            </button>
            
            {showVouchers && (
                <div className="voucher-dropdown">
                    {/* Voucher list */}
                </div>
            )}
        </div>
    );
};
---

## ? Performance & Optimization

### **1. Frontend Performance**// Lazy loading voucher components
const VoucherSelector = lazy(() => import('./components/VoucherSelector'));
const VoucherManagement = lazy(() => import('./components/admin/VoucherManagement'));

// Memoization for expensive calculations
const memoizedDiscountCalculation = useMemo(() => {
    return calculateDiscountPreview(cartTotal, selectedVoucher);
}, [cartTotal, selectedVoucher]);

// Debounced voucher search
const debouncedSearch = useDebounce((searchTerm: string) => {
    searchVouchers(searchTerm);
}, 500);
### **2. API Optimization**// Request batching
const batchVoucherRequests = async (requests: VoucherRequest[]): Promise<VoucherResponse[]> => {
    const batchedRequest = {
        requests: requests.map(r => ({
            type: r.type,
            params: r.params
        }))
    };
    
    const response = await fetch('/api/vouchers/batch', {
        method: 'POST',
        body: JSON.stringify(batchedRequest)
    });
    
    return response.json();
};

// Connection pooling and keep-alive
const apiClient = new ApiClient({
    baseURL: process.env.API_BASE_URL,
    timeout: 10000,
    keepAlive: true,
    maxSockets: 50
});
---

## ?? Security Considerations

### **1. Input Validation**const sanitizeVoucherInput = (input: any): CreateVoucherDto => {
    return {
        name: validator.escape(input.name?.trim() || ''),
        description: validator.escape(input.description?.trim() || ''),
        quantity: Math.max(1, Math.min(10000, parseInt(input.quantity) || 1)),
        discountAmount: Math.max(0, parseFloat(input.discountAmount) || 0),
        discountPercent: input.discountPercent ? Math.max(0, Math.min(100, parseInt(input.discountPercent))) : null,
        startDate: new Date(input.startDate),
        endDate: new Date(input.endDate)
    };
};
### **2. Authorization**const checkVoucherPermissions = (user: User, action: string, voucherId?: string): boolean => {
    switch (action) {
        case 'CREATE':
        case 'UPDATE':
        case 'DELETE':
            return user.role === 'Admin';
        case 'VIEW_ALL':
            return user.role === 'Admin';
        case 'USE':
            return user.role === 'User' && user.isActive;
        default:
            return false;
    }
};
---

## ?? Integration Checklist

- [ ] **API Integration**
  - [ ] Implement VoucherService v?i t?t c? endpoints
  - [ ] Handle authentication headers
  - [ ] Implement error handling và retry logic
  - [ ] Add request/response logging
  - [ ] Setup API monitoring

- [ ] **UI Components**
  - [ ] VoucherSelector component v?i responsive design
  - [ ] CheckoutSummary v?i voucher calculation
  - [ ] Admin VoucherManagement component
  - [ ] VoucherNotification component
  - [ ] Loading states và error boundaries

- [ ] **State Management**
  - [ ] Voucher store/context v?i Redux/Zustand
  - [ ] Cart state v?i voucher selection
  - [ ] Notification state for voucher alerts
  - [ ] Cache management cho voucher data

- [ ] **Testing**
  - [ ] Unit tests cho discount calculation
  - [ ] Integration tests v?i API
  - [ ] E2E tests cho checkout flow
  - [ ] Performance tests
  - [ ] Security tests

- [ ] **Performance**
  - [ ] Lazy loading vouchers
  - [ ] Caching available vouchers
  - [ ] Debounce voucher selection
  - [ ] Image optimization
  - [ ] Bundle size optimization

- [ ] **Security**
  - [ ] Input sanitization
  - [ ] XSS protection
  - [ ] CSRF protection
  - [ ] Rate limiting implementation
  - [ ] Audit logging

- [ ] **Monitoring**
  - [ ] Error tracking (Sentry/similar)
  - [ ] Performance monitoring
  - [ ] Business metrics tracking
  - [ ] User behavior analytics

---

## ?? Support & Documentation

### API Base URLs:
- **Development**: `https://localhost:7205/api/Product`
- **Staging**: `https://staging.taynhinhtour.com/api/Product`
- **Production**: `https://api.taynhinhtour.com/api/Product`

### Key Contacts:
- **Backend Team**: backend@taynhinhtour.com - API endpoints và business logic
- **DevOps Team**: devops@taynhinhtour.com - Infrastructure và deployment
- **QA Team**: qa@taynhinhtour.com - Test scenarios và edge cases  
- **Product Team**: product@taynhinhtour.com - UX requirements và business rules

### Testing Accounts:Admin Account:
Email: admin@gmail.com
Password: 12345678h@
Role: Admin

User Account:
Email: user@gmail.com  
Password: 12345678h@
Role: User

Shop Account:
Email: shop@gmail.com
Password: 12345678h@
Role: Specialty Shop
### Documentation Links:
- **API Documentation**: `/swagger` ho?c `/api-docs`
- **Database Schema**: `/docs/database-schema.md`
- **Deployment Guide**: `/docs/deployment.md`
- **Troubleshooting**: `/docs/troubleshooting.md`

---

## ?? Deployment Checklist

### **Pre-deployment:**
- [ ] Code review completed
- [ ] All tests passing
- [ ] Security scan completed
- [ ] Performance benchmarks met
- [ ] Database migration scripts ready
- [ ] Backup procedures verified

### **Deployment:**
- [ ] Deploy to staging first
- [ ] Run smoke tests on staging
- [ ] Monitor staging for 24h
- [ ] Deploy to production during maintenance window
- [ ] Verify all endpoints working
- [ ] Check database connections
- [ ] Verify cache systems
- [ ] Test notification system

### **Post-deployment:**
- [ ] Monitor error rates for 2h
- [ ] Check performance metrics
- [ ] Verify business metrics
- [ ] Update monitoring dashboards
- [ ] Notify stakeholders of successful deployment

---

*Tài li?u này ???c c?p nh?t l?n cu?i: [Ngày hi?n t?i]*
*Version: 2.0.0 - Extended Operations Guide*

---

## ?? Change Log

### Version 2.0.0
- ? Added comprehensive system operation details
- ? Added performance optimization strategies
- ? Added security considerations and fraud detection
- ? Added monitoring and alerting guidelines
- ? Added deployment and DevOps procedures
- ? Added business analytics and health checks
- ? Extended error handling and validation rules

### Version 1.0.0
- ? Initial API documentation
- ? Basic frontend integration guide
- ? UI/UX guidelines
- ? Testing examples