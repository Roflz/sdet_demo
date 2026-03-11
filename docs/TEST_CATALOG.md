# Test Catalog

This document describes every automated test in the Insurance Automation Demo project: what each test does, what it asserts, and what it depends on (API, database, or UI).

---

## Overview

| Project    | Test classes              | Tests | Dependencies        |
|-----------|---------------------------|-------|---------------------|
| **ApiTests** | CustomersApiTests         | 2     | API (HTTP)          |
| **ApiTests** | QuotesApiTests            | 1     | API (HTTP)          |
| **ApiTests** | PoliciesApiTests          | 1     | API (HTTP)          |
| **ApiTests** | AuthApiTests              | 1     | API (HTTP)          |
| **ApiTests** | DatabaseValidationTests   | 3     | API + SQL Server    |
| **UiTests**  | QuoteSmokeTests           | 2     | React app + Chrome  |

**Total: 10 tests** (8 in ApiTests, 2 in UiTests).

All API tests use the shared **ApiTestFixture**, which provides an **ApiClient** configured from `src/ApiTests/appsettings.json` (BaseApiUrl, AuthToken). The client sends `Authorization: Bearer <AuthToken>` on every request.

---

## ApiTests

### CustomersApiTests

Uses **ApiTestFixture** (shared ApiClient). Tests the **Customers** REST API: `POST /customers`, validation, and response shape.

---

#### CreateCustomer_WithValidPayload_ReturnsSuccess

- **Purpose:** Ensures a valid customer create request is accepted and the API returns the created resource with an ID and correct fields.
- **Steps:**
  1. Build a valid request using `CustomerFactory.Create()` (unique FirstName, LastName, Email, Phone).
  2. Call `POST /customers` with that body.
  3. Assert response is success (2xx).
  4. Deserialize response as `Customer` and assert: `Id > 0`, and `FirstName`, `LastName`, `Email` match the request.
- **Assertions:** `EnsureSuccessStatusCode()`, `customer` not null, `customer.Id > 0`, `FirstName`/`LastName`/`Email` equal request values.
- **Dependencies:** API must be running; AuthToken in config must match API `Auth:ApiKey`.

---

#### CreateCustomer_MissingRequiredField_Returns400

- **Purpose:** Ensures the API rejects invalid input when a required field (e.g. Email) is missing.
- **Steps:**
  1. Build a request with `CustomerFactory.Create(r => r.Email = "")`.
  2. Call `POST /customers`.
  3. Assert response status is **400 Bad Request**.
- **Assertions:** `response.StatusCode == BadRequest`.
- **Dependencies:** API must be running and must validate required fields (FirstName, LastName, Email).

---

### QuotesApiTests

Uses **ApiTestFixture**. Tests **Quotes** API: `POST /quotes` and response shape.

---

#### CreateQuote_WithValidPayload_ReturnsSuccess

- **Purpose:** Ensures a valid quote create request is accepted and the API returns the created quote with ID and correct data.
- **Steps:**
  1. Build a request with `QuoteFactory.Create(customerId: 1)` (assumes customer 1 exists, e.g. from seed).
  2. Call `POST /quotes`.
  3. Assert response is success and body is a `Quote` with `Id > 0`, `CustomerId` and `Premium` matching the request.
- **Assertions:** `EnsureSuccessStatusCode()`, quote not null, `quote.Id > 0`, `quote.CustomerId == request.CustomerId`, `quote.Premium == request.Premium`.
- **Dependencies:** API running; at least one customer (e.g. Id 1) in the database for the request to be valid.

---

### PoliciesApiTests

Uses **ApiTestFixture**. Tests **Policies** API: `GET /policies/{id}` for a nonexistent resource.

---

#### GetPolicy_NonexistentId_Returns404

- **Purpose:** Ensures the API returns 404 when requesting a policy that does not exist.
- **Steps:**
  1. Call `GET /policies/999999`.
  2. Assert response status is **404 Not Found**.
- **Assertions:** `response.StatusCode == NotFound`.
- **Dependencies:** API running only; no specific data required.

---

### AuthApiTests

Does **not** use the shared fixture. Creates its own **ApiClient** with **no** AuthToken to simulate an unauthenticated request.

---

#### GetCustomer_WithoutAuthToken_Returns401WhenApiRequiresAuth

- **Purpose:** Ensures that when no (or invalid) auth is sent, the API responds with 401 Unauthorized (or a documented alternative when auth is not enforced).
- **Steps:**
  1. Load config and set `AuthToken = ""`.
  2. Create a new `HttpClient` and `ApiClient` with that config (no Bearer header).
  3. Call `GET /customers/1`.
  4. Assert status is one of: **401 Unauthorized**, **404 Not Found**, or **200 OK** (to allow for APIs that don’t enforce auth yet).
- **Assertions:** `StatusCode` is 401, 404, or 200.
- **Dependencies:** API running. With the current API (auth enforced), the expected result is **401**.

---

### DatabaseValidationTests

Uses **ApiTestFixture** (for API calls) and implements **IAsyncLifetime** to create a **DbHelper** when `DatabaseConnectionString` is set. Combines API and direct SQL Server checks.

---

#### AfterApiCreatesQuote_QuoteExistsInDatabase

- **Purpose:** Ensures that after creating a quote via the API, the same record exists in the database with the expected premium.
- **Steps:**
  1. If `_db` is null (no connection string), return without asserting (skip).
  2. Create a quote via `POST /quotes` using `QuoteFactory.Create(customerId: 1)`. If the response is not success, return (skip).
  3. Deserialize response as `Quote`.
  4. Use `DbHelper.RecordExistsAsync("Quotes", "Id", quote.Id)` to verify the row exists.
  5. Use `DbHelper.GetQuoteByIdAsync(quote.Id)` and assert the returned row’s `Premium` matches the request.
