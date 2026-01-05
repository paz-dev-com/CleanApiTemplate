# API Testing Guide

## Base URL
```
https://localhost:5001
http://localhost:5000
```

## Authentication Flow

### 1. Register a New User

**POST** `/api/auth/register`

```json
{
  "username": "johndoe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+1234567890"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "error": null,
  "validationErrors": null
}
```

### 2. Login

**POST** `/api/auth/login`

```json
{
  "username": "johndoe",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64encodedrefreshtoken==",
    "expiresAt": "2024-01-15T10:30:00Z",
    "username": "johndoe",
    "email": "john.doe@example.com",
    "roles": ["User"]
  },
  "error": null
}
```

### 3. Get Current User

**GET** `/api/auth/me`

**Headers:**
```
Authorization: Bearer {your-jwt-token}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "username": "johndoe",
    "email": "john.doe@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phoneNumber": "+1234567890",
    "isActive": true,
    "emailConfirmed": false,
    "lastLoginAt": "2024-01-15T09:30:00Z",
    "createdAt": "2024-01-14T15:20:00Z"
  }
}
```

### 4. Refresh Token

**POST** `/api/auth/refresh`

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64encodedrefreshtoken=="
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "newbase64encodedrefreshtoken==",
    "expiresAt": "2024-01-15T11:30:00Z",
    "username": "johndoe",
    "email": "john.doe@example.com",
    "roles": ["User"]
  }
}
```

## Product Management

### 1. Get Products (Paginated)

**GET** `/api/products?pageNumber=1&pageSize=10&searchTerm=laptop&categoryId={guid}&includeInactive=false`

**Headers:**
```
Authorization: Bearer {your-jwt-token} (optional for public access)
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3,
    "items": [
      {
        "id": "guid-here",
        "name": "Gaming Laptop",
        "description": "High-performance gaming laptop",
        "sku": "LAPTOP-001",
        "price": 1299.99,
        "stockQuantity": 10,
        "isActive": true,
        "categoryId": "category-guid",
        "categoryName": "Electronics",
        "createdAt": "2024-01-10T10:00:00Z",
        "createdBy": "admin"
      }
    ],
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### 2. Get Product by ID

**GET** `/api/products/{id}`

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "data": {
    "id": "guid-here",
    "name": "Gaming Laptop",
    "description": "High-performance gaming laptop",
    "sku": "LAPTOP-001",
    "price": 1299.99,
    "stockQuantity": 10,
    "isActive": true,
    "categoryId": "category-guid",
    "categoryName": "Electronics",
    "createdAt": "2024-01-10T10:00:00Z",
    "createdBy": "admin"
  }
}
```

### 3. Create Product

**POST** `/api/products`

**Headers:**
```
Authorization: Bearer {your-jwt-token}
Content-Type: application/json
```

**Body:**
```json
{
  "name": "Wireless Mouse",
  "description": "Ergonomic wireless mouse",
  "sku": "MOUSE-001",
  "price": 29.99,
  "stockQuantity": 100,
  "categoryId": "electronics-category-guid"
}
```

**Response (201 Created):**
```json
{
  "isSuccess": true,
  "data": "new-product-guid",
  "error": null
}
```

### 4. Update Product

**PUT** `/api/products/{id}`

**Headers:**
```
Authorization: Bearer {your-jwt-token}
Content-Type: application/json
```

**Body:**
```json
{
  "name": "Wireless Mouse - Updated",
  "description": "Ergonomic wireless mouse with RGB",
  "sku": "MOUSE-001",
  "price": 34.99,
  "stockQuantity": 95,
  "categoryId": "electronics-category-guid",
  "isActive": true
}
```

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "error": null
}
```

### 5. Delete Product

**DELETE** `/api/products/{id}`

**Headers:**
```
Authorization: Bearer {your-jwt-token}
```

**Note:** Requires Admin role

**Response (200 OK):**
```json
{
  "isSuccess": true,
  "error": null
}
```

## Health Checks

