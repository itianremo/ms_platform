# Chat Service (Real-time Messaging)

Provides real-time messaging capabilities and message history.

## Features
- **Real-time Chat**: SignalR Hub (`/chatHub`) for instant message delivery.
- **Persistence**: Stores all messages in MongoDB.
- **Moderation Integration**: Filters messages containing banned words via `ITextModerationService`.
- **History API**: Retrieve past conversations.

## Tech Stack
- **.NET 8**
- **MongoDB**
- **SignalR**
