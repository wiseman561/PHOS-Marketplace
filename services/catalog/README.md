# Catalog Service

A minimal .NET 8 API service for the PHOS Marketplace catalog functionality.

## What it is

This is a stub implementation of the Catalog API that provides basic endpoints for managing marketplace apps. It's designed to be self-contained with no external dependencies like databases or authentication.

## How to run

```bash
dotnet run --project services/catalog
```

The service will start on:
- HTTP: http://localhost:5160
- HTTPS: https://localhost:7160

Swagger UI is available at `/swagger` in development mode.

## API Endpoints

### Health Check
```bash
curl http://localhost:5160/healthz
```
Returns: `{ "status": "ok" }`

### Service Info
```bash
curl http://localhost:5160/api/info
```
Returns: `{ "name": "catalog", "version": "0.1.0" }`

### List Apps
```bash
curl http://localhost:5160/apps
```
Returns: `[]`

### Create App
```bash
curl -X POST http://localhost:5160/apps \
  -H "content-type: application/json" \
  -d '{"name":"Test","description":"A test app"}'
```
Returns: `{ "id": "<guid>", "name": "Test", "description": "A test app" }`

### Get App
```bash
curl http://localhost:5160/apps/demo
```
Returns: `{ "id": "demo", "name": "Demo App", "description": "placeholder" }`

For any other ID, returns 404: `{ "error": "not_found" }`
