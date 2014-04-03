CREATE PROCEDURE [dbo].[UserSolutionView]
	@UserId int
AS
	SELECT
		[Solution].Id AS SolutionId, 
		[UserSecurity].Name AS UserName, 
		[Problem].Id AS ProblemId, 
		[Solution].RunTime AS Time, 
		[Solution].RunMemory AS Memory, 
		Language.ToString() AS Language, 
		DATALENGTH(Code) AS CodeLength, 
		Status.ToString() AS Status, 
		CreateTime As CreateTime
	FROM [Solution]
		INNER JOIN [UserSecurity] ON [Solution].UserId = [UserSecurity].Id
		INNER JOIN [Problem] ON [Solution].ProblemId = [Problem].Id
	WHERE [UserId] = @UserId
RETURN 0
