# Shared person and scoped application users

## Ownership

- `identity.accounts.Id` is the stable person ID. Identity owns the shared name
  (`DisplayName` for now), verified email and phone identifiers, `GenderCode`,
  and `DateOfBirth`. Contact changes must be verified centrally; matching names
  or phone strings alone never authorizes an account merge.
- Each application owns its own local user, display name, normalized username,
  activation state, credential, sessions, recovery, and authorization. In a
  future School deployment this ownership repeats independently per school
  database. A local user's `PersonId` is a logical reference to Identity; no
  cross-database foreign key or direct database-to-database write is allowed.
- Platform roles currently key on the central account ID. Their migration to
  the local-user ID must be a separate, reviewed authorization change.
- `schoolCode` selects a candidate school/database through the trusted Platform
  registry. A server must validate lifecycle and tenant mapping, then verify
  the username and password against that school's own database. A client-sent
  school code, tenant ID, or event scope is never authorization evidence.
- Family sign-in may still accept verified phone/email to identify its local
  user, but the Family password must belong to Family. A student without a
  phone remains linkable by the centrally issued person ID and verified
  guardian relationship.

## Expand-only slice now present

The Identity migration adds nullable `GenderCode` and `DateOfBirth`. The
Platform migration adds `access.local_users` and `access.local_credentials`
with a unique person reference, unique normalized local username, independent
status and credential, and a local credential foreign key. Both SQL Server and
PostgreSQL migrations are present; the PostgreSQL migration installs rowversion
triggers for the two new tables.

The bootstrap tool's `--import-platform-local-users` mode is **dry-run by
default**. It requires explicit, distinct Identity and Platform connection
strings and all migrations to have been applied. `--execute` imports one local
user per person with an active Platform role, initially pending activation.
The import does not read, copy, or change any credential. The one-person
`--move-platform-credential --person-id <GUID>` operation is a separate,
dry-run-by-default step. With `--execute` it makes the local credential active,
then removes only that person's legacy Identity credential and revokes legacy
Identity sessions. A rerun can complete a partial move; a conflicting password
change is refused for manual review.

## Platform credential cutover

Platform login and JWT security-stamp validation now use `access.local_credentials`.
Password reset challenges and password changes are Platform-owned and committed
in one Platform transaction. First-owner SMS proves the shared phone in Identity,
but creates only a Platform credential. The existing API request/response
contracts for login, activation and recovery are unchanged.

## Remaining work beyond the current Platform cutover

1. Add scoped Platform-user create/update/deactivate/reactivate and invite
   approval flows. Global profile edits remain Identity-owned and audited.
2. Build School provisioning and school-owned users/credentials before
   introducing `username@schoolcode` login. Use a separate database connection
   principal/secret per school; store secret references, not passwords, in the
   registry. Government self-hosted schools need an explicit connectivity and
   profile-projection policy.
3. Build Family-owned credentials and verified person/guardian links. School
   membership and academic projections reach Family through reliable events,
   not cross-database joins.
4. After all active contexts are cut over and old tokens have expired, remove
   the old Identity password, reset, and session storage in a separate
   contract migration. No active Platform endpoint uses the old credential.

Do not run the importer against an unreviewed environment or infer that an
empty local-user table means Identity accounts should be deleted. The importer
is a staging tool, not the credential cutover.

## Local development status (2026-09-14)

The new Identity and Platform PostgreSQL migrations, including local password
recovery, were applied to the two local development databases. The staging
importer created one local user. The one-person credential mover was previewed,
then executed: the local user is active with one Platform credential, and the
legacy Identity credential count for that person is zero. The password value
was not changed.
SQL Server migration files were generated but not applied to a SQL Server
environment. No School or Family runtime/database was created in this slice.
