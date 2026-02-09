# Search Service

## Overview
The **Search Service** powers the discovery features of the platform, allowing efficient querying of users, content, and other entities.

## 🚀 Key Features
-   **Full-Text Search**: Optimized for complex queries and keyword matching.
-   **Filtering**: Advanced facet-based filtering (e.g., by location, age, interests).
-   **Geo-Search**: specific queries for "Users near me".
-   **Syncing**: Listens to domain events to keep the search index eventually consistent with the primary databases.

## 📡 Event Architecture
### Consumes
-   `UserProfileCreatedEvent`: Indexes a new user.
-   `UserProfileUpdatedEvent`: Updates existing index.
-   `PostCreatedEvent`: Indexes new content (if applicable).

## 🛠️ Tech Stack
-   **.NET 8** (Web API)
-   **ElasticSearch / OpenSearch**
-   **MassTransit** (RabbitMQ)
