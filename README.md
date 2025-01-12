
# Discount Service

A discount code management system consisting of a gRPC server and a web client.

## Prerequisites

- Docker and Docker Compose
- .NET 8.0 (for local development)

## Running the Application

1. Open a terminal in the project root directory
2. Run:
   ```bash
   docker compose build
   ```
   This will build all services
3. Run:
   ```bash
   docker compose up db redis -d
   ```
   This will start the following services in background:
   - MS SQL Server
   - Redis

4. Run:
   ```bash
   docker compose up client server
   ```
   This will start the following services and display their logs:
   - Discount Service (gRPC server)
   - Discount Web Client (Swagger UI)

## Testing the Application

Once all services are running:

1. Open your web browser and navigate to: [http://localhost:30072/swagger](http://localhost:30072/swagger)

2. You can now test the discount code operations through the Swagger UI interface.

## Service Endpoints

- gRPC Server: http://localhost:30175
- Web Client (Swagger UI): http://localhost:30072/swagger

## Stopping the Application

To stop all services:
```bash
docker compose stop
```

To stop all services and remove volumes (this will delete all data):
```bash
docker compose down -v
```
