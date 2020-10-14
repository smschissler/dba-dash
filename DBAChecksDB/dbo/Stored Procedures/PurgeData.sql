﻿CREATE PROC [dbo].[PurgeData]
AS
DECLARE @TableName SYSNAME,@RetentionDays INT
DECLARE cTables CURSOR FAST_FORWARD LOCAL FOR
	SELECT DR.TableName,DR.RetentionDays
	FROM dbo.DataRetention DR
	CROSS APPLY [dbo].[PartitionFunctionName](DR.TableName) PF
	WHERE RetentionDays > 0
	

OPEN cTables
WHILE 1=1
BEGIN
	FETCH NEXT FROM cTables INTO @TableName,@RetentionDays
	IF @@FETCH_STATUS<>0
		BREAK
	EXEC [dbo].[PartitionTable_Cleanup] @TableName=@TableName,@DaysToKeep=@RetentionDays
END

CLOSE cTables
DEALLOCATE cTables