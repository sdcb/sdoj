CREATE TABLE [dbo].[Solution]
(
	[Id] INT NOT NULL IDENTITY, 
    [ProblemId] INT NOT NULL, 
    [UserId] INT NOT NULL,  
    [RunTime] INT NOT NULL, 
    [RunMemory] INT NOT NULL, 

	[Language] [dbo].[LanguageUdt] NOT NULL DEFAULT N'CSharp', 
    [Status] [dbo].[SolutionStatusUdt] NOT NULL DEFAULT N'Pending',
	[CreateTime] DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(), 

    [Code] NVARCHAR(MAX) NOT NULL, 
	CONSTRAINT [PK_Solution] PRIMARY KEY ([Id]), 
    CONSTRAINT [FK_Solution_ToProblem] FOREIGN KEY ([ProblemId]) REFERENCES [Problem]([Id]), 
    CONSTRAINT [FK_Solution_ToUser] FOREIGN KEY ([UserId]) REFERENCES [UserSecurity]([Id]), 
    CONSTRAINT [CK_Solution_Code] CHECK (DATALENGTH(Code) < 32768), 
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'Code的长度小于32768。',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Solution',
    @level2type = N'COLUMN',
    @level2name = N'Code'
GO

CREATE INDEX [IX_Solution_CreateTime] ON [dbo].[Solution] ([CreateTime])
