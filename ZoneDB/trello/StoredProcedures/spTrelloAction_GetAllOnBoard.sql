CREATE PROCEDURE [trello].[spTrelloAction_GetAllOnBoard]
    @BoardId varchar(24)
AS
    begin 
        SELECT   *
        FROM     trello.[TrelloAction]
        WHERE    BoardId = @BoardId
        ORDER BY DateOffset;
    end