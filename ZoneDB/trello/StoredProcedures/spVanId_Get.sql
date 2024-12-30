CREATE PROCEDURE [trello].[spVanId_Get]
    @VanName varchar(7)
AS
    begin 
        SELECT VanId, VanName, Url, Blocked
        FROM [trello].[VanId]
        WHERE VanName = @VanName;
    end