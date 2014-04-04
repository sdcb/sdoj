CREATE TABLE [dbo].[Problem]
(
	[Id] INT NOT NULL IDENTITY, 
	[Title] NVARCHAR(40) NOT NULL, 
    [TimeLimit] INT NOT NULL DEFAULT 1000, 
    [MemoryLimit] INT NOT NULL DEFAULT 65536, 
    [Description] NVARCHAR(MAX) NOT NULL, 
    [Input] NVARCHAR(MAX) NOT NULL, 
    [Output] NVARCHAR(MAX) NOT NULL, 

	CONSTRAINT [PK_Problem] PRIMARY KEY ([Id]), 
)

GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'单位为毫秒。',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Problem',
    @level2type = N'COLUMN',
    @level2name = N'TimeLimit'
GO
EXEC sp_addextendedproperty @name = N'MS_Description',
    @value = N'单位为KB。',
    @level0type = N'SCHEMA',
    @level0name = N'dbo',
    @level1type = N'TABLE',
    @level1name = N'Problem',
    @level2type = N'COLUMN',
    @level2name = N'MemoryLimit'
GO


CREATE INDEX [IX_Problem_Title] ON [dbo].[Problem] ([Title])

GO