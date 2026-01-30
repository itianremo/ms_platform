# Media Service (File Management)

Handles file uploads, storage, and media processing (resizing, moderation).

## ✨ Features
- **Object Storage**: Uses MinIO (S3-compatible) for file storage.
- **Image Processing**: Automatic resizing and format conversion.
- **Moderation**: Integration hooks for content moderation.
- **Secure Access**: Presigned URLs for secure file sharing.

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **Storage**: MinIO (S3 Compatible)
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Media/Media.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d media-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5005/swagger
- **Health Check**: http://localhost:5005/health
