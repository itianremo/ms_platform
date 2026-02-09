# Media Service

## Overview
The **Media Service** is responsible for handling all file uploads, storage, and retrieval within the platform. It supports images, videos, and documents, ensuring secure and optimized delivery.

## 🚀 Key Features
-   **File Upload**: Secure endpoints for uploading user avatars, post images, and documents.
-   **Storage Abstraction**: Configurable to use local storage (Dev) or **MinIO / AWS S3** (Prod).
-   **Image Processing**: Resizing and optimization of images on upload.
-   **Public/Private Access**: Control over who can view specific media assets.

## 🛠️ Tech Stack
-   **.NET 8** (Web API)
-   **MinIO / S3 Compatible Storage**
-   **MassTransit** (RabbitMQ)
