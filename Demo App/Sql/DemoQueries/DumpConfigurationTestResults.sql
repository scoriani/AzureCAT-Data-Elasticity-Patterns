
/*** RUN THIS FROM THE CONFIGURATION DATABASE (AWMain) ******/

SELECT * FROM [DataElasticity].[ShardSets]
SELECT * FROM [DataElasticity].[ShardSetConfigs]
SELECT * FROM [DataElasticity].[ShardSetConfigSettings]
SELECT * FROM [DataElasticity].[Servers]
SELECT * FROM [DataElasticity].[ServerToShardSets]
SELECT * FROM [DataElasticity].[ShardMaps]
SELECT * FROM [DataElasticity].[Databases] ORDER BY [DataElasticity].[Databases].[DatabaseID] 
SELECT * FROM [DataElasticity].[RangeShards]
SELECT * FROM [DataElasticity].[Shards]

	


