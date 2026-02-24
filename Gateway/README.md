# Gateway Service

## Overview
The **Gateway Service** is the central entry point for the entire **Unified Microservices Platform**. It routes all incoming traffic to the appropriate downstream microservices using **YARP (Yet Another Reverse Proxy)**.

## 🚀 Key Features
-   **Centralized Routing**: Maps external requests (e.g., `/auth/*`) to internal services (e.g., `http://auth-service:8080`).
-   **Authentication Proxy**: Validates JWTs before forwarding requests to protected endpoints.
-   **Health Checks UI**: Aggregates health status from all downstream services into a single dashboard.
-   **Rate Limiting**: Protects backend services from abuse.
-   **CORS Management**: Centralized Cross-Origin Resource Sharing policies.

## 🛠️ Configuration
The routing logic is defined in `appsettings.json` (or `appsettings.Docker.json` for containerized environments).

### Main Routes
| Route | Cluster | Destination |
| :--- | :--- | :--- |
| `/auth/*` | `auth-cluster` | Auth Service |
| `/users/*` | `users-cluster` | Users Service |
| `/apps/*` | `apps-cluster` | Apps Service |
| `/notifications/*` | `notifications-cluster` | Notifications Service |
| `/media/*` | `media-cluster` | Media Service |
| `/chat/*` | `chat-cluster` | Chat Service |
| `/payments/*` | `payments-cluster` | Payments Service |
| `/audit/*` | `audit-cluster` | Audit Service |
| `/search/*` | `search-cluster` | Search Service |
| `/geo/*` | `geo-cluster` | Geo Service |
| `/recommendation/*` | `rec-cluster` | Recommendation Service |

## 📦 Tech Stack
-   **.NET 8**
-   **YARP**
-   **AspNetCore.HealthChecks.UI**


# Gateway API Endpoints Documentation

This document outlines all endpoints exposed through the API Gateway, detailing the URL paths, required HTTP methods, required headers, authentication requirements, explicit execution permissions, and realistic input/output samples.

## Postman Environment Setup

To run the generated `Gateway_Collection.postman_collection.json` in Postman, ensure the following environment variables are configured in your active Environment scope:
- `gateway_url`: The address and port of your API Gateway (e.g. `http://localhost:7032` or `https://api.yourdomain.com`).
- `app_id`: The ID of the App Context routing the request (`Guid`).
- `tenant_id`: The optional Tenant context variable.
- `jwt_token`: The Bearer authorization token. Utilizing the *Login* endpoint will automatically populate this variable for subsequent requests.

---

## Apps.API

### Apps Endpoint Catalog

#### `GET` /apps/api/Apps/{id}

> **Purpose**: Get App By Id (api/Apps/)

**Get App By Id**

This endpoint executes the `GetAppById` operation. 

**Authorization Context:**
Requires 'AccessAll' or 'ManageApps' claim, OR all of ('AssignApps', 'EditApps', 'CreateApps').

**Authentication:** Bearer Token (`{{jwt_token}}`)

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `PUT` /apps/api/Apps/{id}

> **Purpose**: Update App (api/Apps/)

**Update App**

This endpoint executes the `UpdateApp` operation. 

**Authorization Context:**
Requires 'AccessAll' or 'ManageApps' claim, OR all of ('AssignApps', 'EditApps', 'CreateApps').

**Authentication:** Bearer Token (`{{jwt_token}}`)

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "Id": "400c3b9f-8dbe-4c78-96e9-07ef54bd1614",
    "Name": "Example Name",
    "Description": "Detailed description of the resource.",
    "BaseUrl": "https://example.com",
    "ThemeJson": "{}"
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `PATCH` /apps/api/Apps/{id}/status

> **Purpose**: Toggle Status (api/Apps/{id}/)

**Toggle Status**

This endpoint executes the `ToggleStatus` operation. 

**Authorization Context:**
Requires 'AccessAll' or 'ManageApps' claim, OR all of ('AssignApps', 'EditApps', 'CreateApps').

**Authentication:** Bearer Token (`{{jwt_token}}`)

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "Id": "471e3daa-056c-42fd-816a-55175b97b8d7",
    "IsActive": true
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `PATCH` /apps/api/Apps/{id}/external-auth

> **Purpose**: Update External Auth (api/Apps/{id}/)

**Update External Auth**

This endpoint executes the `UpdateExternalAuth` operation. 

