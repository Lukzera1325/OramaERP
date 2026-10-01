# Security review

Updated 2026-10-01. This review is source-based and is not a penetration test.

## Corrections in this reconstruction

- Removed signing-key/issuer/audience fallback values. Startup fails clearly if JWT settings are absent or the key is shorter than 32 characters.
- Changed JWT lifetime from 30 days to 8 hours and aligned the response's reported expiration.
- CORS now uses explicitly configured origins; wildcard origin/method/header settings were removed.
- Session cookies are HttpOnly, SameSite=Lax, and Secure outside Development.
- Removed silent default user/company ID `1` from `BaseController`.
- Removed the known development admin from EF model seed and current migration snapshots. Its password hash and seeded membership inserts were removed from source migrations. Existing databases are not automatically modified; rotate or remove any prior seeded account.
- Added optional initial local administrator provisioning through external `Seed:AdminEmail` and `Seed:AdminPassword` settings, only when the user table is empty; password must be at least 12 characters and the account is not a super-admin.
- Removed documented demo credentials and fabricated mobile JWTs. Mobile login calls the real API endpoint; refresh is not simulated.
- Removed destructive web startup database deletion. Compose credentials are required from an ignored `.env` file.

## Remaining concerns

- Stateless JWT logout does not revoke an already issued token; refresh/revocation infrastructure is not present.
- Check and rotate credentials in existing SQLite/PostgreSQL databases and external copies/backups. Source cleanup cannot invalidate old credentials or erase remote Git history.
- PostgreSQL deployment migrations are not ready: checked-in migrations are SQLite-specific, while local Docker selects PostgreSQL and creates an empty schema only in Development.
- Confirm that production CORS origins are configured, HTTPS is terminated correctly, and secret values are supplied by a managed secret store.
- The mobile API base address is configurable through `OramaApiBaseUrl` Preferences; production endpoint configuration and device TLS behavior still require platform validation.

## Secret scan scope

Searched application source, tracked documentation, scripts, migrations, and runtime configuration for the known demo password, JWT fallback, fake token markers, destructive startup database calls, and wildcard CORS. Removed the matching demo values from the current working tree. `.env.example` contains placeholders only. This does not prove that Git history, ignored files, backups, or external copies contain no secrets.
