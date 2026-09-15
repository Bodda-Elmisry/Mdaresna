# Platform notifications

## Delivered flow

1. React or Flutter obtains an FCM registration token only after notification permission is already granted or the user explicitly enables notifications.
2. The client stores a stable installation ID and registers it with `PUT /api/platform/v1/me/devices/{installationId}`.
3. A Platform business operation writes a bilingual notification, its account recipients, and delivery rows in the same Platform database transaction.
4. The background dispatcher sends pending deliveries through FCM. Permanent invalid-token responses delete the stale `user_devices` row.
5. The header bell always reads the durable server inbox. FCM is the real-time wake-up channel, not the notification source of truth.
6. A `platform.permissions.changed` message reloads the current account's roles and permission codes so navigation changes without signing in again.
7. Logout first calls `DELETE /api/platform/v1/me/devices/{installationId}`. The local session is cleared only after this succeeds.

## Database and migrations

The `messaging` schema contains `user_devices`, `notifications`, `notification_recipients`, and `notification_deliveries`.

SQL Server migration: `20260915005127_AddPlatformNotifications`.

PostgreSQL migration: `20260915005141_AddPlatformNotifications`. It has been applied to the local Platform development database. For another database, set the design-time connection in the current PowerShell session and run:

```powershell
$env:MDARESNA_PLATFORM_POSTGRES_DESIGNTIME_CONNECTION = 'Host=localhost;Port=5432;Database=<platform-db>;Username=<user>;Password=<password>'
Update-Database -Context PostgreSqlPlatformDbContext
```

## Backend Firebase configuration

Keep the downloaded service-account JSON outside source control. Configure Application Default Credentials and enable the dispatcher:

```powershell
$env:GOOGLE_APPLICATION_CREDENTIALS = 'C:\secure\mdaresna-platform-firebase.json'
$env:Notifications__Firebase__Enabled = 'true'
$env:Notifications__Firebase__ProjectId = '<firebase-project-id>'
```

Optional settings are `Notifications__Firebase__DispatchIntervalSeconds` and `Notifications__Firebase__BatchSize`. With `Enabled=false` the API and durable inbox work, while push deliveries stay pending safely.

## React Web configuration

Copy `.env.example` to `.env.development.local` and fill in the values from Firebase Console. The VAPID public key is not a secret. The app loads the official Firebase Web SDK modules and registers `public/firebase-messaging-sw.js`.

## Flutter configuration

The mobile client remains runnable before Firebase is configured. Supply the four public Firebase app values at run/build time:

```powershell
flutter run --dart-define=PLATFORM_API_BASE_URL=https://localhost:7241/ --dart-define=FIREBASE_API_KEY=<api-key> --dart-define=FIREBASE_APP_ID=<app-id> --dart-define=FIREBASE_MESSAGING_SENDER_ID=<sender-id> --dart-define=FIREBASE_PROJECT_ID=<project-id>
```

Android declares `POST_NOTIFICATIONS`. Complete the APNs key/capability steps in the Firebase setup guide before testing iOS background notifications.

## Acceptance check

1. Sign in on a physical device or a browser with notifications enabled.
2. Verify one row exists in `messaging.user_devices` for the installation.
3. Change one of the signed-in user's assigned role permissions from another manager session.
4. Verify the notification appears in the bell and the affected navigation visibility refreshes.
5. Mark the item read and verify the badge decrements.
6. Sign out and verify the matching `user_devices` row is physically deleted.
7. Revoke a token in Firebase or reinstall the app, send again, and verify a permanently invalid token is removed by the dispatcher.
