# API Debugging Guide - Network Errors

## Common Network Errors and Solutions

### 1. 401 Unauthorized Error

**Səbəb:** JWT token yoxdur və ya etibarsızdır

**Yoxlama:**
```
Network Tab → Headers → Request Headers → Authorization: Bearer <token>
```

**Həll:**
- Frontend-də localStorage-də `authToken` olduğunu yoxlayın
- Token-in müddəti keçməyib (login təzədən edin)
- Interceptor-in düzgün işlədiyini yoxlayın

**Backend log:**
```bash
# Check if JWT validation is failing
dotnet run --urls="http://localhost:5208"
```

---

### 2. 400 Bad Request - "User has no associated clinic"

**Səbəb:** JWT token-da `ClinicId` claim-i yoxdur

**Yoxlama JWT token-ı:**
```javascript
// Browser Console
const token = localStorage.getItem('authToken');
const base64Url = token.split('.')[1];
const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
const jsonPayload = decodeURIComponent(atob(base64).split('').map(c =>
    '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)
).join(''));
console.log(JSON.parse(jsonPayload));
// ClinicId olmalıdır
```

**Həll:**
- Login təzədən edin
- AuthController-də ClinicId-nin token-a əlavə olduğunu yoxlayın
- User-in ClinicAdmin role-u olduğunu yoxlayın

---

### 3. 404 Not Found - "Clinic not found or inactive"

**Səbəb:** Database-də həmin clinic ID ilə aktiv klinik yoxdur

**Yoxlama:**
```sql
SELECT * FROM Clinics WHERE Id = '<clinic-id-from-token>' AND IsActive = 1
```

**Həll:**
- Database-də klinik olduğunu yoxlayın
- Klinik-in IsActive = true olduğunu yoxlayın
- JWT token-dakı ClinicId düzgün olduğunu yoxlayın

---

### 4. CORS Error

**Səbəb:** Backend CORS policy frontend origin-ə icazə vermir

**Browser console error:**
```
Access to XMLHttpRequest at 'http://localhost:5208/api/clinics/info'
from origin 'http://localhost:4200' has been blocked by CORS policy
```

**Həll:**
Program.cs-də CORS policy-ni yoxlayın:
```csharp
app.UseCors("AllowFrontend"); // Bu UseAuthentication-dan ƏVVƏL olmalıdır
app.UseAuthentication();
app.UseAuthorization();
```

---

### 5. Network Tab-da Request görmək

**Chrome DevTools:**
1. F12 → Network tab
2. "Preserve log" checkbox-u aktivləşdirin
3. Red (qırmızı) status code-lar xətadır
4. Request-ə klik edin:
   - **Headers** tab: Request/Response headers
   - **Payload** tab: Request body
   - **Response** tab: Server cavabı
   - **Preview** tab: JSON formatted response

---

### 6. GetClinicInfo API - Expected Behavior

**Request:**
```
GET /api/clinics/info HTTP/1.1
Authorization: Bearer eyJhbGc...
```

**Success Response (200):**
```json
{
  "id": "guid-here",
  "name": "Salymed Tibb Mərkəzi",
  "type": "general",
  "address": "...",
  ...
}
```

**Error Responses:**

**401 Unauthorized:**
```json
"Unauthorized"
```
Token yoxdur və ya etibarsızdır

**400 Bad Request:**
```json
{
  "message": "User has no associated clinic. Please contact support."
}
```
JWT-də ClinicId claim-i yoxdur

**404 Not Found:**
```json
{
  "message": "Clinic not found or inactive"
}
```
Klinik database-də yoxdur və ya IsActive = false

---

## Quick Debug Steps

1. **Network Tab-da xətanı tap**
   - Status code-u qeyd et (401, 400, 404, 500)
   - URL-i qeyd et
   - Response body-ni oxu

2. **Headers yoxla**
   - Authorization header varmı?
   - X-Clinic-Id header varmı? (artıq lazım deyil, JWT istifadə edir)

3. **Token-ı decode et**
   - jwt.io saytında token-ı yapışdır
   - ClinicId claim-inin olduğunu yoxla

4. **Database yoxla**
   - User var?
   - User-in Role = "ClinicAdmin"?
   - Clinic var və IsActive = true?

5. **Backend logs**
   - Console-da xəta mesajı var?
   - Exception stack trace oxu

---

## Testing with Postman/curl

**Login:**
```bash
curl -X POST http://localhost:5208/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@salymed.az",
    "password": "YourPassword123"
  }'
```

**Get Clinic Info (with token):**
```bash
curl -X GET http://localhost:5208/api/clinics/info \
  -H "Authorization: Bearer <your-token-here>"
```

Expected response: Clinic JSON object
