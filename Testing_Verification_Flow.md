# Testing Verification Flow

Follow these steps to validate the Registration, Verification, and Maintenance Mode logic.

## Pre-requisites
1. **Backend**: Ensure `Apps.API`, `Auth.API`, `Notifications.API`, `Payments.API` (if needed) are running.
2. **Frontend**: Run `npm run dev` in `Frontend/GlobalAdmin`.
3. **Database**: Ensure database is up and seeded (SuperAdmin account should be Active).

---

## Scenario A: Maintenance Mode (No Config)
*Objective: Verify that the system blocks OTP requests when no SMS/Email provider is configured.*

1. **Register User**:
   - Go to `/register`.
   - Create a user (e.g., `test@example.com` / `+1234567890`).
   - *Result*: Redirects to Login. Backend sends "UserRegistered" event (status: PendingVerification).

2. **Login Attempt**:
   - Go to `/login`.
   - Enter credentials.
   - *Result*: Login fails with "Requires Verification", App redirects to `/verify`.

3. **Verify Page**:
   - You should see the Verification Method selection (or OTP input).
   - Click **"Send Code"** (or "Continue" with a method selected).
   - *Result*: **Error Toast** appears: *"Maintenance Mode 1: [Method] Verification System is currently unavailable."*
   - *Result*: No backend logs for sending OTP.

---

## Scenario B: Happy Path (Mock SMS)
*Objective: Verify OTP generation, sending (Mock), and validation.*

1. **Configure SMS (As Admin)**:
   - Login as **SuperAdmin** (or user with permission).
     - *If SuperAdmin is locked out, verify their status in DB `[AuthDb].[dbo].[Users]` is `Active` (2).*
   - Go to **SMS Configs** page (`/sms-configs`).
   - Add/Edit a configuration:
     - **Provider**: `Twilio` (or any name).
     - **Active**: `Yes` (Checked).
     - **Save**.

2. **Retry Verification (As User)**:
   - Go back to `/verify` (or Login again as `test@example.com`).
   - Select **Phone** method (if asked).
   - Click **"Send Code"**.
   - *Result*: **Success Toast** *"OTP sent..."*.
   - *Result*: Check Backend Console (Notifications.Infrastructure): `[Mock Twilio] Sending SMS...`.

3. **Enter OTP**:
   - Use the **Mock OTP**: `1234`.
   - Click **"Verify"**.
   - *Result*: **Success Toast** *"Verification Successful!"*.
   - *Result*: Redirects to `/login`.

4. **Login Success**:
   - Login as `test@example.com`.
   - *Result*: Login Successful! Dashboard loads.

---

## Troubleshooting

- **OTP Invalid?**: Ensure you are entering `1234` (Mock) and not waiting > 5 minutes.
- **Still "Maintenance Mode"?**: 
  - Check `[NotificationsDb].[dbo].[NotificationConfigs]`.
  - Ensure there is a row with `Type='Sms'` and `IsActive=1`.
- **Database Status**:
  - Check `[AuthDb].[dbo].[UserOtps]` to see generated codes.
  - Check `[AuthDb].[dbo].[Users]` to see `Status` changes (e.g., `2` = PendingMobileVerification, `3` = AccountVerified/Active).
