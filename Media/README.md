# Media Service

## Overview
Handles file uploads, storage, and media processing (images, videos).

## Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Storage**: Local Filesystem / AWS S3 / Azure Blob Storage (Configurable)
- **Database**: SQL Server (Metadata)

## Key Features
- **Upload**: Secure file upload endpoints (User Avatars, App Icons, Content).
- **Optimization**: Image resizing and compression.
- **Streaming**: Video streaming support (if applicable).
- **CDN**: Integration with CDN for fast delivery.

## API Documentation
Swagger UI: http://localhost:5009/swagger (via Gateway: http://localhost:5000/media/swagger)
