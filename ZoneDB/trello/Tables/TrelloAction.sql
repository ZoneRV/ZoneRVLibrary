CREATE TABLE [trello].[TrelloAction]
(
    [ActionId]    NVARCHAR(24)    NOT NULL  PRIMARY KEY UNIQUE,
    [BoardId]     NVARCHAR(24)    NOT NULL,
    [CardId]      NVARCHAR(24)    NOT NULL,
    [DateOffset]  DATETIMEOFFSET  NOT NULL,
    [ActionType]  NVARCHAR(MAX)   NOT NULL,
    [MemberId]    NVARCHAR(24)    NOT NULL,
    [Content]     NVARCHAR(MAX)   NULL,
    [CheckId]     NVARCHAR(24)    NULL,
    [DueDate]     DATETIMEOFFSET  NULL
)