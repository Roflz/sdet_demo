-- Seed data for test/CI environments.
-- Run after schema is applied.

SET NOCOUNT ON;

-- Sample customers
IF NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'seed1@example.com')
    INSERT INTO Customers (FirstName, LastName, Email, Phone)
    VALUES ('Seed', 'User1', 'seed1@example.com', '555-1000');

IF NOT EXISTS (SELECT 1 FROM Customers WHERE Email = 'seed2@example.com')
    INSERT INTO Customers (FirstName, LastName, Email, Phone)
    VALUES ('Seed', 'User2', 'seed2@example.com', '555-1001');

-- Sample policies (assumes Customers 1 and 2 exist)
IF NOT EXISTS (SELECT 1 FROM Policies WHERE PolicyNumber = 'SEED-POL-001')
    INSERT INTO Policies (CustomerId, PolicyNumber, Status, EffectiveDate)
    VALUES (1, 'SEED-POL-001', 'Active', GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM Policies WHERE PolicyNumber = 'SEED-POL-002')
    INSERT INTO Policies (CustomerId, PolicyNumber, Status, EffectiveDate)
    VALUES (2, 'SEED-POL-002', 'Pending', GETUTCDATE());

-- Sample quotes
IF NOT EXISTS (SELECT 1 FROM Quotes WHERE CustomerId = 1 AND ProductCode = 'SEED-QUOTE-1')
    INSERT INTO Quotes (CustomerId, ProductCode, Premium, Status)
    VALUES (1, 'SEED-QUOTE-1', 199.99, 'Draft');

GO