**Authorization Context:**
Requires 'AccessAll' or 'ManageApps' claim, OR all of ('AssignApps', 'EditApps', 'CreateApps').

**Authentication:** Bearer Token (`{{jwt_token}}`)

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "Id": "84304a79-218a-4902-91c7-0de2045cb091",
    "ExternalLoginsJson": "{}"
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /apps/api/Apps/{id}/packages

> **Purpose**: Get Packages (api/Apps/{id}/)

**Get Packages**

This endpoint executes the `GetPackages` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /apps/api/Apps/{appId}/users/{userId}/subscriptions

> **Purpose**: Get User Subscriptions (api/Apps/{appId}/users/{userId}/)

**Get User Subscriptions**

This endpoint executes the `GetUserSubscriptions` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /apps/api/Apps/{appId}/users/{userId}/subscriptions

> **Purpose**: Grant Subscription (api/Apps/{appId}/users/{userId}/)

**Grant Subscription**

This endpoint executes the `GrantSubscription` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "UserId": "00000000-0000-0000-0000-000000000000",
    "AppId": "33333333-3333-3333-3333-333333333330",
    "PackageId": "57596d3c-2c6e-487f-928d-5b527c0413fb",
    "StartDate": "2026-02-23T12:00:00Z",
    "EndDate": "2026-02-23T12:00:00Z"
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `PUT` /apps/api/Apps/{appId}/subscriptions/{id}/status

> **Purpose**: Change Subscription Status (api/Apps/{appId}/subscriptions/{id}/)

**Change Subscription Status**

This endpoint executes the `ChangeSubscriptionStatus` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "SubscriptionId": "fb9964f0-26ca-4c1e-b25c-a3d0faebd78a",
    "IsActive": true
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

## Audit.API

### Audit Endpoint Catalog

#### `GET` /audit/api/Audit/stats?days=demo_value

> **Purpose**: Get Stats (api/Audit/)

**Get Stats**

This endpoint executes the `GetStats` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `days`: demo_value

**Expected Successful Response:**
```json
[
    {}
]
```

---

## Auth.API

### Analytics Endpoint Catalog

#### `GET` /auth/api/Analytics/app-user-stats

> **Purpose**: Get App User Stats (api/Analytics/)

**Get App User Stats**

This endpoint executes the `GetAppUserStats` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

### Auth Endpoint Catalog

#### `POST` /auth/api/Auth/register

> **Purpose**: Register (api/Auth/)

**Register**

This endpoint executes the `Register` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "Email": "user@example.com",
    "Phone": "+15551234567",
    "Password": "SecureP@ssw0rd!",
    "VerificationType": {},
    "RequiresAdminApproval": true
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /auth/api/Auth/login

> **Purpose**: Login (api/Auth/)

**Login**

This endpoint executes the `Login` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "Email": "user@example.com",
    "Password": "SecureP@ssw0rd!",
    "IpAddress": "sample_string_for_IpAddress",
    "UserAgent": "sample_string_for_UserAgent"
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /auth/api/Auth/otp/request

> **Purpose**: Request Otp (api/Auth/otp/)

**Request Otp**

This endpoint executes the `RequestOtp` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "Email": "user@example.com",
    "Type": {}
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /auth/api/Auth/otp/verify

> **Purpose**: Verify Otp (api/Auth/otp/)

**Verify Otp**

This endpoint executes the `VerifyOtp` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "Email": "user@example.com",
    "Code": "654321",
    "Type": {}
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /auth/api/Auth/password/forgot

> **Purpose**: Forgot Password (api/Auth/password/)

**Forgot Password**

This endpoint executes the `ForgotPassword` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "Email": "user@example.com"
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /auth/api/Auth/password/reset

> **Purpose**: Reset Password (api/Auth/password/)

**Reset Password**

This endpoint executes the `ResetPassword` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "Email": "user@example.com",
    "Code": "654321",
    "NewPassword": "SecureP@ssw0rd!"
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /auth/api/Auth/reactivate/initiate

> **Purpose**: Initiate Reactivation (api/Auth/reactivate/)

**Initiate Reactivation**

This endpoint executes the `InitiateReactivation` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "OldEmail": "user@example.com",
    "NewEmail": "user@example.com"
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /auth/api/Auth/reactivate/verify

> **Purpose**: Verify Reactivation (api/Auth/reactivate/)

**Verify Reactivation**

This endpoint executes the `VerifyReactivation` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "Email": "user@example.com",
    "Code": "654321"
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /auth/api/Auth/refresh

