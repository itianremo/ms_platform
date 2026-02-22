# Gateway API Endpoints Documentation

This document outlines all the endpoints exposed through the API Gateway, detailing the URL paths, required HTTP methods, required headers (such as `App-Id` and `Tenant-Id`), authentication requirements, role/policy permissions, request parameters, and output samples.

## Service: Apps.API

### Apps Controller

#### `GET` /apps/api/Apps/{id}

**Gateway URL:** `{{gateway_url}}/apps/api/Apps/{id}`

**Description / Permissions:** Method: GetAppById. Authentication required: Yes. Permissions: Policy = "ManageApps".

**Authentication:** Bearer Token Required

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `PUT` /apps/api/Apps/{id}

**Gateway URL:** `{{gateway_url}}/apps/api/Apps/{id}`

**Description / Permissions:** Method: UpdateApp. Authentication required: Yes. Permissions: Policy = "ManageApps".

**Authentication:** Bearer Token Required

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "Id": "23cf3c83-47b1-4ff7-bae7-0bbacf431196",
    "Name": "sample Name",
    "Description": "sample Description",
    "BaseUrl": "https://example.com",
    "ThemeJson": "{}"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `PATCH` /apps/api/Apps/{id}/status

**Gateway URL:** `{{gateway_url}}/apps/api/Apps/{id}/status`

**Description / Permissions:** Method: ToggleStatus. Authentication required: Yes. Permissions: Policy = "ManageApps".

**Authentication:** Bearer Token Required

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "Id": "6335c296-7ce9-4481-81c7-420f2a0ffef9",
    "IsActive": true
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `PATCH` /apps/api/Apps/{id}/external-auth

**Gateway URL:** `{{gateway_url}}/apps/api/Apps/{id}/external-auth`

**Description / Permissions:** Method: UpdateExternalAuth. Authentication required: Yes. Permissions: Policy = "ManageApps".

**Authentication:** Bearer Token Required

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "Id": "c5a96e5d-d5bc-4534-81ff-90f709554690",
    "ExternalLoginsJson": "{}"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /apps/api/Apps/{id}/packages

**Gateway URL:** `{{gateway_url}}/apps/api/Apps/{id}/packages`

**Description / Permissions:** Method: GetPackages. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /apps/api/Apps/{appId}/users/{userId}/subscriptions

**Gateway URL:** `{{gateway_url}}/apps/api/Apps/{appId}/users/{userId}/subscriptions`

**Description / Permissions:** Method: GetUserSubscriptions. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /apps/api/Apps/{appId}/users/{userId}/subscriptions

**Gateway URL:** `{{gateway_url}}/apps/api/Apps/{appId}/users/{userId}/subscriptions`

**Description / Permissions:** Method: GrantSubscription. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "UserId": "00000000-0000-0000-0000-000000000000",
    "AppId": "33333333-3333-3333-3333-333333333330",
    "PackageId": "3f3aa53f-bf5e-4ffe-ac48-e427bc3e7209",
    "StartDate": "2026-02-23T00:51:25.277223",
    "EndDate": "2026-02-23T00:51:25.277248"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `PUT` /apps/api/Apps/{appId}/subscriptions/{id}/status

**Gateway URL:** `{{gateway_url}}/apps/api/Apps/{appId}/subscriptions/{id}/status`

**Description / Permissions:** Method: ChangeSubscriptionStatus. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "SubscriptionId": "418e3ee0-cd29-4b37-8227-85b16d7d5c54",
    "IsActive": true
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

## Service: Audit.API

### Audit Controller

#### `GET` /audit/api/Audit/stats?days=<insert value>

**Gateway URL:** `{{gateway_url}}/audit/api/Audit/stats?days=<insert value>`

**Description / Permissions:** Method: GetStats. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `days`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

## Service: Auth.API

### Analytics Controller

#### `GET` /auth/api/Analytics/app-user-stats

**Gateway URL:** `{{gateway_url}}/auth/api/Analytics/app-user-stats`

**Description / Permissions:** Method: GetAppUserStats. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

### Auth Controller

#### `POST` /auth/api/Auth/register

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/register`

**Description / Permissions:** Method: Register. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "Email": "test@example.com",
    "Phone": "+1234567890",
    "Password": "Password123!",
    "AppId": "33333333-3333-3333-3333-333333333330",
    "VerificationType": "Type: VerificationType",
    "RequiresAdminApproval": true
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /auth/api/Auth/login

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/login`

**Description / Permissions:** Method: Login. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "email": "test@example.com",
    "password": "Password123!",
    "appId": "33333333-3333-3333-3333-333333333330"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /auth/api/Auth/otp/request

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/otp/request`

**Description / Permissions:** Method: RequestOtp. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "Email": "test@example.com",
    "Type": "Type: VerificationType"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /auth/api/Auth/otp/verify

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/otp/verify`

