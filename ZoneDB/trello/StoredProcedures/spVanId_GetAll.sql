CREATE PROCEDURE [trello].[spVanId_GetAll]
AS
    begin 
        SELECT VanId, VanName, Url, Blocked
        FROM trello.[VanId];
    end