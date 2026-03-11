-- Minimal schema for insurance demo. Run before seed.sql.
-- Adjust types and constraints to match your actual API/database.

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
CREATE TABLE Customers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(50) NULL
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Policies')
CREATE TABLE Policies (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    PolicyNumber NVARCHAR(50) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    EffectiveDate DATE NULL
);

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Quotes')
CREATE TABLE Quotes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    CustomerId INT NOT NULL,
    ProductCode NVARCHAR(50) NULL,
    Premium DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL
);

GO
