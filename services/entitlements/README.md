# Entitlements Service

A minimal Entitlements API stub for the PHOS Marketplace that manages user entitlements for applications.

## What it is

This is a simple entitlements service that provides:
- Granting entitlements to users for specific applications
- Checking if users have active entitlements
- Managing entitlement expiration with TTL (Time To Live)
- In-memory storage (no database required)

## How to run

```bash
dotnet run --project services/entitlements
```

The service will start on `http://localhost:5180` (HTTP) and `https://localhost:7180` (HTTPS).

## API Endpoints

### Health Check
```bash
curl http://localhost:5180/healthz
```

### Service Info
```bash
curl http://localhost:5180/api/info
```

### Grant Entitlement
```bash
curl -X POST http://localhost:5180/entitlements \
  -H "content-type: application/json" \
  -d '{"userId":"u1","appId":"app.demo","sku":"pro","ttlDays":7}'
```

### List Entitlements
```bash
curl http://localhost:5180/entitlements
```

### Check Entitlement
```bash
curl -X POST http://localhost:5180/entitlements/check \
  -H "content-type: application/json" \
  -d '{"userId":"u1","appId":"app.demo","sku":"pro"}'
```

## Development

- Swagger UI is available at `/swagger` when running in Development mode
- No authentication or external dependencies required
- Uses in-memory storage with `ConcurrentDictionary`
- Default TTL is 30 days if not specified
