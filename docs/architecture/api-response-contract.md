# Shared HTTP response contract

New Platform, Schools and Family business APIs use `Mdaresna.Api.Contracts` from `src/BuildingBlocks`. This is an HTTP response contract, not a domain DTO or a RabbitMQ integration envelope. It has no dependency on an application, ASP.NET Core, or the legacy API.

## Success

`ApiResponse<T>` serializes as camel-case JSON. `Data` is strongly typed; do not use `object` for each endpoint's payload. A successful response contains `statusCode`, `isSuccess`, `message`, `data`, and optionally `correlationId`. `code` and `errors` are absent. A paged success uses `PagedApiResponse<T> : ApiResponse<IReadOnlyList<T>>`, with the additional top-level `totalCount`, `pageNumber`, `pageSize`, and computed `totalPages`.

```json
{
  "statusCode": 200,
  "isSuccess": true,
  "message": null,
  "data": [{ "id": "school-id", "name": "Example School" }],
  "correlationId": "7f1a8e7e0cc34d85aa5585726d10b5c4",
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

Page numbering starts at 1. An empty page has `data: []`; `totalPages` is 0 when `totalCount` is 0. Use the paged subtype only for successful list responses; errors use the base response.

## Failure

All business API failures use the base response with a real 4xx/5xx HTTP status, `data: null`, a stable machine-readable `code`, a safe human-readable `message`, and optional `errors` keyed by field path. `isSuccess` is derived from `statusCode`, never set independently. The body `statusCode` must equal the HTTP status. Clients branch on `code`/HTTP status, not on translated text.

```json
{
  "statusCode": 400,
  "isSuccess": false,
  "message": "Some fields are invalid.",
  "data": null,
  "code": "validation.failed",
  "errors": { "schoolCode": ["School code is required."] },
  "correlationId": "7f1a8e7e0cc34d85aa5585726d10b5c4"
}
```

The API returns the same correlation ID in `X-Correlation-ID`. Never put stack traces, SQL errors, credentials, tokens, or raw exception messages in 5xx responses. Prefer a generic 500 message and log the details server-side against the correlation ID.

## HTTP behavior and adoption

- Use actual HTTP semantics: 200 for reads/updates, 201 with `Location` for creation, 202 with an operation resource for asynchronous work, and appropriate 400/401/403/404/409/422/429/500 responses.
- HTTP 204 has no body or envelope. Health endpoints, Swagger, file/stream responses, and protocol-defined auth endpoints are not business-response envelopes.
- Future Schools and Family API hosts reference the shared contract project directly and implement a single HTTP result/error mapping that preserves these invariants. They do not reference the Platform API or Platform domain.
- Platform uses a host-side `ApiResponseWriter.ToResult` helper for successful routes so the actual HTTP status is taken from the envelope. New hosts should use an equivalent adapter or integration test rather than hand-writing a mismatched status.
- Do not wrap or change the existing legacy API globally: Flutter currently consumes raw DTOs, arrays, strings, and older paged shapes. Migrate endpoints and Flutter parsing together behind a versioned route or explicit compatibility boundary.
