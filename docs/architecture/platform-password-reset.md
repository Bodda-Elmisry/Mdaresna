# Platform password recovery

This is a Platform-only recovery flow for an active central person with a verified global phone identifier, an active Platform role, and an active Platform-local credential. It does not recover School or Family accounts.

`POST /api/platform/v1/auth/password-reset/start` accepts `{ "phone": "..." }` and always returns the same accepted response for valid requests, regardless of account eligibility or SMS delivery. `POST /api/platform/v1/auth/password-reset/complete` accepts `{ "phone": "...", "code": "12345678", "newPassword": "..." }`. A successful completion returns no access token; the user signs in with the new password.

The Platform database owns `access.local_password_reset_challenges` through `PlatformLocalPasswordRecovery`. The shared phone and its verification status remain in Identity, but OTP challenge state and the password hash are Platform-owned. The old Identity reset-challenge table is retained only for migration compatibility and is not used by this endpoint.

The code is eight digits, stored only as a purpose-separated HMAC using the existing `PlatformActivation__CodeHashKey`, valid for ten minutes, and single use. Keep that key configured and stable in the environment's secret store. The service limits sends to one per minute and ten per rolling day, and invalidates a challenge after five wrong attempts. API rate limiting applies too. An unsuccessful SMS delivery invalidates the code. The Platform SMS sender records attempts in the existing SMS log; no real SMS is sent by tests.

Successful recovery hashes the new password in Platform, rotates its credential security stamp, clears local login lockout, consumes the local challenge, and appends a Platform audit entry in one Platform transaction. Existing Platform JWTs are rejected by security-stamp validation. It does not change the shared phone or central person status. Legacy Identity sessions are not used as Platform refresh tokens.

The opt-in LocalDB integration check (`MDARESNA_FIRST_OWNER_ACTIVATION_TEST_LOCALDB=1`) creates two uniquely named temporary databases and uses a fake SMS sender to exercise activation followed by password recovery. It never uses the development databases.
