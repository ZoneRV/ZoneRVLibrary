CREATE PROCEDURE [trello].[spLocationsWithListNames_GetAll]
AS
    begin
        SELECT *
        FROM [trello].[ListNameToLocation]
        FULL JOIN [production].[Location] On ListNameToLocation.LocationId = Location.Id;
    end