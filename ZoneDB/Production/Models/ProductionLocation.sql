CREATE TABLE [production].[Location]
(
    [Id]           INTEGER         NOT NULL PRIMARY KEY IDENTITY,
    [LineId]       INTEGER         NOT NULL ,
    [Name]         NVARCHAR(24)    NOT NULL UNIQUE,
    [Description]  NVARCHAR(1024)  NOT NULL DEFAULT '',
    [Order]        DECIMAL         NOT NULL, 
    CONSTRAINT [LineIdFK] FOREIGN KEY ([LineId]) REFERENCES [production].[Location]([Id])
)
