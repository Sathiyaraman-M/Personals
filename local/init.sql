USE master;
GO
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'Personals')
    BEGIN
        CREATE DATABASE Personals;
    END;
GO
USE Personals;
GO