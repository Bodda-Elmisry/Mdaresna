# First Platform App Manager provisioning

The first Platform owner is provisioned **once per environment** against its separate, migrated Identity and Platform databases. The owner phone is fixed at `00967777661929`. Provisioning creates no password, does not verify the phone, and does not grant a login token. The account remains `PendingVerification` until the first-login SMS OTP flow verifies phone control and the owner chooses a password.

Run the local `Mdaresna.Platform.Bootstrap` tool with a display name and a stable operation GUID. No public bootstrap API or default password exists. Set `ConnectionStrings__IdentityConnection` and `ConnectionStrings__PlatformConnection` to explicit, different SQL Server databases and confirm the targets before running:

```powershell
$env:ConnectionStrings__IdentityConnection = '<identity SQL connection string>'
$env:ConnectionStrings__PlatformConnection = '<platform SQL connection string>'
dotnet run --project src/Platform/Mdaresna.Platform.Bootstrap -- --display-name 'App Manager' --operation-id <stable-guid> --dry-run
dotnet run --project src/Platform/Mdaresna.Platform.Bootstrap -- --display-name 'App Manager' --operation-id <same-guid>
```

The tool creates one central Identity account with an **unverified** phone identifier and no password credential. It then creates the `app-manager` system role with the current Platform permissions, assigns it to that account, and records a Platform audit entry. Because the account is pending and has no password, it cannot sign in or use Platform APIs yet.

Both databases have operation-ID markers. If a step fails, inspect the state and rerun with the **same** GUID and matching display name. Never use a new GUID to work around a partial failure. A database application lock serializes concurrent first-owner attempts. A completed rerun validates the markers and assignment, but never changes the account status, phone verification, or password; in particular, it cannot reactivate an intentionally disabled owner. If an owner or conflicting phone/role already exists without matching markers, provisioning refuses the operation for manual review. This also protects pre-existing environments with the old active owner account.

The role grants a snapshot of Platform permissions present at provisioning. Newly introduced permissions require an explicit role update.

Before first activation, configure the Platform API in each environment with `PlatformSms` (the five-field legacy provider URL template, username, password, sender name, and an unambiguous success-response prefix) and `PlatformActivation__CodeHashKey`, a **separate random 32-byte secret encoded as Base64**. Supply both through environment secrets, not committed settings. Keep the activation key stable while a code is outstanding. The configuration-supplied SMS provider is used only before any `platform.sms_providers` row exists; afterward an active database provider is required. An empty SMS configuration fails closed; no OTP is sent. Apply the Identity `AddAccountActivationChallenge` and Platform `AddPlatformSmsProviders` migrations before enabling the flow. See [Platform SMS providers](platform-sms-providers.md).

The Flutter setup screen calls `POST /api/platform/v1/auth/first-owner/activation/start` with `{ "phone": "00967777661929" }`. The response is always generic so it does not reveal whether an account exists. The owner receives an eight-digit SMS code, enters it, then chooses a password of at least 12 characters. The client submits both in `POST /api/platform/v1/auth/first-owner/activation/complete` as `{ "phone": "...", "code": "...", "password": "..." }`. The API checks and consumes the code, verifies the phone, creates the hashed password, and activates the account in one Identity transaction. It does not issue a token; the owner then uses the normal `login` endpoint. Codes expire in ten minutes, permit five failed attempts, and are limited to one send per minute and ten sends per rolling day per account. IP-level rate limiting also applies.

Existing environments with an already-active owner are **not** silently migrated or reset. First-owner activation applies to accounts provisioned by the new passwordless bootstrap operation only. SMS delivery and real phone ownership must be verified in an environment with a configured provider; the automated tests use a fake sender and do not send messages.
