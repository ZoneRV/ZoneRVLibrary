CREATE TABLE [trello].[LocationBayLeader]
(
    [LocationId]   INTEGER       FOREIGN KEY REFERENCES production.Location(Id),
    [BayLeaderId]  NVARCHAR(24)  NOT NULL
)