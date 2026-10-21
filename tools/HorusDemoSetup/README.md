# Horus local demo setup

Run from the backend repository root. This bounded .NET 8 CLI targets only the local PostgreSQL development school `SCH-1A769BC8A279` and checks the school name, owner login, migration state and student entitlement. It refuses to replace existing academic data. A completed rerun verifies the existing demo and refreshes only its classroom permissions and Cairo attendance settings.

```powershell
dotnet run --project tools/HorusDemoSetup/HorusDemoSetup.csproj -- --inspect
dotnet run --project tools/HorusDemoSetup/HorusDemoSetup.csproj -- --self-test
dotnet run --project tools/HorusDemoSetup/HorusDemoSetup.csproj -- --apply
dotnet run --project tools/HorusDemoSetup/HorusDemoSetup.csproj -- --verify
```

`--apply` reads the owner password from the ephemeral environment key `MDARESNA_DEMO_OWNER_PASSWORD`. Supply it privately and remove it after execution. Credentials are never included in reports. Configuration is loaded from the existing API appsettings, Development settings, User Secrets and environment. The separate generated demo staff password is held in Schools API User Secrets under `SchoolDemo:HorusStaffPassword`; the owner's password is unchanged.

Before the initial seed, the CLI creates custom PostgreSQL backups of the school, Platform and Identity databases in `%LOCALAPPDATA%/Mdaresna/Backups` and checks their catalogs using `pg_restore --list`. This checks the backup catalog, not a full recovery drill. School records and capacity allocations commit together in one transaction. Staff identity and global student resolution use the existing Platform gateways, with deterministic demo identity markers; those central writes are outside the school transaction and may remain if it rolls back. The CLI never sends invitations, activation messages, SMS or email.

The isolated self-test migrates a random temporary PostgreSQL database, substitutes the two external gateways, creates and verifies the full dataset and drops the temporary database in `finally`. It never sends messages. This tool does not change application runtime behavior.

## Dataset

### Reset existing demo staff passwords

`--reset-staff-password` uses the ephemeral environment variable `MDARESNA_DEMO_STAFF_PASSWORD` (minimum eight characters). The existing local Horus guards apply. It requires exactly 105 active accounts with the `demo-` username prefix and existing credentials. It updates their password hashes and security stamps, clears sign-in lockout, revokes their refresh sessions, verifies every stored hash and synchronizes `SchoolDemo:HorusStaffPassword` in Schools API User Secrets. Owner credentials are excluded. Remove the environment variable after execution; do not place the password in reports or source files.

- Two programmes: National KG1–KG2, primary 1–6, preparatory 1–3; British FS1–FS2 and Year 1–9. Academic year 2026/2027; dates are demo configuration, not an official school calendar.
- One campus, two buildings, three stage floors per building, 22 dedicated classrooms of capacity 10, four labs and six supervision offices.
- One class per grade; five active students in every class: 110 students, 220 guardians and 110 allocated student seats.
- 69 subject teachers, 22 class co-teachers, six stage supervisors and eight operations employees. Random Arabic/English names and sequential demo phone numbers beginning at the user-requested value. No phone verification or external messaging occurs.
- Subject departments, staff memberships and department leadership, primary/assistant class subject assignments, operational class grants, curriculum quotas and metadata-only book editions.
- Sunday–Thursday, starting 08:00. Daily lessons: 5 early years, 6 primary, 7 preparatory, with one break. Total 670 weekly lessons and 110 breaks. Verification rejects teacher, classroom and class overlaps and quota mismatches.
- Yearly, term, October monthly, current-week and applicable current-day teaching plan drafts. These are clearly labeled demo content requiring review; no approval or publication is fabricated. No real textbook PDFs, attendance history, payroll or financial transactions are fabricated.

`--verify` exports UTF-8 CSV reports under `../Deliverables/Horus-Demo`: staff logins and contacts, students and classes, and weekly timetables. No passwords or connection details are exported.
