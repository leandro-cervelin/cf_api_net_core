-- Creates the CF database (if missing) and a least-privilege login for the API.
-- Run as sa by the db-init service in docker-compose.yml. Safe to run repeatedly.
-- Scripting variables (from the environment): AppLogin, AppPassword.
-- AppPassword must meet SQL Server's password policy and must not contain a single quote.

IF DB_ID(N'CF') IS NULL
    CREATE DATABASE [CF];
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$(AppLogin)')
    CREATE LOGIN [$(AppLogin)] WITH PASSWORD = N'$(AppPassword)', CHECK_POLICY = ON, DEFAULT_DATABASE = [CF];
ELSE
    ALTER LOGIN [$(AppLogin)] WITH PASSWORD = N'$(AppPassword)';
GO

USE [CF];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$(AppLogin)')
    CREATE USER [$(AppLogin)] FOR LOGIN [$(AppLogin)];

-- Data only: no DDL (schema changes go through the migration bundle as sa) and no access to other databases.
ALTER ROLE db_datareader ADD MEMBER [$(AppLogin)];
ALTER ROLE db_datawriter ADD MEMBER [$(AppLogin)];
GO

PRINT N'Login $(AppLogin) is ready.';
