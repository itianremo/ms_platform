# Geo Service

## Overview
Provides location-based services, geocoding, and distance calculations for the FitIT Platform.

## Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Database**: SQL Server (Spatial Types) or Redis (Geo)
- **External API**: Google Maps API / Mapbox (if applicable)

## Key Features
- **Geocoding**: Convert addresses to coordinates.
- **Reverse Geocoding**: Convert coordinates to addresses.
- **Search**: Find Gyms/Trainers within a radius.
- **Validation**: Verify address headers.

## API Documentation
Swagger UI: http://localhost:5008/swagger (via Gateway: http://localhost:5000/geo/swagger)