### 1. Application Health

**GET** `/health`

**Response (200 OK):**
```json
{
  "status": "healthy",
  "timestamp": "2024-01-15T10:00:00Z",
  "environment": "Development",
  "version": "1.0.0"
}
```

### 2. Database Health

**GET** `/health/db`

**Response (200 OK):**
```json
{
  "status": "healthy",
  "database": "connected"
}
```

**Response (503 Service Unavailable) - If DB is down:**
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.6.4",
  "title": "An error occurred while processing your request.",
  "status": 503,
  "detail": "Connection timeout..."
}
```

## Error Responses

### Validation Error (400 Bad Request)
```json
{
  "isSuccess": false,
  "data": null,
  "error": "Validation failed",
  "validationErrors": {
    "Name": ["Product name is required"],
    "Price": ["Price must be greater than zero"]
  }
}
```

### Unauthorized (401)
```json
{
  "isSuccess": false,
  "data": null,
  "error": "Invalid username or password",
  "validationErrors": null
}
```

### Forbidden (403)
```json
{
  "statusCode": 403,
  "message": "User does not have the required role",
  "traceId": "00-trace-id-here-00"
}
```

### Not Found (404)
```json
{
  "isSuccess": false,
  "data": null,
  "error": "Product with ID 'guid' not found",
  "validationErrors": null
}
```

### Server Error (500)
```json
{
  "statusCode": 500,
  "message": "An unexpected error occurred",
  "traceId": "00-trace-id-here-00"
}
```

## Testing with cURL

### Register User
```bash
curl -X POST https://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "Test123!",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### Login
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "Test123!"
  }'
```

### Get Products with Auth
```bash
curl -X GET "https://localhost:5001/api/products?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE"
```

### Create Product
```bash
curl -X POST https://localhost:5001/api/products \
  -H "Authorization: Bearer YOUR_JWT_TOKEN_HERE" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Test Product",
    "sku": "TEST-001",
    "price": 99.99,
    "stockQuantity": 50,
    "categoryId": "CATEGORY_GUID_HERE"
  }'
```

## Testing with PowerShell

### Register User
```powershell
$body = @{
    username = "testuser"
    email = "test@example.com"
    password = "Test123!"
    firstName = "Test"
    lastName = "User"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/auth/register" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

### Login and Save Token
```powershell
$loginBody = @{
    username = "testuser"
    password = "Test123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:5001/api/auth/login" `
    -Method Post `
    -Body $loginBody `
    -ContentType "application/json"

$token = $response.data.token
```

### Use Token for Authenticated Request
```powershell
$headers = @{
    Authorization = "Bearer $token"
}

Invoke-RestMethod -Uri "https://localhost:5001/api/products" `
    -Method Get `
    -Headers $headers
```

## Postman Collection

Import this JSON to get started with Postman:

```json
{
  "info": {
    "name": "Clean API Template",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "auth": {
    "type": "bearer",
    "bearer": [
      {
        "key": "token",
        "value": "{{jwt_token}}",
        "type": "string"
      }
    ]
  },
  "variable": [
    {
      "key": "base_url",
      "value": "https://localhost:5001",
      "type": "string"
    },
    {
      "key": "jwt_token",
      "value": "",
      "type": "string"
    }
  ]
}
```

## Common Issues

### SSL Certificate Errors
```bash
# For development, you may need to trust the self-signed certificate
dotnet dev-certs https --trust
```

### CORS Errors
- Ensure CORS is properly configured in Program.cs
- Check that allowed origins match your client application

### 401 Unauthorized
- Verify JWT token is not expired
- Check token is included in Authorization header
- Ensure format is: `Bearer {token}`

### 403 Forbidden
- User doesn't have required role
- Check role assignments in database
- Verify [Authorize(Roles = "...")] attribute

## Rate Limiting
- Default: 100 requests per minute per IP
- Returns 429 Too Many Requests when exceeded
- Headers include: X-Rate-Limit-Limit, X-Rate-Limit-Remaining
