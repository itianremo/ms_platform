# Search Service

## Overview
Provides text-based search capabilities across Users, Apps, and Content.

## Tech Stack
- **Framework**: .NET 8 (ASP.NET Core Web API)
- **Engine**: Elasticsearch / Solr / SQL Full-Text Search
- **Messaging**: RabbitMQ (MassTransit) - Listens for data updates to index.

## Key Features
- **Indexing**: Real-time indexing of User profiles and App metadata.
- **Querying**: Fuzzy search, filtering, and facets.
- **AutoComplete**: Type-ahead suggestions.

## API Documentation
Swagger UI: http://localhost:5011/swagger (via Gateway: http://localhost:5000/search/swagger)
