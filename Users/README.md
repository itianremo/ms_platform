# Users Service (Profile Management)

Handles extended user profile data, settings, and efficient profile retrieval.

## Features
- **Profile Management**: Bio, Age, Gender, Interests.
- **Synchronization**: Publishes `UserProfileCreated` events for Search/Recommendation services.
- **Isolation**: Decoupled from Auth credentials.

## Tech Stack
- **.NET 8**
- **PostgreSQL**
- **MassTransit** (Event Bus)
