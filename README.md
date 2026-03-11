# Insurance Automation Demo

A C#/.NET automated testing scaffold for an insurance-themed web app and API. The structure mirrors what a Senior SDET might build at a company like Vertafore: API-first coverage, database assertions, reusable fixtures, and a small amount of UI automation.

## What This Project Is

- **API tests** – REST client, typed models, and example tests for customers, policies, and quotes.
- **Database tests** – `DbHelper` with SQL execution helpers and example tests that validate DB state after API calls or relationships (e.g. customer–policy JOIN).
- **UI tests** – Minimal Selenium layer (WebDriver factory, base/login/quote pages, 1–2 smoke tests).
- **Shared** – Config, models, API client, DB helper, and test data factories used by both API and UI tests.

Tests are scaffolded so they are ready to run against a real service and database once URLs and connection strings are configured.

## Why Most Coverage Is at the API Layer

API tests are fast, stable, and give broad coverage of business rules and integration points. They are the main focus of this framework so that:

- CI runs quickly.
- Flakiness from the UI is minimized.
- Backend behavior (validation, status transitions, relationships) is asserted in one place.

UI tests are kept to a small set of smoke checks.

## Why Database Assertions Are Included

Insurance workflows often require that API actions produce the correct persistent state (e.g. quote created, policy status updated). The `DbHelper` and example DB tests show how to:

- Verify records exist after API creation.
- Verify status or other fields after PATCH.
- Verify relationships (e.g. customer–policy) with JOINs.

SQL scripts under `sql/` (`seed.sql`, `cleanup.sql`, `schema_queries.sql`) support local and CI database setup and cleanup.

## Why UI Tests Are Intentionally Limited

UI tests are expensive to maintain and run. This scaffold includes:

- A small Selenium layer (WebDriver factory, base page, login and quote pages).
- A couple of smoke tests that load pages and assert basic presence.

Expanding UI coverage is left as a deliberate choice once API and DB coverage are in place.

## Fixtures and Test Data

- **ApiTestFixture** – Shared `HttpClient` and `ApiClient` for API tests.
- **DatabaseFixture** – Optional shared `DbHelper` for DB tests (used where needed).
- **CustomerFactory, PolicyFactory, QuoteFactory** – Build valid request DTOs with optional overrides (e.g. empty email for 400 tests).

Factories use a simple incrementing seed so each test can get a unique-looking payload without heavy setup.

## How This Maps to a Senior SDET-Style Framework

- **Layered structure** – Shared lib, API project, UI project, SQL scripts, config.
- **CI-friendly** – Solution and projects build and test with `dotnet test`; GitHub Actions workflow placeholder is in `.github/workflows/ci.yml`.
- **Config-driven** – Base URLs and connection strings in `appsettings.json` so the same tests can run locally and in CI.
- **No over-engineering** – Straightforward classes, minimal abstraction, and only the hooks and helpers needed for the scaffold.

## How to Run Tests

1. **Restore and build**
   ```bash
   dotnet restore
   dotnet build
   ```

2. **Run all tests**
   ```bash
   dotnet test
   ```

3. **Run only API tests**
   ```bash
   dotnet test src/ApiTests/ApiTests.csproj
   ```

4. **Run only UI tests** (requires Chrome/ChromeDriver)
   ```bash
   dotnet test src/UiTests/UiTests.csproj
   ```

5. **Configure endpoints and database**
   - Set `BaseApiUrl`, `UiBaseUrl`, and `DatabaseConnectionString` in `src/ApiTests/appsettings.json` (and `src/UiTests/appsettings.json` for UI) for your environment.
   - Without a real API, some tests may fail (e.g. 404/connection). DB tests are written to skip when no connection string is set.

## What to Implement Next

1. **Real API and DB** – Point config at a running API and database; adjust or add tests as needed.
2. **Auth** – Set `AuthToken` (or switch to OAuth) in config and use it in `ApiClient` for protected endpoints.
3. **CI** – In GitHub Actions, set secrets for connection strings and base URLs; run `sql/seed.sql` (and schema) before tests if required.
4. **DB schema** – Add a schema script (e.g. `schema.sql`) that creates `Customers`, `Policies`, `Quotes` if you want to run `seed.sql` and `cleanup.sql` as-is.
5. **Optional UI** – Add more pages or flows only if you need broader UI coverage after API/DB coverage is solid.

## Project Layout

```
InsuranceAutomationDemo/
  InsuranceAutomationDemo.sln
  src/
    ApiTests/           # API tests, fixtures, appsettings
    UiTests/            # Selenium smoke tests, pages, WebDriver factory
    Shared/             # Config, Models, Clients (ApiClient), Database (DbHelper), Fixtures (factories), Helpers
  sql/
    seed.sql
    cleanup.sql
    schema_queries.sql
  .github/workflows/
    ci.yml
  README.md
```
