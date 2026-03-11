# How the API Tests Work

This document describes the ApiTests project: how tests are run, where configuration comes from, how the fixture and test classes fit together, and what each test class does. It is written for someone opening the project for the first time.

---

## How the tests are executed

The API tests are written with **xUnit** (the test framework from the xunit NuGet package). When `dotnet test` is run against the ApiTests project, xUnit discovers all public methods marked with the `[Fact]` attribute and runs each one as a separate test. Each `[Fact]` method is a single test case.

Test classes can declare **IClassFixture&lt;ApiTestFixture&gt;** (an xUnit interface). When a class does this, xUnit creates one instance of ApiTestFixture before running any test in that class, then creates the test class and passes the fixture instance into the class constructor. Every test method in that class then shares the same fixture (and thus the same ApiClient and config). This avoids creating a new HttpClient and loading config for every single test.

AuthApiTests does not use IClassFixture&lt;ApiTestFixture&gt;. That class creates its own ApiClient inside each test method so it can load config and then clear the AuthToken to simulate an unauthenticated request.

---

## Where configuration comes from

The tests need the API base URL, an API key (token), and optionally a database connection string. These values live in **src/ApiTests/appsettings.json**. The ApiTests.csproj file contains a rule that copies appsettings.json into the build output directory (e.g. src/ApiTests/bin/Debug/net8.0/) when the project is built. At runtime, the tests execute from that output directory; the .NET property **AppContext.BaseDirectory** returns that path. **ConfigLoader** (in Shared.Helpers) reads the file "appsettings.json" from that path, deserializes it into a **TestConfig** object (Shared.Config), and returns it. TestConfig has the properties BaseApiUrl, AuthToken, DatabaseConnectionString, and UiBaseUrl. The ApiClient is constructed with this config so it knows where to send requests and what token to put in the Authorization header.

