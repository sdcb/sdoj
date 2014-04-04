CREATE FULLTEXT CATALOG [ProblemCatagory];

GO;

CREATE FULLTEXT INDEX
	ON [dbo].[Problem]
		([Title], [Description], [Input], [Output])
	KEY INDEX [PK_Problem]
	ON [ProblemCatagory]
	WITH CHANGE_TRACKING AUTO
