CREATE TABLE [DataElasticity].[Settings] (
    [Version]           INT             NOT NULL,
    [DateCreated]       DATETIME        NOT NULL,
    [ShardPrefix]       VARCHAR (10)    NOT NULL,
    [AdminUserName]     VARCHAR (15)    NOT NULL,
    [AdminUserPassword] VARBINARY (128) NOT NULL,
    [ShardUserName]     VARCHAR (15)    NOT NULL,
    [ShardUserPassword] VARBINARY (128) NOT NULL,
    CONSTRAINT [PK_CDX_Common_Version] PRIMARY KEY CLUSTERED ([Version] ASC)
);


