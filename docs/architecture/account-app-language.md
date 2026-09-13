# Per-application account language

Arabic (`ar`) is the default for each application. Central Identity stores an
optional preference per `(AccountId, AppCode)` in
`identity.account_app_language_preferences`. Supported application codes are
`platform`, `schools`, and `family`; supported languages are `ar` and `en`.
The old `accounts.PreferredLocale` field is not used for this feature because
one account can choose different languages in different applications.

The Platform API currently exposes only its own preference:

- `POST /api/platform/v1/auth/login` includes nullable `preferredLanguage`.
  Null means the account has not chosen a Platform language yet.
- `GET /api/platform/v1/me/language` returns the effective language and whether
  it was explicitly stored.
- `PUT /api/platform/v1/me/language` accepts `{ "languageCode": "ar" }` or
  `{ "languageCode": "en" }`. Both `/me` endpoints require a validated Platform
  token and derive the account ID from that token; callers cannot edit another
  account's preference or submit an app code.

The Platform Flutter and web clients store a local, **Platform-only** copy so
the login page opens in the most recently used language. After sign-in, the
account preference overrides the local copy. If the account has no stored
preference, Arabic applies unless the person explicitly picked another language
on this login screen, in which case that choice is saved to the account.
The UI switches between RTL and LTR with the language. Failed account saves are
shown to the user rather than silently treated as persisted.

Schools and Family backends/frontends are not implemented here. When they are
built, each should expose only its own app code through its own authenticated
API and use a distinct local cache key, while sharing the central Identity
preference table or an authorized Identity preference service.
