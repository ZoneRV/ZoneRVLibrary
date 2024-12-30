CREATE TYPE [trello].[TrelloActionUDT] AS TABLE
(
    ActionId    NVARCHAR(24),
    BoardId     NVARCHAR(24),
    CardId      NVARCHAR(24),
    DateOffset  DATETIMEOFFSET,
    ActionType  NVARCHAR(MAX),
    MemberId    NVARCHAR(24),
    Content     NVARCHAR(MAX),
    CheckId     NVARCHAR(24),
    DueDate     DATETIMEOFFSET
)