> **Purpose**: Refresh (api/Auth/)

**Refresh**

This endpoint executes the `Refresh` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /auth/api/Auth/sessions

> **Purpose**: Get Sessions (api/Auth/)

**Get Sessions**

This endpoint executes the `GetSessions` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `PUT` /auth/api/Auth/users/{id}/status

> **Purpose**: Update User Status (api/Auth/users/{id}/)

**Update User Status**

This endpoint executes the `UpdateUserStatus` operation. 

**Authorization Context:**
Requires Role(s): SuperAdmin OR UsersManager.

**Authentication:** Bearer Token (`{{jwt_token}}`)

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `PUT` /auth/api/Auth/users/{id}/verify

> **Purpose**: Update User Verification (api/Auth/users/{id}/)

**Update User Verification**

This endpoint executes the `UpdateUserVerification` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /auth/api/Auth/users/{id}/apps

> **Purpose**: Add User To App (api/Auth/users/{id}/)

**Add User To App**

This endpoint executes the `AddUserToApp` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `DELETE` /auth/api/Auth/users/{id}/apps

> **Purpose**: Remove User From App (api/Auth/users/{id}/apps/)

**Remove User From App**

This endpoint executes the `RemoveUserFromApp` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `PUT` /auth/api/Auth/users/{id}/apps/status

> **Purpose**: Update App Status (api/Auth/users/{id}/apps/{appId}/)

**Update App Status**

This endpoint executes the `UpdateAppStatus` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `PUT` /auth/api/Auth/users/{id}/apps/role

> **Purpose**: Assign Role (api/Auth/users/{id}/apps/{appId}/)

**Assign Role**

This endpoint executes the `AssignRole` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `DELETE` /auth/api/Auth/sessions/{sessionId}

> **Purpose**: Revoke Session (api/Auth/sessions/)

**Revoke Session**

This endpoint executes the `RevokeSession` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /auth/api/Auth/users

> **Purpose**: Get All Users (api/Auth/)

**Get All Users**

This endpoint executes the `GetAllUsers` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `appId`: demo_value

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /auth/api/Auth/logout

> **Purpose**: Logout (api/Auth/)

**Logout**

This endpoint executes the `Logout` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

### ExternalAuth Endpoint Catalog

#### `GET` /auth/api/ExternalAuth/login/{provider}

> **Purpose**: Callback (api/ExternalAuth/login/)

**Callback**

This endpoint executes the `Callback` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /auth/api/ExternalAuth/callback

> **Purpose**: Callback (api/ExternalAuth/)

**Callback**

This endpoint executes the `Callback` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /auth/api/ExternalAuth/link/{provider}

> **Purpose**: Callback Link (api/ExternalAuth/link/)

**Callback Link**

This endpoint executes the `CallbackLink` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /auth/api/ExternalAuth/link-callback

> **Purpose**: Callback Link (api/ExternalAuth/)

**Callback Link**

This endpoint executes the `CallbackLink` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `DELETE` /auth/api/ExternalAuth/users/{userId}/links/{provider}

> **Purpose**: Unlink (api/ExternalAuth/users/{userId}/links/)

**Unlink**

This endpoint executes the `Unlink` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

### Maintenance Endpoint Catalog

#### `DELETE` /auth/api/Maintenance/reset

> **Purpose**: Reset App Data (api/Maintenance/reset/)

**Reset App Data**

This endpoint executes the `ResetAppData` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

### Permissions Endpoint Catalog

#### `GET` /auth/api/Permissions/check?userId=demo_value&permission=demo_value

> **Purpose**: Check Permission (api/Permissions/)

**Check Permission**

This endpoint executes the `CheckPermission` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `userId`: demo_value
- `appId`: demo_value
- `permission`: demo_value

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

### Roles Endpoint Catalog

#### `POST` /auth/api/Roles/assign

> **Purpose**: Assign Role (api/Roles/)

**Assign Role**

This endpoint executes the `AssignRole` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "UserId": "00000000-0000-0000-0000-000000000000",
    "RoleId": "bba2be1f-7acd-48ee-b65e-d91905234882"
  }
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

## Chat.API

### Chat Endpoint Catalog

#### `GET` /chat/api/Chat/history?senderId=demo_value&recipientId=demo_value

> **Purpose**: Get History (api/Chat/)

**Get History**

This endpoint executes the `GetHistory` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `senderId`: demo_value
- `recipientId`: demo_value

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /chat/api/Chat/flagged?appId=demo_value

