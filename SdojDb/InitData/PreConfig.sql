-- 确保打开CLR的功能。
DECLARE @clrEnabled sql_variant;
SELECT @clrEnabled = value FROM sys.configurations WHERE name = 'clr enabled';
IF @clrEnabled = 0
BEGIN
	EXEC sp_configure 'clr enabled', 1;
	RECONFIGURE;
END