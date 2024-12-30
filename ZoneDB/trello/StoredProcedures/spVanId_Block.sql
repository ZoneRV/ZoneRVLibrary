CREATE PROCEDURE [trello].[spVanId_Block]
    @VanName varchar(7),
    @Blocked bit
AS
    begin 
        UPDATE trello.VanId
        SET Blocked = @Blocked
        WHERE VanName = @VanName;
    end