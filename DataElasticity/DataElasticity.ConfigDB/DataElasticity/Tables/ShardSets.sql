CREATE TABLE [DataElasticity].[ShardSets]
(
	[ShardSetID]	INT				IDENTITY(1,1) NOT NULL PRIMARY KEY,
	[Name]				varchar(128)	NOT NULL,
	[Description]		varchar(max)	NULL, 
    [CurrentShardMapID] INT NULL
)