> **Purpose**: Get Flagged Messages (api/Chat/)

**Get Flagged Messages**

This endpoint executes the `GetFlaggedMessages` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `appId`: demo_value

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /chat/api/Chat/channels

> **Purpose**: Create Channel (api/Chat/)

**Create Channel**

This endpoint executes the `CreateChannel` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

## Geo.API

### Geo Endpoint Catalog

#### `GET` /geo/api/Geo/nearby?appId=demo_value&lat=demo_value&lon=demo_value&radiusKm=demo_value

> **Purpose**: Get Nearby (api/Geo/)

**Get Nearby**

This endpoint executes the `GetNearby` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `appId`: demo_value
- `lat`: demo_value
- `lon`: demo_value
- `radiusKm`: demo_value

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /geo/api/Geo/location

> **Purpose**: Update Location (api/Geo/)

**Update Location**

This endpoint executes the `UpdateLocation` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "UserId": 1,
    "AppId": 1,
    "Latitude": 1.5,
    "Longitude": 1.5
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

### Lookups Endpoint Catalog

#### `GET` /geo/api/Lookups/countries

> **Purpose**: Get Countries (api/Lookups/)

**Get Countries**

This endpoint executes the `GetCountries` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
[
    {
        "Name": "Example Name",
        "Code": "654321",
        "PhoneCode": "+15551234567",
        "IsActive": true,
        "Cities": {
            "Name": "Example Name",
            "CountryId": "ae115aaa-228f-455d-aac9-c573b2932c3d",
            "Country": {},
            "IsActive": true
        }
    }
]
```

---

#### `GET` /geo/api/Lookups/cities/{countryId}

> **Purpose**: Get Cities (api/Lookups/cities/)

**Get Cities**

This endpoint executes the `GetCities` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
[
    {
        "Name": "Example Name",
        "CountryId": "cb3e2383-3d42-472c-a8a3-195df515c5a3",
        "Country": {},
        "IsActive": true
    }
]
```

---

## Media.API

### Media Endpoint Catalog

#### `POST` /media/api/Media/upload

> **Purpose**: Upload (api/Media/)

**Upload**

This endpoint executes the `Upload` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /media/api/Media/flagged

> **Purpose**: Get Url (api/Media/)

**Get Url**

This endpoint executes the `GetUrl` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /media/api/Media/{id}/url

> **Purpose**: Get Url (api/Media/{id}/)

**Get Url**

This endpoint executes the `GetUrl` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

## Notifications.API

### Config Endpoint Catalog

#### `POST` /notifications/api/Config/test-sms

> **Purpose**: Test Sms (api/Config/)

**Test Sms**

This endpoint executes the `TestSms` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /notifications/api/Config/test-email

> **Purpose**: Test Email (api/Config/)

**Test Email**

This endpoint executes the `TestEmail` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

### Notifications Endpoint Catalog

#### `GET` /notifications/api/Notifications/unread

> **Purpose**: Get Unread (api/Notifications/)

**Get Unread**

This endpoint executes the `GetUnread` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `PUT` /notifications/api/Notifications/{id}/read

> **Purpose**: Mark As Read (api/Notifications/{id}/)

**Mark As Read**

This endpoint executes the `MarkAsRead` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `PUT` /notifications/api/Notifications/read-all

> **Purpose**: Mark All As Read (api/Notifications/)

**Mark All As Read**

This endpoint executes the `MarkAllAsRead` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

## Payments.API

### Checkout Endpoint Catalog

#### `POST` /payments/api/payments/checkout

> **Purpose**: Create Checkout Session (api/payments/)

**Create Checkout Session**

This endpoint executes the `CreateCheckoutSession` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

### Payments Endpoint Catalog

#### `GET` /payments/api/Payments/config/{appId}

> **Purpose**: Get Available Methods (api/Payments/config/)

**Get Available Methods**

This endpoint executes the `GetAvailableMethods` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /payments/api/Payments/config

> **Purpose**: Configure Method (api/Payments/)

**Configure Method**

This endpoint executes the `ConfigureMethod` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "AppId": "sample_string_for_AppId",
    "GatewayName": "Example Name",
    "IsEnabled": true,
    "ConfigJson": "{}"
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /payments/api/Payments/subscribe

> **Purpose**: Subscribe (api/Payments/)

**Subscribe**

