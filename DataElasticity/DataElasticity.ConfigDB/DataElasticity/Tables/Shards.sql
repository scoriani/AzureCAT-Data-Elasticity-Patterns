CREATE TABLE [DataElasticity].[Shards] (
    [ShardID]  INT           IDENTITY (1, 1) NOT NULL,
    [ShardSetID] INT           NOT NULL,
    [DatabaseID]      INT           NOT NULL,
    [Description]     VARCHAR (255) NOT NULL,
    PRIMARY KEY CLUSTERED ([ShardID] ASC),
    CONSTRAINT [FK_Shard_Database] FOREIGN KEY ([DatabaseID]) REFERENCES [DataElasticity].[Databases] ([DatabaseID]),
    CONSTRAINT [FK_Shard_ShardSet] FOREIGN KEY ([ShardSetID]) REFERENCES [DataElasticity].[ShardSets] ([ShardSetID])
);