**Description / Permissions:** Method: VerifyOtp. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "Email": "test@example.com",
    "Code": "sample Code",
    "Type": "Type: VerificationType"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /auth/api/Auth/password/forgot

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/password/forgot`

**Description / Permissions:** Method: ForgotPassword. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "Email": "test@example.com"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /auth/api/Auth/password/reset

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/password/reset`

**Description / Permissions:** Method: ResetPassword. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "Email": "test@example.com",
    "Code": "sample Code",
    "NewPassword": "Password123!"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /auth/api/Auth/reactivate/initiate

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/reactivate/initiate`

**Description / Permissions:** Method: InitiateReactivation. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "OldEmail": "test@example.com",
    "NewEmail": "test@example.com"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /auth/api/Auth/reactivate/verify

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/reactivate/verify`

**Description / Permissions:** Method: VerifyReactivation. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "Email": "test@example.com",
    "Code": "sample Code"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /auth/api/Auth/refresh

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/refresh`

**Description / Permissions:** Method: Refresh. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /auth/api/Auth/sessions

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/sessions`

**Description / Permissions:** Method: GetSessions. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `PUT` /auth/api/Auth/users/{id}/status

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/users/{id}/status`

**Description / Permissions:** Method: UpdateUserStatus. Authentication required: Yes. Permissions: Roles = "SuperAdmin,UsersManager".

**Authentication:** Bearer Token Required

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "error": "Could not parse type UpdateUserStatusRequest"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `PUT` /auth/api/Auth/users/{id}/verify

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/users/{id}/verify`

**Description / Permissions:** Method: UpdateUserVerification. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "error": "Could not parse type UpdateUserVerificationRequest"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /auth/api/Auth/users/{id}/apps

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/users/{id}/apps`

**Description / Permissions:** Method: AddUserToApp. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "error": "Could not parse type AddUserToAppRequest"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `DELETE` /auth/api/Auth/users/{id}/apps/{appId}

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/users/{id}/apps/{appId}`

**Description / Permissions:** Method: RemoveUserFromApp. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `PUT` /auth/api/Auth/users/{id}/apps/{appId}/status

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/users/{id}/apps/{appId}/status`

**Description / Permissions:** Method: UpdateAppStatus. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "error": "Could not parse type UpdateAppStatusRequest"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `PUT` /auth/api/Auth/users/{id}/apps/{appId}/role

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/users/{id}/apps/{appId}/role`

**Description / Permissions:** Method: AssignRole. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "error": "Could not parse type AssignRoleRequest"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `DELETE` /auth/api/Auth/sessions/{sessionId}

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/sessions/{sessionId}`

**Description / Permissions:** Method: RevokeSession. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /auth/api/Auth/users?appId=<insert value>

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/users?appId=<insert value>`

**Description / Permissions:** Method: GetAllUsers. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `appId`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /auth/api/Auth/logout

**Gateway URL:** `{{gateway_url}}/auth/api/Auth/logout`

**Description / Permissions:** Method: Logout. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

### ExternalAuth Controller

#### `GET` /auth/api/ExternalAuth/login/{provider}

**Gateway URL:** `{{gateway_url}}/auth/api/ExternalAuth/login/{provider}`

**Description / Permissions:** Method: Callback. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /auth/api/ExternalAuth/callback

**Gateway URL:** `{{gateway_url}}/auth/api/ExternalAuth/callback`

**Description / Permissions:** Method: Callback. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /auth/api/ExternalAuth/link/{provider}

**Gateway URL:** `{{gateway_url}}/auth/api/ExternalAuth/link/{provider}`

**Description / Permissions:** Method: CallbackLink. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /auth/api/ExternalAuth/link-callback

**Gateway URL:** `{{gateway_url}}/auth/api/ExternalAuth/link-callback`

**Description / Permissions:** Method: CallbackLink. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `DELETE` /auth/api/ExternalAuth/users/{userId}/links/{provider}

**Gateway URL:** `{{gateway_url}}/auth/api/ExternalAuth/users/{userId}/links/{provider}`

**Description / Permissions:** Method: Unlink. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

### Maintenance Controller

#### `DELETE` /auth/api/Maintenance/reset/{appId}

**Gateway URL:** `{{gateway_url}}/auth/api/Maintenance/reset/{appId}`

**Description / Permissions:** Method: ResetAppData. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

### Permissions Controller

#### `GET` /auth/api/Permissions/check?userId=<insert value>&appId=<insert value>&permission=<insert value>

**Gateway URL:** `{{gateway_url}}/auth/api/Permissions/check?userId=<insert value>&appId=<insert value>&permission=<insert value>`

**Description / Permissions:** Method: CheckPermission. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `userId`
- `appId`
- `permission`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

### Roles Controller

#### `POST` /auth/api/Roles/assign

