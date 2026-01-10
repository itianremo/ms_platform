# Media Service (Content Management)

Handles file uploads, storage, and processing.

## Features
- **Object Storage**: Uses MinIO (S3 Compatible) for scalable storage.
- **Image Resizing**: Automatically resizes large images (>1920px) using **ImageSharp**.
- **Moderation**: Mock moderation service flags potentially unsafe content in file metadata.

## Tech Stack
- **.NET 8**
- **MinIO**
- **SixLabors.ImageSharp**