For the tests to pass against a running API, BaseApiUrl in appsettings.json must point at that API (e.g. http://localhost:5000), and AuthToken must match the API key configured in the API’s own appsettings (e.g. Auth:ApiKey). The database tests also require DatabaseConnectionString to be set to the same database the API uses.

---

## Request flow from test to API

1. **Config is loaded.** ApiTestFixture’s constructor (or, in AuthApiTests, the test method) calls ConfigLoader.Load(AppContext.BaseDirectory) and gets a TestConfig.
2. **ApiClient is created.** The fixture (or the test) creates an HttpClient and an ApiClient(http, config). ApiClient sets the HttpClient’s BaseAddress to TestConfig.BaseApiUrl and, if AuthToken is non-empty, sets the default Authorization header to "Bearer " + AuthToken.
3. **The test calls a method on ApiClient.** For example, _api.PostCustomerAsync(request). That method serializes the request to JSON and sends an HTTP POST to the path "customers" (relative to the base URL).
4. **The API responds.** The test receives an HttpResponseMessage (status code and body).
5. **The test asserts.** The test uses methods like response.EnsureSuccessStatusCode(), Assert.NotNull(customer), Assert.Equal(...) to verify the status and body. Assert is from xUnit; a failing assertion fails the test.

In database validation tests, after step 4 the test also uses **DbHelper** (Shared.Database) to run SQL against the database and then asserts on the query results (e.g. that a row exists or that a column has the expected value).

---

## Test class map

| Class | Purpose | Shared fixture? | Depends on |
|-------|---------|-----------------|------------|
| **AuthApiTests** | Verifies API behavior when no valid auth token is sent. Sends GET /customers/1 with no Authorization header and asserts status is 401, 404, or 200. | No; creates its own client with cleared token. | API running. |
| **CustomersApiTests** | Tests POST /customers: one test sends valid data and asserts 2xx and correct response body; one test sends invalid data (empty Email) and asserts 400. | Yes (ApiTestFixture). | API running; API validates required fields. |
| **QuotesApiTests** | Tests POST /quotes with valid data; asserts 2xx and that the response body contains the created quote with correct Id, CustomerId, Premium. | Yes (ApiTestFixture). | API running; customer Id 1 must exist. |
| **PoliciesApiTests** | Tests GET /policies/{id} for a missing resource: sends GET /policies/999999 and asserts 404. | Yes (ApiTestFixture). | API running. |
| **DatabaseValidationTests** | Calls the API (e.g. POST quote, PATCH policy status) then queries the database to verify the data was persisted. One test only runs a JOIN query (no API call). | Yes (ApiTestFixture); also creates its own DbHelper in InitializeAsync when DatabaseConnectionString is set. | API and SQL Server running; connection string in appsettings; some tests assume customer 1 and policy 1 exist. |

---

## Fixtures

**ApiTestFixture** (src/ApiTests/Fixtures/ApiTestFixture.cs) is the shared setup used by CustomersApiTests, QuotesApiTests, PoliciesApiTests, and DatabaseValidationTests. Its constructor runs once per test class. It loads appsettings.json via ConfigLoader.Load(AppContext.BaseDirectory), creates one HttpClient and one ApiClient with that config, and exposes ApiClient and Config as properties. The test class constructor receives the fixture and stores fixture.ApiClient in a field (e.g. _api) so each test method can call _api.PostCustomerAsync(...) and similar methods.

**DatabaseFixture** (src/ApiTests/Fixtures/DatabaseFixture.cs) is an optional fixture that provides a shared DbHelper. It implements IAsyncLifetime; xUnit calls InitializeAsync() before running tests, and the fixture loads config and creates a DbHelper from DatabaseConnectionString. DatabaseValidationTests does not use DatabaseFixture; that class creates its own DbHelper in its own InitializeAsync() and stores it in a private field _db. DatabaseFixture exists for any other test class that might want a shared DbHelper via IAsyncFixture&lt;DatabaseFixture&gt;.

---

## Lifecycle when a test runs

1. xUnit loads the test assembly (ApiTests.dll).
2. For a test class that uses IClassFixture&lt;ApiTestFixture&gt;, xUnit creates one ApiTestFixture. ApiTestFixture’s constructor runs: it gets AppContext.BaseDirectory, calls ConfigLoader.Load(basePath), creates HttpClient and ApiClient with the returned TestConfig.
3. xUnit creates an instance of the test class (e.g. CustomersApiTests) and passes the fixture into the constructor. The constructor assigns fixture.ApiClient to _api.
4. For DatabaseValidationTests, xUnit then calls InitializeAsync() on the test class instance (because the class implements IAsyncLifetime). InitializeAsync loads config and, if DatabaseConnectionString is set, creates a new DbHelper and assigns it to _db.
5. xUnit runs each [Fact] method. The method uses _api (and, in DatabaseValidationTests, _db when non-null) to perform the test and call Assert.*.
6. When all tests in the class have run, xUnit disposes the fixture if it implements IDisposable.

---

## Terms used in the tests

- **Fixture** — A shared object that xUnit creates once per test class when the class implements IClassFixture&lt;T&gt;. The fixture is passed into the test class constructor. ApiTestFixture and DatabaseFixture are fixtures.
- **ApiClient** — A type in Shared.Clients that wraps HttpClient. It is configured with a base URL and optional auth token from TestConfig and exposes methods such as PostCustomerAsync, GetCustomerAsync, GetPolicyAsync. Tests use it to send HTTP requests to the API.
- **TestConfig** — A type in Shared.Config. It holds BaseApiUrl, AuthToken, DatabaseConnectionString, UiBaseUrl. It is populated by deserializing appsettings.json in the test output directory.
- **ConfigLoader** — A static helper in Shared.Helpers. Load(basePath) reads appsettings.json from the given path and returns a TestConfig.
- **BaseApiUrl / AuthToken** — Keys in src/ApiTests/appsettings.json. BaseApiUrl is the root URL of the API (e.g. http://localhost:5000). AuthToken is the value sent as the Bearer token in the Authorization header; it must match the API’s Auth:ApiKey for protected endpoints.
- **CustomerFactory, QuoteFactory, PolicyFactory** — Types in Shared.Fixtures. They build valid request objects (CreateCustomerRequest, CreateQuoteRequest, etc.) with default or overridable values so tests do not hand-construct JSON and so each call can get unique data (e.g. unique email) to avoid conflicts.
- **DbHelper** — A type in Shared.Database. It is constructed with a SQL Server connection string and provides methods such as RecordExistsAsync, GetQuoteByIdAsync, GetPolicyStatusAsync, GetCustomerWithPolicyAsync. The database validation tests use it to run SQL and verify that data the API claimed to create or update is present in the database.

---

## Flow diagram (text)

```
appsettings.json (in test output dir)
        │
        ▼
ConfigLoader.Load(BaseDirectory)  →  TestConfig (BaseApiUrl, AuthToken, DatabaseConnectionString)
        │
        ▼
ApiTestFixture constructor:  HttpClient + ApiClient(http, config)
        │
        ▼
Test class constructor:  _api = fixture.ApiClient
        │
        ▼
[Fact] method:  _api.PostCustomerAsync(request)  →  HTTP POST to BaseApiUrl/customers
        │
        ▼
API responds with HttpResponseMessage (status + body)
        │
        ▼
Test:  Assert on status and/or deserialized body (and, in DB tests, on DbHelper query results)
```
