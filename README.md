# AI-Powered E-Learning API

## Overview
This project is a RESTful backend API for an AI-powered e-learning platform designed to enhance student interaction through AI-assisted features. The system integrates ChatGPT to support interactive learning workflows.

## Key Features
- RESTful API built with ASP.NET Core
- AI integration for question answering
- Modular backend structure (Controllers, Services, DTOs)
- MongoDB for data storage
- Clear separation of concerns using a three-layer architecture

## Tech Stack
- ASP.NET Core
- MongoDB
- ChatGPT API
- Swagger (API Documentation)

## Architecture
The backend follows a layered architecture:
- Controllers: Handle HTTP requests
- Services: Business logic
- Data / Models: Database interaction and entities

## API Documentation
Swagger UI is available after running the project locally.

## How to Run Locally
1. Clone the repository
2. Configure MongoDB connection string in `appsettings.json`
3. Run the project using Visual Studio or `dotnet run`
4. Access Swagger UI via `/swagger`

## Testing
- Manual testing performed using Swagger to validate API endpoints.
- Tested different input scenarios to verify system behavior.
- Planned improvements include adding automated tests (xUnit).

## Future Improvements
- Add automated unit tests
- Improve input validation and error handling
- Expand AI features
