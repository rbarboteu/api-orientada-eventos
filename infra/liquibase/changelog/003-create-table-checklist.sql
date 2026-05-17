--liquibase formatted sql

--changeset highway:003 labels:init
USE HighwayMonitorDb;
GO

CREATE TABLE ItensChecklist (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
    InspecaoId  UNIQUEIDENTIFIER NOT NULL,
    Ordem       INT              NOT NULL,
    Descricao   NVARCHAR(255)    NOT NULL,
    Conforme    BIT              NOT NULL,

    CONSTRAINT FK_ItensChecklist_Inspecoes
        FOREIGN KEY (InspecaoId)
        REFERENCES Inspecoes(Id)
);
GO

-- Índice para garantir que a consulta por ORDEM seja rápida
CREATE INDEX IX_ItensChecklist_InspecaoId_Ordem
    ON ItensChecklist (InspecaoId, Ordem);
GO