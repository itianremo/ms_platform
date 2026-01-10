# API Gateway (YARP)

The single entry point for all frontend client requests. Routes traffic to appropriate microservices.

## Features
- **Reverse Proxy**: Built with **YARP** (Yet Another Reverse Proxy).
- **Routing**: Maps `/api/auth` -> `AuthService`, `/api/chat` -> `ChatService`, etc.
- **Load Balancing**: Capable of distributing load (configured for single instances in Dev).

## Tech Stack
- **.NET 8**
- **YARP**
