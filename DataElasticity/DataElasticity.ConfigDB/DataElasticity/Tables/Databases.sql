CREATE TABLE [DataElasticity].[Databases] (
    [DatabaseID]   INT           IDENTITY (1, 1) NOT NULL,
    [DatabaseName] VARCHAR (128) NOT NULL,
    [ServerID]     INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([DatabaseID] ASC),
    CONSTRAINT [FK_Databases_ToServers] FOREIGN KEY ([ServerID]) REFERENCES [DataElasticity].[Servers] ([ServerID]),
    CONSTRAINT [UQ__Database__6F513197AF0E91F0] UNIQUE NONCLUSTERED ([DatabaseName] ASC, [ServerID] ASC)
);



GO
