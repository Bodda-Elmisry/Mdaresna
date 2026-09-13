# Platform SMS providers

Platform owns its own `platform.sms_providers` table in `PlatformDbContext`. Schools and Family do not read or write this table; they will have separate provider configuration when their applications are built. The shape follows the legacy `SMSProviders` fields (username, password, sender, URL template, message length, priority, active state), with safer Platform conventions: the password is encrypted at rest, writes are audited, deletions are soft, and EF row versions prevent lost updates. The old `ProviderPeriority` spelling is replaced by `Priority`.

Apply the `AddPlatformSmsProviders` migration to the Platform database before using the API. Configure `PlatformSms__EncryptionKey` as a random 32-byte Base64 value from the environment's secret store. It must remain stable for existing rows; losing it makes provider credentials unreadable. The key is not stored in the database. Do not put real credentials in committed `appsettings.json` files. The URL template must be HTTPS and contain the five placeholders `{0}` username, `{1}` password, `{2}` sender, `{3}` recipient, `{4}` message. `SuccessResponsePrefix` must be an unambiguous provider acceptance response. The API does not return the password or ciphertext.

All endpoints require `platform.deployments.manage` and use the shared API response envelope:

| Action | Endpoint |
| --- | --- |
| List/filter/page | `GET /api/platform/v1/sms-providers?isActive=true&pageNumber=1&pageSize=20` |
| Read | `GET /api/platform/v1/sms-providers/{id}` |
| Add | `POST /api/platform/v1/sms-providers` |
| Edit | `PUT /api/platform/v1/sms-providers/{id}` |
| Activate/deactivate | `POST /api/platform/v1/sms-providers/{id}/activate` or `/deactivate` |
| Soft delete | `DELETE /api/platform/v1/sms-providers/{id}?version=<Base64 row version>` |

Create accepts `providerUserName`, `providerPassword`, `senderName`, `apiUrlTemplate`, `messageCharactersLength`, `priority`, and `successResponsePrefix`. New rows are inactive. Read responses include a Base64 `version` used in edit, activate, deactivate and delete; edits can omit `providerPassword` to keep the existing credential. Activate and deactivate accept `{ "version": "..." }`. Deletion hides the row and prevents future delivery; it retains audit history.

For each send, the Platform resolves the currently active, non-deleted provider with the lowest priority number. If any provider row exists but none is active, delivery fails closed. **Only while the table has no rows** can first-owner activation use the deployment-supplied `PlatformSms` settings as a bootstrap fallback; this solves the initial OTP-before-admin-login dependency without letting a disabled database provider be bypassed. The fallback also requires HTTPS and a definite success prefix. The legacy seed uses an explicit `legacy:any-nonempty` response compatibility mode because the provider's actual success prefix is unknown; this has weaker delivery acknowledgement semantics and should be replaced after verification. `MessageCharactersLength` is retained as legacy metadata, not enforced as a hard send cap. No real SMS is sent by tests. The [initial provider seed command](platform-sms-provider-seed.md) provisions the supplied gateway securely from an environment secret or hidden prompt.

This change does not provide WhatsApp sending or a general-purpose send-message API. Those are separate messaging features.

## SMS delivery log

The `AddPlatformSmsLogs` migration adds `platform.sms_logs` to the Platform database. Every Platform SMS send attempt creates a `Pending` row before the HTTP request. The row is completed as `Accepted`, `Rejected`, or `Failed`, with the selected provider ID (null for bootstrap configuration fallback), HTTP status when one was received, a sanitized failure reason, and timestamps. A `Pending` row left by a process crash is deliberately retained for operational reconciliation; it must not be interpreted as successful delivery.

`RecipientEncrypted`, `MessageEncrypted`, and `ResponseEncrypted` hold authenticated ciphertext. The masked recipient is the only readable phone representation in the table. The gateway response may echo an OTP or credentials, so neither the raw response nor the request URL is written to application logs or public exceptions. An empty gateway response is encrypted as an empty value; `ResponseEncrypted = NULL` means there was no response. Responses are bounded to 16 KiB, and an oversized response is rejected. A functioning `PlatformSms__EncryptionKey` is now required before any send, including the bootstrap configuration fallback. Keep this key stable and backed up securely; losing it makes historical SMS transcripts and provider credentials unreadable. If the audit write fails before dispatch, the SMS is not sent. If finalization fails after dispatch, the caller treats delivery as unconfirmed.

There is no SMS-log read API in this slice. Access to full message and response contents should be permission-gated and decrypted only through a future administrative workflow; direct SSMS rows intentionally do not show OTPs or gateway bodies.

The cross-system source/type/school metadata and RabbitMQ intake are described in [Cross-system SMS audit in Platform](platform-sms-event-logging.md).
