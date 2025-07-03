# ?? H??ng D?n T�ch H?p Thanh To�n PayOS - Frontend

## ?? T?ng Quan

T�i li?u h??ng d?n t�ch h?p PayOS cho frontend React/Vue/Angular. Payment ???c x? l� t? ??ng sau khi user thanh to�n th�nh c�ng.

---

## ?? Lu?ng Thanh To�n

1. **Checkout** ? Backend tr? v? `{orderId, checkoutUrl}`
2. **Redirect to PayOS** ? User scan QR thanh to�n  
3. **PayOS redirect v?** ? Frontend nh?n `orderId` t? URL
4. **G?i API x�c nh?n** ? Backend x? l� t? ??ng
5. **Hi?n th? k?t qu?** ? Th�nh c�ng/Th?t b?i

---

## ?? Backend APIs S?n S�ng

| API | Method | M?c ?�ch |
|-----|--------|----------|
| `/api/Product/checkout` | POST | T?o ??n h�ng v� link thanh to�n |
| `/api/payment-callback/payment-success/{orderId}` | POST | X? l� thanh to�n th�nh c�ng |
| `/api/payment-callback/payment-cancel/{orderId}` | POST | X? l� thanh to�n b? h?y |
| `/api/payment-callback/check-status/{orderId}` | GET | Ki?m tra tr?ng th�i ??n h�ng |

---

## ?? Code Implementation

### 1. ?? Checkout Service
// CheckoutService.js
class CheckoutService {
  constructor(apiUrl, token) {
    this.apiUrl = apiUrl;
    this.token = token;
  }

