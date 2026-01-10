# Search Service (Discoverability)

Enables users to search for other profiles across the platform.

## Features
- **User Index**: Maintains a query-optimized copy of User Profiles.
- **Synchronization**: Listens for `UserProfileCreated` and `UserProfileUpdated` events from the Users Service to keep the index fresh.
- **Full-Text Search**: Uses PostgreSQL FTS (or accessible via API).

## Tech Stack
- **.NET 8**
- **PostgreSQL**
- **MassTransit**
