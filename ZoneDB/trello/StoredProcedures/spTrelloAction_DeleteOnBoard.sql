CREATE PROCEDURE [trello].[spTrelloAction_DeleteOnBoard]
    @BoardId varchar(24)
AS
    begin
        Delete
        FROM    trello.[TrelloAction]
        WHERE   BoardId = @BoardId;
    end