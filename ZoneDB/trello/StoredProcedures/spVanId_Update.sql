CREATE PROCEDURE [trello].[spVanId_Update]
    @VanName  varchar(7),
    @VanId    varchar(24),
    @Url      varchar(1024),
    @Blocked  bit
AS
    begin 
        UPDATE trello.VanId
        SET VanId = @VanId, Url = @Url, Blocked = @Blocked
        WHERE VanName = @VanName;
    end