# Platform Notifications implementation checklist

This checklist tracks the remaining implementation after the Firebase Console setup guide. A stage is marked complete only after its code and proportional verification pass.

- [x] Stage 1  Define the Platform notification, user-device, and delivery-attempt model
- [x] Stage 2  Add SQL Server and PostgreSQL migrations for the notification tables (PostgreSQL development database updated)
- [x] Stage 3  Add authenticated device registration, refresh, and removal APIs
- [x] Stage 4  Remove the current device registration during logout before clearing the client session
- [x] Stage 5  Add the Firebase Cloud Messaging sender with safe disabled-development behavior
- [x] Stage 6  Add the in-app notification inbox APIs, unread count, read, and read-all operations
- [x] Stage 7  Publish notifications after permission changes so affected Platform users refresh their access
- [x] Stage 8  Integrate React Web with Firebase Messaging, token refresh, foreground messages, and the header notification panel
- [x] Stage 9  Integrate Flutter with Firebase Messaging, token refresh, foreground/background handling, and the header notification panel
- [x] Stage 10  Add automated backend, React, and Flutter tests and run all relevant builds
- [x] Stage 11  Document environment variables, migration commands, Firebase files, and the end-to-end verification procedure

## Firebase Console work owned by the operator

These are external credentials and console settings, so they remain deliberately separate from source control:

- [ ] Complete the Firebase Console steps in the supplied Word guide.
- [ ] Add the Web Firebase values to `.env.development.local`.
- [ ] Supply the Flutter Firebase values as `--dart-define` arguments.
- [ ] Store the backend service-account credential outside the repository and set `GOOGLE_APPLICATION_CREDENTIALS`.
- [ ] Enable `Notifications__Firebase__Enabled=true` and run the live-device acceptance check.

## Required logout behavior

Both clients call the authenticated device-unregister endpoint for their current installation before deleting the local access token or session. If that server call fails, the session is retained and the user is shown a retry message; this prevents a successful-looking logout from leaving the device registered. Tokens rejected permanently by FCM are also deleted automatically by the backend dispatcher.
