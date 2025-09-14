# Review Policy Service

A minimal .NET 8 API for handling app submission reviews and policy checks.

## What it is

This is a stub implementation of the Review & Policy API that handles app submissions for review. It provides endpoints for creating submissions, retrieving submission details, and listing all submissions. The service uses in-memory storage and is designed for development and testing purposes.

## How to run

```bash
dotnet run --project services/review-policy
```

The service will start on `http://localhost:5170` (HTTP) and `https://localhost:7170` (HTTPS).

## API Endpoints

- `GET /healthz` - Health check endpoint
- `GET /api/info` - Service information
- `GET /submissions` - List all submissions
- `POST /submissions` - Create a new submission
- `GET /submissions/{id}` - Get a specific submission

## Example Usage

### Health Check
```bash
curl http://localhost:5170/healthz
```

### Service Info
```bash
curl http://localhost:5170/api/info
```

### Create Submission
```bash
curl -X POST http://localhost:5170/submissions \
  -H "content-type: application/json" \
  -d '{"appId":"app.demo","version":"1.0.0","notes":"Initial release"}'
```

### List Submissions
```bash
curl http://localhost:5170/submissions
```

### Get Specific Submission
```bash
curl http://localhost:5170/submissions/{submission-id}
```

## Development

Swagger UI is available at `http://localhost:5170/swagger` when running in Development mode.
