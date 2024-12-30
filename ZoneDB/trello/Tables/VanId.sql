Create Table [Trello].[VanId]
(
    [Id]       INTEGER        NOT NULL  PRIMARY KEY IDENTITY,
    [VanName]  varchar(7)     NOT NULL,
    [VanId]    varchar(24)    NOT NULL  DEFAULT '',
    [Url]      varchar(1024)  NOT NULL  DEFAULT '',
    [Blocked]  BIT            NOT NULL  DEFAULT 0
)