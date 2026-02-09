# Users Service

## Overview
The **Users Service** manages the extended user profile, preferences, and personal information. While `Auth Service` handles credentials, `Users Service` handles the "Person" behind the account.

## 🚀 Key Features
-   **Profile Management**: Display Name, Avatar, Bio, Date of Birth, Gender.
-   **App Preferences**: Stores JSON-based settings per app (e.g., Theme: Dark, Sidebar: Collapsed).
-   **Fast Lookups**: Optimized for retrieving user details for UI display.
-   **Search Sync**: Pushes profile updates to the `Search Service` for discoverability.

## 📡 Event Architecture
### Consumes
-   `UserRegisteredEvent`: Automatically creates a base profile for the new user.
-   `UserContactUpdatedEvent`: Updates contact details if mirrored in profile.

### Publishes
-   `UserProfileCreatedEvent`: Notifies Search service to index the new profile.
-   `UserProfileUpdatedEvent`: Notifies Search service to update the index.

## 🛠️ Tech Stack
-   **.NET 8** (Web API)
-   **SQL Server** (Profile Data)
-   **MassTransit** (RabbitMQ)
