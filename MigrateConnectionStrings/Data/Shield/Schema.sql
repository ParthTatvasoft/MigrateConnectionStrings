-- Please replace the string for password which we need to set for encryption. 
-- Please find 'YourStrongPassword!' / 'AnotherStrongPassword!' a place where you need to replace your password.

CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'YourStrongPassword!';
GO
 
CREATE SYMMETRIC KEY SymmetricKey_UserPassword
WITH ALGORITHM = AES_256
ENCRYPTION BY PASSWORD = 'AnotherStrongPassword!';
GO

IF EXISTS (SELECT 1 FROM SYS.OBJECTS WHERE TYPE = 'P' AND NAME = 'InsertAppConfigurations')
BEGIN
	DROP PROCEDURE [dbo].[InsertAppConfigurations]
END
GO

IF EXISTS (SELECT 1 FROM SYS.TYPES WHERE [name] = 'AppConfigurationsType' AND is_user_defined = 1)
BEGIN
	DROP TYPE [dbo].[AppConfigurationsType]
END
GO


CREATE TYPE [dbo].[AppConfigurationsType] AS TABLE(
	[AgencyAppId] [int] NOT NULL,
	[DBConnectionString] [nvarchar](255) NULL,
	[Username] [nvarchar](4000) NULL,
	[Password] [nvarchar](4000) NULL,
	[AgencyId] [int] NOT NULL,
	[Deleted] [bit] NOT NULL
)
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertAppConfigurations]
    @AppConfigurationsTypes AppConfigurationsType READONLY
AS
BEGIN
    -- Open the symmetric key for encryption
    OPEN SYMMETRIC KEY SymmetricKey_UserPassword 
    DECRYPTION BY PASSWORD = 'AnotherStrongPassword!';
    
    -- Insert rows from the table type with encrypted Username and Password
    INSERT INTO [dbo].[AppConfigurations] 
    (
        AgencyAppId, 
        DBConnectionString, 
        AgencyId, 
        Deleted, 
        CreatedBy, 
        Username, 
        [Password]
    )
    SELECT 
        AgencyAppId, 
        DBConnectionString, 
        AgencyId, 
        Deleted, 
        'System', 
        CONVERT(NVARCHAR(4000), EncryptByKey(Key_GUID('SymmetricKey_UserPassword'), [Username]), 1), 
        CONVERT(NVARCHAR(4000), EncryptByKey(Key_GUID('SymmetricKey_UserPassword'), [Password]), 1)
    FROM @AppConfigurationsTypes;
    
    -- Close the symmetric key
    CLOSE SYMMETRIC KEY SymmetricKey_UserPassword;
END;
