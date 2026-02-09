# Chat Service

## Overview
The **Chat Service** provides real-time messaging capabilities for the platform, enabling 1-on-1 and group conversations.

## 🚀 Key Features
-   **Real-Time Messaging**: Powered by **SignalR** websockets.
-   **Chat Rooms**: Support for private DMs and group chats.
-   **Message History**: Persistent storage of past conversations.
-   **Presence**: Online/Offline status tracking.
-   **Typing Indicators**: Real-time feedback when a user is typing.

## 🛠️ Tech Stack
-   **.NET 8** (Web API + SignalR)
-   **Redis** (SignalR Backplane for scaling)
-   **SQL Server** (Message History)
-   **MassTransit** (RabbitMQ)