**Gateway URL:** `{{gateway_url}}/auth/api/Roles/assign`

**Description / Permissions:** Method: AssignRole. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "UserId": "00000000-0000-0000-0000-000000000000",
    "RoleId": "c728265f-e32e-4c03-aaf9-eab582b5a105",
    "AppId": "33333333-3333-3333-3333-333333333330"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

## Service: Chat.API

### Chat Controller

#### `GET` /chat/api/Chat/history?senderId=<insert value>&recipientId=<insert value>

**Gateway URL:** `{{gateway_url}}/chat/api/Chat/history?senderId=<insert value>&recipientId=<insert value>`

**Description / Permissions:** Method: GetHistory. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `senderId`
- `recipientId`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /chat/api/Chat/flagged?appId=<insert value>

**Gateway URL:** `{{gateway_url}}/chat/api/Chat/flagged?appId=<insert value>`

**Description / Permissions:** Method: GetFlaggedMessages. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `appId`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /chat/api/Chat/channels

**Gateway URL:** `{{gateway_url}}/chat/api/Chat/channels`

**Description / Permissions:** Method: CreateChannel. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "error": "Could not parse type CreateChannelRequest"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

## Service: Geo.API

### Geo Controller

#### `GET` /geo/api/Geo/nearby?appId=<insert value>&lat=<insert value>&lon=<insert value>&radiusKm=<insert value>

**Gateway URL:** `{{gateway_url}}/geo/api/Geo/nearby?appId=<insert value>&lat=<insert value>&lon=<insert value>&radiusKm=<insert value>`

**Description / Permissions:** Method: GetNearby. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `appId`
- `lat`
- `lon`
- `radiusKm`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /geo/api/Geo/location

**Gateway URL:** `{{gateway_url}}/geo/api/Geo/location`

**Description / Permissions:** Method: UpdateLocation. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "UserId": 0,
    "AppId": 0,
    "Latitude": 10.99,
    "Longitude": 10.99
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

### Lookups Controller

#### `GET` /geo/api/Lookups/countries

**Gateway URL:** `{{gateway_url}}/geo/api/Lookups/countries`

**Description / Permissions:** Method: GetCountries. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /geo/api/Lookups/cities/{countryId}

**Gateway URL:** `{{gateway_url}}/geo/api/Lookups/cities/{countryId}`

**Description / Permissions:** Method: GetCities. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

## Service: Media.API

### Media Controller

#### `POST` /media/api/Media/upload

**Gateway URL:** `{{gateway_url}}/media/api/Media/upload`

**Description / Permissions:** Method: Upload. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /media/api/Media/flagged

**Gateway URL:** `{{gateway_url}}/media/api/Media/flagged`

**Description / Permissions:** Method: GetUrl. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /media/api/Media/{id}/url

**Gateway URL:** `{{gateway_url}}/media/api/Media/{id}/url`

**Description / Permissions:** Method: GetUrl. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

## Service: Notifications.API

### Config Controller

#### `POST` /notifications/api/Config/test-sms

**Gateway URL:** `{{gateway_url}}/notifications/api/Config/test-sms`

**Description / Permissions:** Method: TestSms. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "error": "Could not parse type TestSmsRequest"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /notifications/api/Config/test-email

**Gateway URL:** `{{gateway_url}}/notifications/api/Config/test-email`

**Description / Permissions:** Method: TestEmail. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "error": "Could not parse type TestEmailRequest"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

### Notifications Controller

#### `GET` /notifications/api/Notifications/unread

**Gateway URL:** `{{gateway_url}}/notifications/api/Notifications/unread`

**Description / Permissions:** Method: GetUnread. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `PUT` /notifications/api/Notifications/{id}/read

**Gateway URL:** `{{gateway_url}}/notifications/api/Notifications/{id}/read`

**Description / Permissions:** Method: MarkAsRead. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `PUT` /notifications/api/Notifications/read-all

**Gateway URL:** `{{gateway_url}}/notifications/api/Notifications/read-all`

**Description / Permissions:** Method: MarkAllAsRead. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

## Service: Payments.API

### Checkout Controller

#### `POST` /payments/api/payments/checkout

**Gateway URL:** `{{gateway_url}}/payments/api/payments/checkout`

**Description / Permissions:** Method: CreateCheckoutSession. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "error": "Could not parse type CheckoutRequest"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

### Payments Controller

#### `GET` /payments/api/Payments/config/{appId}

**Gateway URL:** `{{gateway_url}}/payments/api/Payments/config/{appId}`

**Description / Permissions:** Method: GetAvailableMethods. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /payments/api/Payments/config

**Gateway URL:** `{{gateway_url}}/payments/api/Payments/config`

**Description / Permissions:** Method: ConfigureMethod. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "AppId": "sample AppId",
    "GatewayName": "sample GatewayName",
    "IsEnabled": true,
    "ConfigJson": "{}"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /payments/api/Payments/subscribe

