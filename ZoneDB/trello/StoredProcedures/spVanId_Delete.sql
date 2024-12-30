CREATE PROCEDURE [trello].[spVanId_Delete]
    @VanName varchar(7)
AS
    begin 
        DELETE
        FROM [trello].[VanId]
        WHERE VanName = @VanName;
    end