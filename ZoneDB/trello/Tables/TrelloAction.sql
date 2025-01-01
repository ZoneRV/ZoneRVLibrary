CREATE TABLE [trello].[TrelloAction]
(
    [Id]          INTEGER         NOT NULL PRIMARY KEY IDENTITY,
    [ActionId]    NVARCHAR(24)    NOT NULL UNIQUE,
    [BoardId]     NVARCHAR(24)    NOT NULL,
    [CardId]      NVARCHAR(24)    NOT NULL,
    [DateOffset]  DATETIMEOFFSET  NOT NULL,
    [ActionType]  NVARCHAR(MAX)   NOT NULL,
    [MemberId]    NVARCHAR(24)    NOT NULL,
    [Content]     NVARCHAR(MAX)   NULL,
    [CheckId]     NVARCHAR(24)    NULL,
    [DueDate]     DATETIMEOFFSET  NULL
)