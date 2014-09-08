CREATE TABLE [DataElasticity].[Servers]
(
	[ServerID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
    [Location] VARCHAR(128) NOT NULL, -- Typically a region name, else on-premise
    [ServerName] VARCHAR(128) NOT NULL UNIQUE NONCLUSTERED, 
    [MaxShardsAllowed] INT NOT NULL
)
GO