**Gateway URL:** `{{gateway_url}}/payments/api/Payments/subscribe`

**Description / Permissions:** Method: Subscribe. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "UserId": "00000000-0000-0000-0000-000000000000",
    "PlanId": "425fb6ed-8400-4e6d-9df2-abbcf345e055",
    "Email": "test@example.com",
    "PaymentProvider": "sample PaymentProvider",
    "Success": true,
    "Message": "sample Message",
    "SubscriptionId": "b04563aa-9545-4207-81f0-13100530c158",
    "ProviderSubscriptionId": "sample ProviderSubscriptionId",
    "RedirectUrl": "https://example.com"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /payments/api/Payments/analytics?startDate=<insert value>&endDate=<insert value>

**Gateway URL:** `{{gateway_url}}/payments/api/Payments/analytics?startDate=<insert value>&endDate=<insert value>`

**Description / Permissions:** Method: GetAnalytics. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `startDate`
- `endDate`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /payments/api/Payments/plans

**Gateway URL:** `{{gateway_url}}/payments/api/Payments/plans`

**Description / Permissions:** Method: GetPlans. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

### StripeWebhook Controller

#### `POST` /payments/api/webhooks/stripe

**Gateway URL:** `{{gateway_url}}/payments/api/webhooks/stripe`

**Description / Permissions:** Method: Handle. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

## Service: Recommendation.API

## Service: Scheduler.API

## Service: Search.API

## Service: Users.API

### Analytics Controller

#### `GET` /users/api/Analytics/dashboard?startDate=<insert value>&endDate=<insert value>

**Gateway URL:** `{{gateway_url}}/users/api/Analytics/dashboard?startDate=<insert value>&endDate=<insert value>`

**Description / Permissions:** Method: GetDashboardStats. Authentication required: Yes. Permissions: Roles = "GlobalAdmin") (Roles = "GlobalAdmin".

**Authentication:** Bearer Token Required

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `startDate`
- `endDate`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

### Maintenance Controller

#### `DELETE` /users/api/Maintenance/reset/{appId}

**Gateway URL:** `{{gateway_url}}/users/api/Maintenance/reset/{appId}`

**Description / Permissions:** Method: ResetAppData. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

### Users Controller

#### `GET` /users/api/Users/profile?userId=<insert value>&appId=<insert value>

**Gateway URL:** `{{gateway_url}}/users/api/Users/profile?userId=<insert value>&appId=<insert value>`

**Description / Permissions:** Method: GetProfile. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `userId`
- `appId`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `PUT` /users/api/Users/profile

**Gateway URL:** `{{gateway_url}}/users/api/Users/profile`

**Description / Permissions:** Method: UpdateProfile. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "UserId": "00000000-0000-0000-0000-000000000000",
    "AppId": "33333333-3333-3333-3333-333333333330",
    "DisplayName": "sample DisplayName",
    "Bio": "sample Bio",
    "AvatarUrl": "https://example.com",
    "CustomDataJson": "{}",
    "DateOfBirth": "2026-02-23T00:51:33.250732",
    "Gender": "sample Gender"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /users/api/Users/profiles?appId=<insert value>

**Gateway URL:** `{{gateway_url}}/users/api/Users/profiles?appId=<insert value>`

**Description / Permissions:** Method: GetProfiles. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `appId`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /users/api/Users/dashboard/stats?appId=<insert value>&startDate=<insert value>&endDate=<insert value>

**Gateway URL:** `{{gateway_url}}/users/api/Users/dashboard/stats?appId=<insert value>&startDate=<insert value>&endDate=<insert value>`

**Description / Permissions:** Method: GetDashboardStats. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `appId`
- `startDate`
- `endDate`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /users/api/Users/ReportReasons

**Gateway URL:** `{{gateway_url}}/users/api/Users/ReportReasons`

**Description / Permissions:** Method: GetReportReasons. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `GET` /users/api/Users/Report?reporterId=<insert value>&reportedId=<insert value>

**Gateway URL:** `{{gateway_url}}/users/api/Users/Report?reporterId=<insert value>&reportedId=<insert value>`

**Description / Permissions:** Method: GetReportReason. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Query Parameters:**
- `reporterId`
- `reportedId`

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

#### `POST` /users/api/Users/Report

**Gateway URL:** `{{gateway_url}}/users/api/Users/Report`

**Description / Permissions:** Method: ReportUser. Authentication required: No. Permissions: None.

**Authentication:** None

**Headers:**
- `Content-Type`: application/json
- `App-Id`: {{app_id}}
- `Tenant-Id`: {{tenant_id}}

**Request Body:**
```json
{
    "error": "Could not parse type ReportUserRequest"
}
```

**Sample Output:**
```json
{
    "message": "Action completed successfully."
}
```

---

