CREATE PROCEDURE [trello].[spTrelloAction_Insert]
    @ActionId    NVARCHAR(24), 
    @BoardId     NVARCHAR(24), 
    @CardId      NVARCHAR(24), 
    @DateOffset  DateTimeOffset, 
    @ActionType  NVARCHAR(max), 
    @MemberId    NVARCHAR(24), 
    @Content     NVARCHAR(max),
    @CheckId     NVARCHAR(24),
    @DueDate     DATETIMEOFFSET
AS
    begin 
        INSERT INTO trello.[TrelloAction](ActionId, BoardId, CardId, DateOffset, ActionType, MemberId, Content, CheckId, DueDate)
        VALUES (@ActionId, @BoardId, @CardId, @DateOffset, @ActionType, @MemberId, @Content, @CheckId, @DueDate)
    end