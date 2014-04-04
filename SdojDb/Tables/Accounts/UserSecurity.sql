CREATE TABLE [dbo].[UserSecurity]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Name] NCHAR(12) NOT NULL, 
    [Email] NVARCHAR(50) NOT NULL, 
    [Password] BINARY(70) NOT NULL DEFAULT 0x0, 
    [Role] [dbo].[RoleUdt] NOT NULL DEFAULT N'User'
)

GO


CREATE UNIQUE INDEX [IX_UserSecurity_Name] ON [dbo].[UserSecurity] ([Name])

GO

CREATE UNIQUE INDEX [IX_UserSecurity_Email] ON [dbo].[UserSecurity] ([Email])

GO
