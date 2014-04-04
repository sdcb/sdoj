SET IDENTITY_INSERT [UserSecurity] ON;

MERGE INTO [UserSecurity] AS TARGET
USING(VALUES
	(1, N'Admin', N'397482054@qq.com', PWDENCRYPT(N'123456'), N'Admin'), 
	(2, N'Flysha', N'sdflysha@qq.com', PWDENCRYPT(N'123456'), N'User')
) AS SOURCE(Id, Name, Email, Password, Role) 
ON Target.Id = Source.Id
WHEN MATCHED THEN
	UPDATE SET 
		Name = Source.Name, 
		Email = Source.Email, 
		Password = Source.Password, 
		Role = Source.Role
WHEN NOT MATCHED BY TARGET THEN
	INSERT (Id, Name, Email, Password, Role)
	VALUES (Id, Name, Email, Password, Role)
WHEN NOT MATCHED BY SOURCE THEN
	DELETE;

SET IDENTITY_INSERT [UserSecurity] OFF;
SET IDENTITY_INSERT [Problem] ON;

MERGE INTO [Problem] AS TARGET
USING(VALUES
	(1, N'Hello World', 1000, 65536, N'This is some description.', N'this is the input description.', N'This is the output description.'), 
	(2, N'你好，世界', 2000, 32768, N'这是一些描述。这是另一些描述。', N'这是输入数据的描述。', N'这是输出数据的描述。')
) AS SOURCE(Id, Title, TimeLimit, MemoryLimit, Description, Input, Output) 
ON Target.Id = Source.Id
WHEN MATCHED THEN
	UPDATE SET 
		TimeLimit = Source.TimeLimit,
		MemoryLimit = Source.MemoryLimit, 
		Description = Source.Description, 
		Input = Source.Input, 
		Output = Source.Output
WHEN NOT MATCHED BY TARGET THEN
	INSERT (Id, Title, TimeLimit, MemoryLimit, Description, Input, Output)
	VALUES (Id, Title, TimeLimit, MemoryLimit, Description, Input, Output)
WHEN NOT MATCHED BY SOURCE THEN
	DELETE;
	
SET IDENTITY_INSERT [Problem] OFF;
SET IDENTITY_INSERT [Solution] ON;


MERGE INTO [Solution] AS TARGET
USING(VALUES
	(1, 1, 1, 567, 12345, N'CSharp', N'Accepted', N'Console.Write("Hello World");'), 
	(2, 2, 2, 345, 2345, N'Cpp', N'Accepted', N'printf("Hello World\n");') 
) AS SOURCE(Id, ProblemId, UserId, RunTime, RunMemory, Language, Status, Code)
ON Target.Id = Source.Id
WHEN MATCHED THEN
	UPDATE SET 
		ProblemId = Source.ProblemId, 
		UserId = Source.UserId, 
		RunTime = Source.RunTime, 
		RunMemory = Source.RunMemory, 
		Language = Source.Language, 
		Status = Source.Status, 
		Code = Source.Code
WHEN NOT MATCHED BY TARGET THEN
	INSERT(Id, ProblemId, UserId, RunTime, RunMemory, Language, Status, Code)
	VALUES(Id, ProblemId, UserId, RunTime, RunMemory, Language, Status, Code)
WHEN NOT MATCHED BY SOURCE THEN
	DELETE;

SET IDENTITY_INSERT [Solution] OFF;