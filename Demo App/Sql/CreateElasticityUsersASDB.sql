CREATE LOGIN Superman WITH PASSWORD = 'Blank123';

CREATE USER Superman
FOR LOGIN Superman
WITH DEFAULT_SCHEMA = dbo
GO

-- Add user to the database owner role
EXEC sp_addrolemember N'dbmanager', N'Superman'
EXEC sp_addrolemember N'loginmanager', N'Superman'
GO
