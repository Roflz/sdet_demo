-- Cleanup script for test data. Use in teardown or between test runs.
-- Order matters due to FKs if your schema has them.

SET NOCOUNT ON;

-- Delete test-created data (adjust WHERE clauses to match your test data patterns)
DELETE FROM Quotes WHERE ProductCode LIKE 'AUTO-%' OR ProductCode LIKE 'SEED-%';
DELETE FROM Policies WHERE PolicyNumber LIKE 'POL-%' OR PolicyNumber LIKE 'SEED-%';
DELETE FROM Customers WHERE Email LIKE 'test%@example.com' OR Email LIKE 'seed%@example.com';

-- Optional: reset identity if needed
-- DBCC CHECKIDENT ('Customers', RESEED, 0);

GO
