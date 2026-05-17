--liquibase formatted sql

--changeset highway:004 labels:init
USE HighwayMonitorDb;
GO

CREATE TABLE Acidentes (
    Id           UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    Rodovia      NVARCHAR(100)    NOT NULL,
    Km           NVARCHAR(20)     NOT NULL,
    TipoAcidente NVARCHAR(100)    NOT NULL,
    RecebidoEm  DATETIME2        NOT NULL,
    ProcessadoEm DATETIME2        NULL
);
GO