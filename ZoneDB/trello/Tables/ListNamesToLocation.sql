CREATE TABLE [trello].[ListNameToLocation]
(
    [Id]          INTEGER         NOT NULL PRIMARY KEY IDENTITY,
    [Name]        NVARCHAR(1024)  NOT NULL,
    [LocationId]  INTEGER         NOT NULL,
    CONSTRAINT [TrelloListToLocationIdFK] FOREIGN KEY ([LocationId]) REFERENCES [production].[Location]([Id])
)