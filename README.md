# Partner Integration BFF

A .NET 8 Backend-for-Frontend (BFF) microservice that receives incoming transaction data from third-party partners, validates it, verifies the partner identity via an external API, and reliably queues it for downstream legacy systems to process.

---

## Table of Contents

- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Key Design Decisions](#key-design-decisions)
- [Prerequisites](#prerequisites)
- [How to Run](#how-to-run)
- [How to Run Tests](#how-to-run-tests)
- [API Reference](#api-reference)
- [Configuration](#configuration)

---

## Architecture

The solution follows **Clean Architecture** with three layers, each with a single responsibility and a strict dependency rule — outer layers depend on inner layers, never the reverse.

```
PartnerBFF.API              → HTTP entry point, controllers, middleware, DI wiring
PartnerBFF.Application      → Business logic, interfaces, models, validation, exceptions
PartnerBFF.Infrastructure   → Concrete implementations (RabbitMQ, HTTP clients, policies)
```

### Request Flow

```
POST /api/v1/partner/transactions
        │
        ▼
TransactionController
        │
        ▼
ASP.NET Core Model Binding
  [PositiveAmount], [AllowedCurrency], [AllowedTimestamp], [Required]
  → 400 Bad Request if any rule fails
        │
        ▼
TransactionService
        │
        ├── IPartnerVerifierService.VerifyPartnerAsync()
        │       └── HTTP call to MockPartnerVerificationController
        │               └── PartnerVerifierPolicy (Polly retry + circuit breaker)
        │                       30% chance TimeoutException → retried up to 3 times
        │
        └── IMessagePublisherBroker.PublishAsync()
                └── MessagePublisherBroker
                        └── RabbitMqPublisher
                                └── RabbitMqRetryPolicy (Polly retry on publish failure)
                                        └── partner.exchange → partner.transactions queue
```

### Messaging — Broker Pattern

`MessagePublisherBroker` sits between `TransactionService` and the concrete publishers. It holds `IEnumerable<IMessagePublisher>` injected by DI, so adding a new broker (Kafka, Azure Service Bus) requires only a new `IMessagePublisher` implementation and one DI registration — zero changes to service logic.

```
TransactionService
  injects → IMessagePublisherBroker
                    │
                    ▼
          MessagePublisherBroker     ← lives in Application
            injects → IEnumerable<IMessagePublisher>
                              │
                              ▼
                      RabbitMqPublisher  ← lives in Infrastructure
```

---

## Project Structure

```
PartnerBFF/
├── docker-compose.yml
├── dockerfile
│
├── PartnerBFF.API/
│   ├── Controllers/
│   │   ├── TransactionController.cs              ← POST /api/v1/partner/transactions
│   │   └── MockPartnerVerificationController.cs  ← mock verify endpoint (same project)
│   ├── Middlewares/
│   │   └── GlobalExceptionHandler.cs             ← formats all exceptions consistently
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Program.cs
│
├── PartnerBFF.Application/
│   ├── Interfaces/
│   │   ├── ITransactionService.cs
│   │   ├── IMessagePublisherBroker.cs
│   │   └── IPartnerVerifierService.cs
│   ├── Messaging/
│   │   └── MessagePublisherBroker.cs             ← broker pattern coordinator
│   ├── Models/
│   │   ├── ErrorResponse.cs
│   │   ├── TransactionMessage.cs
│   │   ├── TransactionStatusEnum.cs
│   │   ├── Requests/
│   │   │   └── TransactionRequest.cs
│   │   └── Responses/
│   │       ├── PartnerVerificationResponse.cs
│   │       └── TransactionResponse.cs
│   ├── Services/
│   │   ├── TransactionService.cs
│   │   └── PartnerVerifierService.cs
│   ├── ValidationAttributes/
│   │   ├── AllowedCurrencyAttribute.cs           ← ISO 4217 currency validation
│   │   ├── AllowedTimestampAttribute.cs          ← not default, not in future
│   │   └── PositiveAmountAttribute.cs            ← amount > 0
│   ├── Exceptions/
│   │   ├── BaseException.cs
│   │   ├── TransactionValidationException.cs
│   │   ├── PartnerVerificationException.cs
│   │   └── MessagePublishException.cs
│   └── Constants/
│       └── CurrencyCodes.cs
│
├── PartnerBFF.Infrastructure/
│   ├── Interfaces/
│   │   ├── IMessagePublisher.cs                  ← contract for concrete publishers
│   │   └── IRabbitMqConnectionFactory.cs
│   ├── Messaging/RabbitMQ/
│   │   ├── RabbitMqPublisher.cs                  ← implements IMessagePublisher
│   │   └── RabbitMqConnectionFactory.cs
│   ├── Policies/
│   │   ├── PartnerVerifierPolicy.cs              ← Polly retry + circuit breaker
│   │   └── RabbitMqRetryPolicy.cs                ← Polly retry for publish failures
│   └── Configurations/
│       ├── RabbitMqSettings.cs
│       └── PartnerVerificationSettings.cs
│
└── PartnerBFF.Application.Test/
    ├── Helpers/
    │   └── MockHttpMessageHandler.cs
    ├── Models/
    │   └── TransactionRequestTests.cs            ← Data Annotation validation tests
    └── Services/
        ├── TransactionServiceTests.cs
        └── PartnerVerifierServiceTests.cs
```

---

## Key Design Decisions

### 1. Clean Architecture / Dependency Inversion
`Application` has zero infrastructure dependencies — it defines interfaces (`IMessagePublisherBroker`, `IPartnerVerifierService`) and `Infrastructure` implements them. All business logic is independently testable without spinning up RabbitMQ or HTTP servers.

### 2. Broker Pattern for Messaging
`MessagePublisherBroker` in the Application layer holds all registered `IMessagePublisher` implementations injected via `IEnumerable<IMessagePublisher>`. `TransactionService` only calls `IMessagePublisherBroker` — it has no knowledge of RabbitMQ or any specific broker. Adding a new broker is a one-line DI registration.

### 3. `IMessagePublisher` in Infrastructure, `IMessagePublisherBroker` in Application
This separation avoids circular DI. The broker depends on `IEnumerable<IMessagePublisher>` — if both interfaces were registered under the same type, DI would attempt to resolve the broker as its own dependency.

### 4. Data Annotations for Validation
All request validation lives in custom `ValidationAttribute` classes on `TransactionRequest`. ASP.NET Core model binding runs these automatically before the action executes — `TransactionService` receives a guaranteed-valid object and needs no re-validation logic.

### 5. Polly Resilience — Two Separate Policies
- `PartnerVerifierPolicy` — handles `TimeoutException` from the mock verify endpoint with exponential backoff retry (3 attempts: 2s, 4s, 8s) and a circuit breaker that opens after 3 consecutive failures for 30 seconds.
- `RabbitMqRetryPolicy` — handles transient RabbitMQ publish failures with exponential backoff retry (3 attempts).

### 6. Global Exception Handler
`GlobalExceptionHandler` implements `IExceptionHandler` and intercepts all unhandled exceptions, returning a consistent `ErrorResponse` shape. Controllers contain no try/catch blocks — exceptions are thrown freely and handled in one place.

### 7. DI Lifetime Strategy
- `Singleton` — `RabbitMqPublisher`, `RabbitMqConnectionFactory`, `MessagePublisherBroker` (RabbitMQ connections are expensive — created once and reused for the app lifetime)
- `Scoped` — `TransactionService`, `PartnerVerifierService` (isolated per HTTP request)

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

---

## How to Run

### Option 1: Docker Compose (recommended)

Starts both the API and RabbitMQ in one command:

```bash
docker-compose up --build
```

| Service | URL |
|---|---|
| API | http://localhost:8080 |
| Swagger UI | http://localhost:8080/swagger |
| RabbitMQ Management UI | http://localhost:15672 (guest / guest) |

To stop:

```bash
docker-compose down
```

To stop and clear all queued messages:

```bash
docker-compose down -v
```

### Option 2: Local (without Docker)

**Step 1** — Start RabbitMQ:

```bash
docker run -d \
  --name rabbitmq \
  -p 5672:5672 \
  -p 15672:15672 \
  rabbitmq:3-management
```

**Step 2** — Run the API:

```bash
cd PartnerBFF.API
dotnet run
```

The API starts at `https://localhost:7050` by default (see `Properties/launchSettings.json` to change the port).

**Step 3** — Open Swagger UI:

```
https://localhost:7050/swagger
```

### Verify Messages Are Published

After posting a valid transaction, open the RabbitMQ Management UI at `http://localhost:15672`, navigate to **Queues → partner.transactions → Get Messages** to inspect the published payload.

---

## How to Run Tests

### Run All Tests

```bash
dotnet test
```

### Run with Coverage Report

```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run the Test Project Directly

```bash
dotnet test PartnerBFF.Application.Test/PartnerBFF.Application.Test.csproj
```

### Run a Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~TransactionServiceTests"
dotnet test --filter "FullyQualifiedName~TransactionRequestTests"
dotnet test --filter "FullyQualifiedName~PartnerVerifierServiceTests"
```

### Test Coverage Areas

| Test Class | Location | What It Covers |
|---|---|---|
| `TransactionRequestTests` | `Models/` | All Data Annotation validation rules (`[Required]`, `[PositiveAmount]`, `[AllowedCurrency]`, `[AllowedTimestamp]`) |
| `TransactionServiceTests` | `Services/` | Orchestration flow, partner verification failure, publish status (`Published` / `Failed`) |
| `PartnerVerifierServiceTests` | `Services/` | HTTP client success, failure, timeout, null response, cancellation |

---

## API Reference

### `POST /api/v1/partner/transactions`

Accepts a partner transaction, verifies the partner identity, and queues the transaction for downstream processing.

**Request Body:**

```json
{
  "partnerId": "P-1001",
  "transactionReference": "TXN-99823",
  "amount": 250.00,
  "currency": "USD",
  "timestamp": "2024-05-10T14:30:00Z"
}
```

**Validation Rules:**

| Field | Rule |
|---|---|
| `partnerId` | Required |
| `transactionReference` | Required |
| `amount` | Required, must be greater than 0 |
| `currency` | Required, must be a valid ISO 4217 code (e.g. USD, EUR, GBP) |
| `timestamp` | Required, must not be default, must not be in the future |

**Responses:**

| Status | Description |
|---|---|
| `200 OK` | Transaction accepted and queued successfully |
| `400 Bad Request` | One or more validation rules failed |
| `422 Unprocessable Entity` | Partner could not be verified |
| `503 Service Unavailable` | Message broker unavailable after retries |
| `500 Internal Server Error` | Unexpected error |

**Success Response (`200`):**

```json
{
  "transactionReference": "TXN-99823",
  "status": "Published",
  "receivedAt": "2024-05-10T14:30:01Z"
}
```

**Error Response (`400`):**

```json
{
  "traceId": "0HN5K2J3L4M5N6",
  "statusCode": 400,
  "message": "Validation failed",
  "errors": [
    "Amount must be greater than 0.",
    "Currency must be a valid ISO 4217 code."
  ],
  "timestamp": "2024-05-10T14:30:00Z"
}
```

---

## Configuration

### `appsettings.json`

```json
{
  "RabbitMQ": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "ExchangeName": "partner.exchange",
    "QueueName": "partner.transactions",
    "RoutingKey": "transactions"
  },
  "PartnerVerification": {
    "BaseUrl": "https://localhost:7050"
  }
}
```

### Environment Variable Overrides (Docker Compose)

.NET maps `__` (double underscore) to `:` (config section separator), so environment variables override `appsettings.json` values at runtime with no code changes.

| Variable | Overrides | Description |
|---|---|---|
| `RabbitMQ__Host` | `RabbitMQ:Host` | Use `rabbitmq` inside Docker |
| `RabbitMQ__Port` | `RabbitMQ:Port` | Default `5672` |
| `RabbitMQ__Username` | `RabbitMQ:Username` | Default `guest` |
| `RabbitMQ__Password` | `RabbitMQ:Password` | Default `guest` |
| `RabbitMQ__ExchangeName` | `RabbitMQ:ExchangeName` | Default `partner.exchange` |
| `RabbitMQ__QueueName` | `RabbitMQ:QueueName` | Default `partner.transactions` |
| `RabbitMQ__RoutingKey` | `RabbitMQ:RoutingKey` | Default `transactions` |
| `PartnerVerification__BaseUrl` | `PartnerVerification:BaseUrl` | Use `http://api:8080` inside Docker |
