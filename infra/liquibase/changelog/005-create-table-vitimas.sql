--liquibase formatted sql

--changeset highway:005 labels:init
USE HighwayMonitorDb;
GO

CREATE TABLE Vitimas (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    AcidenteId      UNIQUEIDENTIFIER NOT NULL,
    NomeSanitizado  NVARCHAR(200)    NOT NULL,
    Documento       NVARCHAR(20)     NOT NULL,
    TipoDocumento   NVARCHAR(10)     NOT NULL,

    CONSTRAINT FK_Vitimas_Acidentes
        FOREIGN KEY (AcidenteId)
        REFERENCES Acidentes(Id)
);
GO