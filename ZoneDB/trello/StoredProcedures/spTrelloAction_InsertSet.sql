CREATE PROCEDURE [trello].[spTrelloAction_InsertSet]
    @actions TrelloActionUDT READONLY
AS
    begin 
        INSERT INTO [trello].[TrelloAction] (ActionId,   BoardId,   CardId,   DateOffset,   ActionType,   MemberId,   Content,   CheckId,   DueDate)
        SELECT                              [ActionId], [BoardId], [CardId], [DateOffset], [ActionType], [MemberId], [Content], [CheckId], [DueDate]
        FROM @actions;
    end