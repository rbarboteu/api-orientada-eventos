--liquibase formatted sql

--changeset highway:001 labels:init
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'HighwayMonitorDb')
BEGIN
    CREATE DATABASE HighwayMonitorDb;
END
GO