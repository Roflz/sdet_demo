-- Run once after SQL Server is up (e.g. docker compose up -d) to create the database.
-- Execute against 'master' or default DB, then run schema.sql and seed.sql against InsuranceDemo.
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InsuranceDemo')
BEGIN
    CREATE DATABASE InsuranceDemo;
END
GO
