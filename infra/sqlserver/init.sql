IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'HighwayMonitorDb')
BEGIN
    CREATE DATABASE HighwayMonitorDb;
END
GO