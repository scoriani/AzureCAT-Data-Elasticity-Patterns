CREATE TABLE [DataElasticity].[ShardMaps]
(
	[ShardMapID] INT IDENTITY(1,1) NOT NULL,
	[Active] BIT NOT NULL,
-- Null to allow for non-range shards
    PRIMARY KEY ([ShardMapID]), 
)
