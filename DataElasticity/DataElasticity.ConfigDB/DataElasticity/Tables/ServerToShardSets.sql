CREATE TABLE [DataElasticity].[ServerToShardSets]
(
    [ShardSetID] INT NOT NULL,
    [ServerID] INT NOT NULL,
    CONSTRAINT [FK_ServerToShardSet_ShardSet] FOREIGN KEY ([ShardSetID]) REFERENCES [DataElasticity].[ShardSets]([ShardSetID]), 
    CONSTRAINT [FK_ServerToShardSet_Server] FOREIGN KEY ([ServerID]) REFERENCES [DataElasticity].[Servers]([ServerID]), 
    CONSTRAINT [PK_ServerToShardSet] PRIMARY KEY ([ShardSetID], [ServerID])
)
