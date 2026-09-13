# Platform password recovery

This is a Platform-only recovery flow for an active central account with a verified global phone identifier, an active Platform role, and an existing password. It does not recover School or Family accounts yet.

`POST /api/platform/v1/auth/password-reset/start` accepts `{ "phone": "..." }` and always returns the same accepted response for valid requests, regardless of account eligibility or SMS delivery. `POST /api/platform/v1/auth/password-reset/complete` accepts `{ "phone": "...", "code": "12345678", "newPassword": "..." }`. A successful completion returns no access token; the user signs in with the new password.

The Identity database owns `identity.account_password_reset_challenges` through the additive `AddPlatformPasswordResetChallenge` migration. Apply this migration to the intended **Identity** database before using the endpoints; it has not been applied automatically. Review the target connection string and migration list first. No Platform database migration is required for this flow.

The code is eight digits, stored only as a purpose-separated HMAC using the existing `PlatformActivation__CodeHashKey`, valid for ten minutes, and single use. Keep that key configured and stable in the environment's secret store. The service limits sends to one per minute and ten per rolling day, and invalidates a challenge after five wrong attempts. API rate limiting applies too. An unsuccessful SMS delivery invalidates the code. The Platform SMS sender records attempts in the existing SMS log; no real SMS is sent by tests.

Successful recovery hashes the new password, rotates the credential security stamp, clears login lockout, revokes active Identity sessions, and records an Identity security event. Existing Platform JWTs are rejected by security-stamp validation. It does not change phone verification or account status.

The opt-in LocalDB integration check (`MDARESNA_FIRST_OWNER_ACTIVATION_TEST_LOCALDB=1`) creates two uniquely named temporary databases and uses a fake SMS sender to exercise activation followed by password recovery. It never uses the development databases.