- **Assertions:** Quote exists in DB; `row.Premium == request.Premium`.
- **Dependencies:** API and SQL Server; `DatabaseConnectionString` in appsettings; customer 1 must exist.

---

#### AfterPolicyStatusPatch_StatusUpdatedInDatabase

- **Purpose:** Ensures that after patching a policy’s status via the API, the database row is updated.
- **Steps:**
  1. If `_db` is null, return (skip).
  2. Call `PATCH /policies/1/status` with body `{ "status": "Cancelled" }`. If not success, return (skip).
  3. Use `DbHelper.GetPolicyStatusAsync(1)` to read `Status` from the database.
  4. Assert status equals `"Cancelled"`.
- **Assertions:** `status == "Cancelled"`.
- **Dependencies:** API and SQL Server; policy Id 1 must exist (e.g. from seed). This test **modifies** data (status change).

---

#### CustomerPolicyRelationship_JoinReturnsCorrectRow

- **Purpose:** Ensures the database relationship between Customers and Policies can be queried via a JOIN and returns the expected customer and policy data.
- **Steps:**
  1. If `_db` is null, return (skip).
  2. Call `DbHelper.GetCustomerWithPolicyAsync(1, 1)` (JOIN Customers and Policies on CustomerId, filter by customer 1 and policy 1).
  3. If a row is returned, assert `CustomerId == 1`, `PolicyId == 1`, and `PolicyNumber` is non-empty.
  4. If no row (e.g. no seed data), the test does not fail (no assertion).
- **Assertions:** When row exists: `CustomerId`, `PolicyId`, and `PolicyNumber` are correct.
- **Dependencies:** SQL Server and connection string; seed data with customer 1 and policy 1 linked.

---

## UiTests

### QuoteSmokeTests

Creates a **Chrome** WebDriver (headless) and loads **UiBaseUrl** from `src/UiTests/appsettings.json`. Each test navigates to a page and performs a minimal check. **IDisposable** is implemented to quit the driver after the class.

---

#### QuotePage_LoadsWithoutError

- **Purpose:** Smoke test that the Quotes page loads without throwing and the browser has a title.
- **Steps:**
  1. Create `QuotePage` with driver and base URL.
  2. Navigate to the quotes route (`/quotes`).
  3. Assert `driver.Title` is not null.
- **Assertions:** `_driver.Title` is not null (page loaded).
- **Dependencies:** React app running at UiBaseUrl (e.g. `http://localhost:5173`); Chrome/ChromeDriver installed.

---

#### LoginPage_LoadsWithoutError

- **Purpose:** Smoke test that the Login page loads without throwing and the browser has a title.
- **Steps:**
  1. Create `LoginPage` with driver and base URL.
  2. Navigate to `/login`.
  3. Assert `driver.Title` is not null.
- **Assertions:** `_driver.Title` is not null.
- **Dependencies:** Same as QuotePage_LoadsWithoutError.

---

## Summary Table

| Test name                                              | Project   | Endpoint / action              | Expected result / assertion summary                    |
|--------------------------------------------------------|-----------|--------------------------------|--------------------------------------------------------|
| CreateCustomer_WithValidPayload_ReturnsSuccess         | ApiTests  | POST /customers                | 2xx, body has Id and matching fields                  |
| CreateCustomer_MissingRequiredField_Returns400         | ApiTests  | POST /customers (empty email)  | 400 Bad Request                                       |
| CreateQuote_WithValidPayload_ReturnsSuccess             | ApiTests  | POST /quotes                   | 2xx, body has Id, CustomerId, Premium                 |
| GetPolicy_NonexistentId_Returns404                     | ApiTests  | GET /policies/999999           | 404 Not Found                                         |
| GetCustomer_WithoutAuthToken_Returns401WhenApiRequiresAuth | ApiTests  | GET /customers/1 (no auth)     | 401, 404, or 200                                      |
| AfterApiCreatesQuote_QuoteExistsInDatabase             | ApiTests  | POST /quotes + DB SELECT       | Quote row exists; Premium matches                      |
| AfterPolicyStatusPatch_StatusUpdatedInDatabase         | ApiTests  | PATCH /policies/1/status + DB  | DB Status = "Cancelled"                               |
| CustomerPolicyRelationship_JoinReturnsCorrectRow      | ApiTests  | DB JOIN only                  | Row with CustomerId 1, PolicyId 1, PolicyNumber set    |
| QuotePage_LoadsWithoutError                             | UiTests   | GET /quotes in browser        | Page loads; Title not null                             |
| LoginPage_LoadsWithoutError                            | UiTests   | GET /login in browser         | Page loads; Title not null                             |

---

## Configuration Reference

Tests read configuration from:

- **ApiTests:** `src/ApiTests/appsettings.json` (copied to output; base path = `AppContext.BaseDirectory` when running tests).
- **UiTests:** `src/UiTests/appsettings.json`.

Relevant keys:

- **BaseApiUrl** – Use **http://localhost:5000** for API tests to avoid HTTPS certificate errors.
- **DatabaseConnectionString** – Required for the three DatabaseValidationTests; if empty, those tests skip (no failure).
- **AuthToken** – Must match the API’s `Auth:ApiKey` for authenticated tests.
- **UiBaseUrl** – Base URL of the React app for UI tests (e.g. `http://localhost:5173`).