This endpoint executes the `Subscribe` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "UserId": "00000000-0000-0000-0000-000000000000",
    "PlanId": "c8d25a4d-decf-41c5-9098-aaf2b2f3072f",
    "Email": "user@example.com",
    "PaymentProvider": "sample_string_for_PaymentProvider",
    "Success": true,
    "Message": "sample_string_for_Message",
    "SubscriptionId": "da87d77d-4212-4549-a3e9-5ec53af426f0",
    "ProviderSubscriptionId": "sample_string_for_ProviderSubscriptionId",
    "RedirectUrl": "https://example.com"
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /payments/api/Payments/analytics?startDate=demo_value&endDate=demo_value

> **Purpose**: Get Analytics (api/Payments/)

**Get Analytics**

This endpoint executes the `GetAnalytics` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `startDate`: demo_value
- `endDate`: demo_value

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /payments/api/Payments/plans

> **Purpose**: Get Plans (api/Payments/)

**Get Plans**

This endpoint executes the `GetPlans` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

### StripeWebhook Endpoint Catalog

#### `POST` /payments/api/webhooks/stripe

> **Purpose**: Handle (api/webhooks/)

**Handle**

This endpoint executes the `Handle` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

## Recommendation.API

## Scheduler.API

## Search.API

## Users.API

### Analytics Endpoint Catalog

#### `GET` /users/api/Analytics/dashboard?startDate=demo_value&endDate=demo_value

> **Purpose**: Get Dashboard Stats (api/Analytics/)

**Get Dashboard Stats**

This endpoint executes the `GetDashboardStats` operation. 

**Authorization Context:**
Requires Role(s): GlobalAdmin GlobalAdmin.

**Authentication:** Bearer Token (`{{jwt_token}}`)

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `startDate`: demo_value
- `endDate`: demo_value

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

### Maintenance Endpoint Catalog

#### `DELETE` /users/api/Maintenance/reset

> **Purpose**: Reset App Data (api/Maintenance/reset/)

**Reset App Data**

This endpoint executes the `ResetAppData` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

### Users Endpoint Catalog

#### `GET` /users/api/Users/profile?userId=demo_value&appId=demo_value

> **Purpose**: Get Profile (api/Users/)

**Get Profile**

This endpoint executes the `GetProfile` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `userId`: demo_value
- `appId`: demo_value

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `PUT` /users/api/Users/profile

> **Purpose**: Update Profile (api/Users/)

**Update Profile**

This endpoint executes the `UpdateProfile` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `Content-Type`: `application/json`
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Sample Request Payload:**
```json
{
    "UserId": "00000000-0000-0000-0000-000000000000",
    "AppId": "33333333-3333-3333-3333-333333333330",
    "DisplayName": "Example Name",
    "Bio": "sample_string_for_Bio",
    "AvatarUrl": "https://example.com",
    "CustomDataJson": "{}",
    "DateOfBirth": "2026-02-23T12:00:00Z",
    "Gender": "sample_string_for_Gender"
}
```

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /users/api/Users/profiles?appId=demo_value

> **Purpose**: Get Profiles (api/Users/)

**Get Profiles**

This endpoint executes the `GetProfiles` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `appId`: demo_value

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /users/api/Users/dashboard/stats?appId=demo_value&startDate=demo_value&endDate=demo_value

> **Purpose**: Get Dashboard Stats (api/Users/dashboard/)

**Get Dashboard Stats**

This endpoint executes the `GetDashboardStats` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `appId`: demo_value
- `startDate`: demo_value
- `endDate`: demo_value

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /users/api/Users/ReportReasons

> **Purpose**: Get Report Reasons (api/Users/)

**Get Report Reasons**

This endpoint executes the `GetReportReasons` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `GET` /users/api/Users/Report?reporterId=demo_value&reportedId=demo_value

> **Purpose**: Get  Reason (api/Users/)

**Get Report Reason**

This endpoint executes the `GetReportReason` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Query Parameters:**
- `reporterId`: demo_value
- `reportedId`: demo_value

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---

#### `POST` /users/api/Users/Report

> **Purpose**: User (api/Users/)

**Report User**

This endpoint executes the `ReportUser` operation. 

**Authorization Context:**
Publicly accessible endpoint (No authentication required).

**Authentication:** None

**Required Headers:**
- `App-Id`: `{{app_id}}`
- `Tenant-Id`: `{{tenant_id}}`

**Expected Successful Response:**
```json
{
    "message": "Success"
}
```

---


