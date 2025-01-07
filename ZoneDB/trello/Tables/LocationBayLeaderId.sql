CREATE TABLE [trello].[LocationBayLeader]
(
    [Id]           INTEGER       NOT NULL PRIMARY KEY IDENTITY,
    [LocationId]   INTEGER       NOT NULL,
    [BayLeaderId]  NVARCHAR(24)  NOT NULL,
    CONSTRAINT [BayLeaderToLocationIdFK] FOREIGN KEY ([LocationId]) REFERENCES [production].[Location]([Id])
)