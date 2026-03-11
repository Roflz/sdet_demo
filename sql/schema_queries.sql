-- Schema reference and example queries for insurance-themed tables.
-- Use these patterns in DbHelper and tests.

-- SELECT single row
-- SELECT Id, CustomerId, ProductCode, Premium, Status FROM Quotes WHERE Id = @id;

-- SELECT with COUNT
-- SELECT COUNT(*) FROM Quotes WHERE CustomerId = @customerId;

-- UPDATE
-- UPDATE Policies SET Status = @status WHERE Id = @id;

-- JOIN: Customer and Policy relationship
-- SELECT c.Id, c.FirstName, c.LastName, p.Id, p.PolicyNumber, p.Status
-- FROM Customers c
-- INNER JOIN Policies p ON p.CustomerId = c.Id
-- WHERE c.Id = @customerId AND p.Id = @policyId;

-- WHERE with multiple conditions
-- SELECT Status FROM Policies WHERE Id = @id;
