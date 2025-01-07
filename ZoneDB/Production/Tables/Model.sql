CREATE TABLE [production].[Model]
(
    [Id]           INTEGER       NOT NULL PRIMARY KEY IDENTITY,
    [Name]         NVARCHAR(1024)  NOT NULL UNIQUE,
    [Description]  NVARCHAR(1024),
    [Prefix]       NVARCHAR(12)  NOT NULL,
    [LineId]       INTEGER  NOT NULL,
    CONSTRAINT [ModelToLineIdFK] FOREIGN KEY ([LineId]) REFERENCES [production].[Line]([Id])
)