  async checkout(cartItemIds) {
    const response = await fetch(`${this.apiUrl}/api/Product/checkout`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${this.token}`
      },
      body: JSON.stringify({ cartItemIds })
    });

    const result = await response.json();
    
    // L?u orderId ?? backup
    localStorage.setItem('currentOrderId', result.orderId);
    
    return result; // {orderId, checkoutUrl}
  }
}
### 2. ?? Checkout Component
// CheckoutPage.jsx
import React, { useState } from 'react';

const CheckoutPage = () => {
  const [loading, setLoading] = useState(false);
  const [selectedItems, setSelectedItems] = useState([]);

  const handleCheckout = async () => {
    setLoading(true);
    try {
      const checkoutService = new CheckoutService(API_URL, authToken);
      const result = await checkoutService.checkout(selectedItems);
      
      // Redirect ??n PayOS
      window.location.href = result.checkoutUrl;
    } catch (error) {
      alert('L?i: ' + error.message);
      setLoading(false);
    }
  };

  return (
    <div>
      <h2>Thanh To�n</h2>
      {/* Ch?n s?n ph?m */}
      <button onClick={handleCheckout} disabled={loading}>
        {loading ? '?ang x? l�...' : 'Thanh To�n PayOS'}
      </button>
    </div>
  );
};
### 3. ? Trang Thanh To�n Th�nh C�ng
// PaymentSuccessPage.jsx
import React, { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';

const PaymentSuccessPage = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [status, setStatus] = useState('processing');
  const [message, setMessage] = useState('?ang x? l�...');

  useEffect(() => {
    // L?y orderId t? URL ho?c localStorage
    const orderId = searchParams.get('orderId') || localStorage.getItem('currentOrderId');
    
    if (orderId) {
      processPayment(orderId);
    } else {
      setStatus('error');
      setMessage('Kh�ng t�m th?y th�ng tin ??n h�ng');
    }
  }, []);

  const processPayment = async (orderId) => {
    try {
      const response = await fetch(`${API_URL}/api/payment-callback/payment-success/${orderId}`, {
        method: 'POST'
      });

      const data = await response.json();

      if (data.inventoryUpdated && data.cartCleared) {
        setStatus('success');
        setMessage('Thanh to�n th�nh c�ng!');
        localStorage.removeItem('currentOrderId');
        setTimeout(() => navigate('/orders'), 3000);
      } else if (data.shouldRetry) {
        setTimeout(() => processPayment(orderId), 3000);
      }
    } catch (error) {
      setStatus('error');
      setMessage('L?i x? l� thanh to�n');
    }
  };

  return (
    <div className="payment-page">
      {status === 'processing' && (
        <div>
          <div className="spinner"></div>
          <h2>?ang x? l� thanh to�n</h2>
          <p>{message}</p>
        </div>
      )}

      {status === 'success' && (
        <div>
          <h2>? Thanh to�n th�nh c�ng!</h2>
          <p>??n h�ng ?� ???c x? l�. Chuy?n h??ng trong gi�y l�t...</p>
        </div>
      )}

      {status === 'error' && (
        <div>
          <h2>? C� l?i x?y ra</h2>
          <p>{message}</p>
          <button onClick={() => navigate('/cart')}>V? gi? h�ng</button>
        </div>
      )}
    </div>
  );
};
### 4. ? Trang H?y Thanh To�n
// PaymentCancelPage.jsx
import React, { useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';

const PaymentCancelPage = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();

  useEffect(() => {
    const orderId = searchParams.get('orderId');
    if (orderId) {
      // G?i API x? l� h?y thanh to�n
      fetch(`${API_URL}/api/payment-callback/payment-cancel/${orderId}`, {
        method: 'POST'
      });
    }
  }, []);

  return (
    <div className="payment-cancel-page">
      <h2>?? Thanh to�n ?� b? h?y</h2>
      <p>S?n ph?m v?n ???c gi? trong gi? h�ng.</p>
      <button onClick={() => navigate('/cart')}>V? gi? h�ng</button>
      <button onClick={() => navigate('/products')}>Ti?p t?c mua s?m</button>
    </div>
  );
};
### 5. ??? Router Setup
// App.jsx
import { Routes, Route } from 'react-router-dom';

function App() {
  return (
    <Routes>
      <Route path="/checkout" element={<CheckoutPage />} />
      <Route path="/payment-success" element={<PaymentSuccessPage />} />
      <Route path="/payment-cancel" element={<PaymentCancelPage />} />
    </Routes>
  );
}
---

## ?? CSS ??n Gi?n
.payment-page {
  text-align: center;
  padding: 2rem;
}

.spinner {
  border: 4px solid #f3f3f3;
  border-top: 4px solid #3498db;
  border-radius: 50%;
  width: 40px;
  height: 40px;
  animation: spin 1s linear infinite;
  margin: 0 auto;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}

button {
  padding: 10px 20px;
  margin: 10px;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 5px;
  cursor: pointer;
}

button:disabled {
  background: #ccc;
}
---

## ?? Test Checklist

### ? C?n Test:

- [ ] Checkout v?i s?n ph?m h?p l?
- [ ] Checkout v?i gi? h�ng tr?ng (l?i)
- [ ] Thanh to�n th�nh c�ng tr�n PayOS
- [ ] H?y thanh to�n tr�n PayOS
- [ ] Refresh trang trong l�c x? l�
- [ ] orderId l?u trong localStorage
- [ ] Redirect v? trang ?�ng sau thanh to�n

---

## ?? L?u � Quan Tr?ng

1. **Environment Variables:**REACT_APP_API_URL=https://localhost:7205
2. **PayOS URLs ???c config s?n:**
   - Success: `https://yoursite.com/payment-success?orderId={orderId}`
   - Cancel: `https://yoursite.com/payment-cancel?orderId={orderId}`

3. **Debug Tools:**// Console commands ?? debug
PaymentDebugger.checkOrderStatus('order-id');
PaymentDebugger.testCallback('order-id', 'PAID');
---

## ?? H? Tr?

- **Debug order:** `GET /api/payment-callback/debug/{orderId}`
- **Manual test:** `POST /api/payment-callback/confirm/{orderId}`
- **Auto-check:** `POST /api/payment-callback/auto-check`

---

**?? Payment system s?n s�ng s? d?ng!**