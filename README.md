# LunchSplit

LunchSplit calculates weighted meal shares and processes payments through Square Sandbox.
The original bill calculation and receipt features are in `src/LunchSplit`.
See `CODEMAP.md` for the solution, source, and test paths.

## Requirements

- .NET 8 SDK or a newer SDK capable of targeting .NET 8, plus the .NET 8 runtime.
- Internet access for NuGet restore and the five live Square Sandbox tests.
- A Square Sandbox access token and a Canadian Sandbox location that processes CAD.

## Configure and run

Credentials are read from environment variables; they are never embedded in source.
Configure these in your terminal before running the complete suite:

```powershell
$env:SQUARE_SANDBOX_ACCESS_TOKEN = '<your Sandbox access token>'
$env:SQUARE_SANDBOX_LOCATION_ID = '<your Canadian Sandbox location ID>'
dotnet test
```

Alternatively, copy `.env.example` to `.env.local`, fill in your Sandbox values,
and run `powershell -File scripts/Test.ps1`. The helper loads the settings and runs
`dotnet test` from the solution root. `.env.local` is ignored by Git and excluded
from the submission archive. Missing credentials fail the live tests with a setup
message rather than silently skipping them.

To run only the isolated tests, use `dotnet test --filter TestCategory=Moq`.
To run only the live tests, use `dotnet test --filter TestCategory=SquareSandbox`.

## Payment design

`IPaymentProcessor` and the payment DTOs belong to the core project.
`PaymentManager` depends only on that interface, rejects nonpositive or
fractional-cent amounts, and submits the rounded shares from the original splitter.
Batch results retain each outcome and count successes and failures.

`SquarePaymentService` implements the interface in a separate infrastructure
project. It uses the official Square SDK, pins the Sandbox endpoint, converts
dollars to integer cents, and sets both the HTTP client and SDK request timeout
to five seconds with automatic retries disabled. The pinned SDK 47 uses
`Payments.CreateAsync` for `POST /v2/payments` and `Payments.GetAsync` for
`GET /v2/payments/{payment_id}`.

The declined magic nonce can return `GENERIC_DECLINE`; the wrapper translates
that to the domain code `CARD_DECLINED` while preserving the actual gateway code.
Currency validation errors retain their HTTP status, category, and field.
Idempotency keys are sent in Square's `idempotency_key` request-body field.
Retries must reuse both that key and the original payment contents.

## Assignment 3 test coverage

| Test ID | Tier | Verification |
| --- | --- | --- |
| M3-01 | Square Sandbox | Successful payment with a transaction ID |
| M3-02 | Square Sandbox | Declined card translated to `CARD_DECLINED` |
| M3-03 | Moq | HTTP failure translated to a domain timeout exception |
| M3-04 | Moq | Exact dollar amount and one gateway invocation |
| M3-05 | Moq | Zero amount never invokes the gateway |
| M3-06 | Moq | Four-attendee bill invokes the gateway exactly four times |
| M3-07 | Moq | Second attendee declines; two succeed and one fails |
| M3-08 | Square Sandbox | EUR at a CAD location returns HTTP 400 validation |
| M3-09 | Square Sandbox | Repeated key returns the original transaction ID |
| M3-10 | Square Sandbox | Transaction status query returns `COMPLETED` |

There are ten Assignment 3 test methods. Assignment 1's tests are preserved,
including its two previously ignored future-feature placeholders.
Live tests use fresh idempotency keys and create only simulated Sandbox payments.
