CREATE TABLE [DataElasticity].[ShardSetConfigs]
(
	[ShardSetConfigID] INT IDENTITY(1,1) NOT NULL,
    [ShardSetID] INT NOT NULL ,
	[Version] INT NOT NULL,
    [TargetShardCount] INT NOT NULL,
    [MaxShardCount] INT NOT NULL,
	[MaxShardletsPerShard] BIGINT NOT NULL, 
    [MinShardSizeMB] INT NOT NULL, 
    [MaxShardSizeMB] INT NOT NULL, 
    [AllowDeployment] BIT NOT NULL, 
    [ShardMapID] INT NOT NULL, 
    CONSTRAINT [FK_ShardSetConfig_ShardMap] FOREIGN KEY (ShardMapID) REFERENCES [DataElasticity].ShardMaps(ShardMapID), 
    CONSTRAINT [FK_ShardSetConfig_ShardSet] FOREIGN KEY ([ShardSetID]) REFERENCES [DataElasticity].[ShardSets]([ShardSetID]), 
    CONSTRAINT [PK_ShardSetConfigs] PRIMARY KEY ([ShardSetConfigID]) 
)
