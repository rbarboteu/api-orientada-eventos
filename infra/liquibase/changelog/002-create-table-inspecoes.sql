--liquibase formatted sql

--changeset highway:002 labels:init
USE HighwayMonitorDb;
GO

CREATE TABLE Inspecoes (
    Id            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    Rodovia       NVARCHAR(100)    NOT NULL,
    KmInicial     NVARCHAR(20)     NOT NULL,
    KmFinal       NVARCHAR(20)     NOT NULL,
    FiscalId      NVARCHAR(50)     NOT NULL,
    Status        NVARCHAR(20)     NOT NULL DEFAULT 'PROCESSANDO',
    RecebidoEm   DATETIME2        NOT NULL,
    ProcessadoEm DATETIME2        NULL
);
GO