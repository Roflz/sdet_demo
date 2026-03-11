# Insurance Automation Demo

A C#/.NET automated testing scaffold for an insurance-themed web app and API.

## What This Project Is

- **API tests** – REST client, typed models, and example tests for customers, policies, and quotes.
- **Database tests** – `DbHelper` with SQL execution helpers and example tests that validate DB state after API calls or relationships (e.g. customer–policy JOIN).
- **UI tests** – Minimal Selenium layer (WebDriver factory, base/login/quote pages, 1–2 smoke tests).
- **Shared** – Config, models, API client, DB helper, and test data factories used by both API and UI tests.

Tests are scaffolded so they are ready to run against a real service and database once URLs and connection strings are configured. How the API tests are executed, where config comes from, and how the fixture and test classes fit together are described in [docs/HOW_TO_READ_THE_API_TESTS.md](docs/HOW_TO_READ_THE_API_TESTS.md). A full description of every test (what it does, what it asserts, and what it depends on) is in [docs/TEST_CATALOG.md](docs/TEST_CATALOG.md).

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

## Running the Full Stack (API, DB, React, Tests)

### 1. Database (SQL Server via Docker)

Start SQL Server and create the database:

```bash
docker compose up -d
# Wait ~10 seconds for SQL Server to start
```

Using **Sql Server Management Studio**, connect to `localhost,1433` (user `sa`, password `InsuranceDemo1!`), create database `InsuranceDemo`, then run `sql/schema.sql` and `sql/seed.sql` in order.

### 2. API (.NET 8)

```bash
cd src/Api
dotnet run
```

API runs at **https://localhost:5001** (and http://localhost:5000). Swagger: https://localhost:5001/swagger.  
Auth is a single API key: set `Auth:ApiKey` in `appsettings.json` (default `test-api-key-12345`). All endpoints require `Authorization: Bearer <ApiKey>`.

### 3. React web app

```bash
cd webapp
npm install
npm run dev
```

App runs at **http://localhost:5173**. Use **Quotes** to fetch a quote by ID; use **Log in** with API key `test-api-key-12345` so the app can call the API (Vite proxies `/api` to the API).

### 4. Run tests

**Prerequisites:** API and database must be running. Tests use the HTTP endpoint to avoid dev-certificate issues.

Build and run **API tests** (do not run full `dotnet build` while the API is running, or you may get file-lock errors; build only the test project):

```bash
dotnet build src/ApiTests/ApiTests.csproj
dotnet test src/ApiTests/ApiTests.csproj
```

For **UI tests** (Chrome required), start the React app first, then:

```bash
dotnet test src/UiTests/UiTests.csproj
```

**Saving test results**  
By default, `dotnet test` only prints to the console. To save results to files, add logger arguments:

- **TRX and HTML** (run once to get both; open the HTML in a browser for a readable report):
  ```bash
  dotnet test src/ApiTests/ApiTests.csproj --logger "trx;LogFileName=ApiTests.trx" --logger "html;LogFileName=ApiTests.html" --results-directory TestResults
  ```
  Output: `TestResults/<config>/ApiTests.trx` and `TestResults/<config>/ApiTests.html`

- **TRX only** (good for CI / Azure DevOps):
  ```bash
  dotnet test src/ApiTests/ApiTests.csproj --logger "trx;LogFileName=ApiTests.trx" --results-directory TestResults
  ```

- **Console** (more detail):
  ```bash
  dotnet test src/ApiTests/ApiTests.csproj --logger "console;verbosity=detailed"
  ```

The `TestResults` folder is gitignored so generated reports are not committed.

**Test configuration** (`src/ApiTests/appsettings.json`):

| Setting | Purpose |
|--------|---------|
| **BaseApiUrl** | API base URL. Use **http://localhost:5000** for tests (avoids untrusted HTTPS cert). |
| **DatabaseConnectionString** | Same as API (e.g. Docker: `Server=localhost,1433;Database=InsuranceDemo;User Id=sa;Password=InsuranceDemo1!;TrustServerCertificate=True;`). DB tests skip if empty. |
| **AuthToken** | Must match API `Auth:ApiKey` (e.g. `test-api-key-12345`). Sent as `Authorization: Bearer <token>`. |
| **UiBaseUrl** | React app URL for UI tests (e.g. `http://localhost:5173`). |

## How to Run Tests (without full stack)

- **API tests** require the API and (for DB tests) the database to be running. Run `dotnet build src/ApiTests/ApiTests.csproj` then `dotnet test src/ApiTests/ApiTests.csproj`. If the API is already running, build only the test project to avoid "file in use" errors when building the solution.
- **UI tests** require Chrome and the React app (`npm run dev` in `webapp`). Run `dotnet test src/UiTests/UiTests.csproj`.

For a **full solution build** (e.g. after code changes to Shared or Api), stop the API first (Ctrl+C), then `dotnet build`, then start the API again.

## Project Layout

```
InsuranceAutomationDemo/
  InsuranceAutomationDemo.sln
  src/
    Api/                # ASP.NET Core API (customers, policies, quotes, Bearer API key auth)
    ApiTests/           # API tests, fixtures, appsettings
    UiTests/            # Selenium smoke tests, pages, WebDriver factory
    Shared/             # Config, Models, Clients (ApiClient), Database (DbHelper), Fixtures (factories), Helpers
  webapp/               # React (Vite) – Login, Quote pages, calls API
  sql/
    init-db.sql         # Create InsuranceDemo database
    schema.sql          # Create Customers, Policies, Quotes tables
    seed.sql
    cleanup.sql
    schema_queries.sql
  docker-compose.yml    # SQL Server for local dev
  .github/workflows/
    ci.yml
  docs/
    HOW_TO_READ_THE_API_TESTS.md   # How the API tests run, where config comes from, test map, glossary
    TEST_CATALOG.md   # Detailed description of every test
  README.md
```
