# Recommendation Service (AI/ML)

Provides personalized recommendations and matching logic using ML.NET or external AI models.

## ✨ Features
- **User Matching**: Suggests potential connections based on profile vectors (Matrix Factorization).
- **Candidates API**: Returns ranked users based on scoring.
- **Swipe Logic**: Handles Like/Pass actions and detects mutual matches.
- **Hybrid Approach**: Content-based and Collaborative Filtering.

## 🏗 Technology Stack
- **Framework**: .NET 8 (ASP.NET Core)
- **AI/ML**: Python integration or ML.NET (Experimental)
- **Database**: In-Memory / Stateless (currently)
- **Documentation**: Swagger / OpenAPI

## 🚀 Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 8 SDK

### Running Locally
```bash
cd Recommendation/Recommendation.API
dotnet run
```

### Running via Docker
```bash
docker-compose up -d recommendation-api
```

## 🔌 API Documentation
- **Swagger UI**: http://localhost:5012/swagger
- **Health Check**: http://localhost:5012/health
