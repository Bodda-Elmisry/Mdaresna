# Initial Platform SMS provider provisioning

Provision the supplied legacy gateway record once per Platform environment, after
the `AddPlatformSmsProviders` migration has been applied. This is a deliberate
deployment operation, not an EF `HasData` seed or API-startup task: provider
credentials must never appear in a migration, committed configuration, shell
command argument, or log.

Set `ConnectionStrings__PlatformConnection` to the intended Platform database and
`PlatformSms__EncryptionKey` to a stable, random 32-byte Base64 key from that
environment's secret store. Keep the key backed up securely. The same key must
be provided to the Platform API or the stored credential cannot be decrypted.
Review the target and run the read-only check first:

```powershell
dotnet run --project src/Platform/Mdaresna.Platform.Bootstrap -- --seed-sms-provider --dry-run
dotnet run --project src/Platform/Mdaresna.Platform.Bootstrap -- --seed-sms-provider
```

The second command reads the credential from `PlatformSms__SeedPassword` when set.
Otherwise, run it in an interactive terminal and enter the credential at the
hidden prompt; redirected input is refused. Do not pass the credential as a CLI
argument. Remove the seed-password environment variable after provisioning if
one was used. The original plain credential is not returned by the API.

The seed uses a fixed provider ID, username and sender `Mdaresna`, the legacy
HTTPS five-field URL template, priority `1`, active `true`, deleted `false`, and
message length `20`. The supplied creation and modification timestamps are
interpreted as **2026-02-08 21:05:50 UTC**, because the Platform database requires
UTC offsets. Its response setting is the explicit legacy compatibility mode
`legacy:any-nonempty`; this accepts a nonempty provider response that does not
start with the old `SOMETHING WENT AWRY` error text. It should be replaced with
a verified, unambiguous success prefix once the gateway's actual responses are
known.

The operation checks that the database is migrated, holds a SQL application
lock, inserts the encrypted provider and an audit entry in one transaction, and
returns unchanged when its fixed ID is already present. It never reactivates a
disabled or deleted record or overwrites an operator's later edits or credential
rotation. If another SMS provider already exists without this seed ID, it refuses
to write anything and requires manual review. To rotate the gateway password,
use the Platform SMS provider management API, not this seed command. Rotating
`PlatformSms__EncryptionKey` requires planned re-encryption of existing rows;
simply replacing the key makes their credentials unreadable.

The initial legacy message-length value of `20` is retained exactly as supplied.
The old sender never applied this field as a hard cap, so the Platform sender
also treats it as provider metadata; it does not truncate or reject OTP text
because of this value.
