CREATE TABLE [production].[AreaOfOrigin]
(
    [Id]    INTEGER       NOT NULL PRIMARY KEY IDENTITY,
    [Name]  NVARCHAR(24)  NOT NULL UNIQUE
)