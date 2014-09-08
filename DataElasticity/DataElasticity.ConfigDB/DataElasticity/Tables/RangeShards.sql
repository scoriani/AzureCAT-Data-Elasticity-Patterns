CREATE TABLE [DataElasticity].[RangeShards]
(
	ShardID INT IDENTITY(1,1)  NOT NULL PRIMARY KEY, 
	[ShardMapID] INT NOT NULL,
    [DatabaseID] INT NOT NULL, 
    [RangeLowValue] BIGINT NOT NULL, 
    [RangeHighValue] BIGINT NOT NULL, 
    CONSTRAINT [FK_RangeShard_Server] FOREIGN KEY (DatabaseID) REFERENCES [DataElasticity].[Databases]([DatabaseID]), 
    CONSTRAINT [FK_RangeShards_ShardMapID] FOREIGN KEY ([ShardMapID]) REFERENCES [DataElasticity].[ShardMaps]([ShardMapID])
)
