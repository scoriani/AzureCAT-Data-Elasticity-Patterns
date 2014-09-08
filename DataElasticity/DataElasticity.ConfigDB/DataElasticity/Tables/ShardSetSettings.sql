CREATE TABLE [DataElasticity].[ShardSetConfigSettings]
(
	[ShardSetConfigSettingID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY, 
	[ShardSetConfigID] INT NOT NULL , 
    [SettingKey] NVARCHAR(25) NOT NULL, 
    [SettingValue] NVARCHAR(1000) NOT NULL, 
    CONSTRAINT [FK_ShardSetConfigSettings_TableConfigs] FOREIGN KEY ([ShardSetConfigID]) REFERENCES [DataElasticity].[ShardSetConfigs]([ShardSetConfigID])
)

GO

CREATE UNIQUE INDEX [UQ_ShardSetConfigSettings_ShardSetConfigID_SettingKey] ON [DataElasticity].[ShardSetConfigSettings] ([ShardSetConfigID], [SettingKey])
