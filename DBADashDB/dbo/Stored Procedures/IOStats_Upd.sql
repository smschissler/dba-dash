﻿CREATE PROC dbo.IOStats_Upd(
		@IOStats dbo.IOStats READONLY,
		@InstanceID INT,
		@SnapshotDate DATETIME2(2)
)
AS
SET XACT_ABORT ON
DECLARE @Ref VARCHAR(30)='IOStats'
DECLARE @EngineEdition INT 
SELECT @EngineEdition = EngineEdition
FROM dbo.Instances 
WHERE InstanceID = @InstanceID

CREATE TABLE #DBIOStatsTemp(
	InstanceID INT NOT NULL,
	DatabaseID INT NOT NULL,
	Drive CHAR(1) COLLATE DATABASE_DEFAULT NOT NULL ,
	FileID INT NOT NULL,
	SnapshotDate DATETIME2(2) NOT NULL,
	SnapshotDate60 DATETIME2(2) NOT NULL,
	num_of_reads BIGINT NOT NULL,
	num_of_writes BIGINT NOT NULL,
	num_of_bytes_read BIGINT NOT NULL,
	num_of_bytes_written BIGINT NOT NULL,
	io_stall_read_ms BIGINT NOT NULL,
	io_stall_write_ms BIGINT NOT NULL,
	sample_ms_diff BIGINT NOT NULL,
	size_on_disk_bytes BIGINT NOT NULL,
	MaxReadLatency AS ISNULL(io_stall_read_ms/NULLIF(num_of_reads*1.0,0),0),
	MaxWriteLatency AS ISNULL(io_stall_write_ms/NULLIF(num_of_writes*1.0,0),0),
	MaxLatency AS ISNULL((io_stall_read_ms+io_stall_write_ms)/NULLIF(num_of_writes+num_of_reads*1.0,0),0),
	MaxReadIOPs AS num_of_reads/(sample_ms_diff/1000.0),
	MaxWriteIOPs AS num_of_writes/(sample_ms_diff/1000.0),
	MaxIOPs AS  (num_of_reads+num_of_writes)/(sample_ms_diff/1000.0),
	MaxReadMBsec AS	 num_of_bytes_read/(sample_ms_diff/1000.0)/POWER(1024.0,2),
	MaxWriteMBsec AS num_of_bytes_written/(sample_ms_diff/1000.0)/POWER(1024.0,2),
	MaxMBsec AS	 (num_of_bytes_written+num_of_bytes_read)/(sample_ms_diff/1000.0)/POWER(1024.0,2), 	
	PRIMARY KEY(InstanceID,DatabaseID,Drive,FileID,	SnapshotDate)
)
DECLARE @IOStorageBitMask TINYINT
DECLARE @Grp NVARCHAR(MAX)
/*	
	Configuration setting to control what level we store IO stats. 
	1 = Instance
	2 = File level
	4 = DB
	8 = DB, Drive
	16 = Drive
	31 = Everything
*/
SELECT @IOStorageBitMask = CAST(SettingValue AS TINYINT)
FROM dbo.InstanceSettings
WHERE InstanceID = @InstanceID
AND SettingName = 'IOStorageBitMask'

/* Azure DB = Instance and file level */
SELECT @IOStorageBitMask = ISNULL(@IOStorageBitMask,31) & CASE WHEN @EngineEdition IN(1,2,3,4) THEN 31 ELSE 3 END

IF @IOStorageBitMask = 0
BEGIN
	RETURN;
END

SELECT @Grp = STUFF((SELECT REPLICATE(CHAR(9),3) + ',' + Grp + CHAR(13) + CHAR(10)
FROM (VALUES('(x.InstanceID)',1),
		('(x.DatabaseID,x.FileID,x.Drive)',2),
		('(x.DatabaseID)',4),
		('(x.DatabaseID,x.Drive)',8),
		('(x.Drive)',16)
		) T(Grp,BitMask)
WHERE BitMask & @IOStorageBitMask > 0
FOR XML PATH(''),TYPE).value('.','NVARCHAR(MAX)'),1,4,REPLICATE(CHAR(9),3))

DECLARE @SQL NVARCHAR(MAX)
SET @SQL = N'SELECT @InstanceID,
			A.SnapshotDate,
			DG.DateGroup AS SnapshotDate60,
			' + CASE WHEN @IOStorageBitMask & 14 > 0 THEN 'ISNULL(x.DatabaseID,-1)' ELSE '-1' END + ' AS DatabaseID,
			' + CASE WHEN @IOStorageBitMask & 26 > 0 THEN 'COALESCE(x.Drive,''*'')' ELSE '''*''' END + ' AS Drive,
			' + CASE WHEN @IOStorageBitMask & 2 > 0 THEN 'ISNULL(x.FileID,-1)' ELSE '-1' END + ' AS FileID,
			MAX(A.sample_ms-B.sample_ms) AS sample_ms_diff,
			SUM(A.num_of_reads-B.num_of_reads),
			SUM(A.num_of_bytes_read-B.num_of_bytes_read),
			SUM(A.io_stall_read_ms-B.io_stall_read_ms),
			SUM(A.num_of_writes-B.num_of_writes),
			SUM(A.num_of_bytes_written-B.num_of_bytes_written),
			SUM(A.io_stall_write_ms-B.io_stall_write_ms),
			SUM(A.size_on_disk_bytes)
FROM @IOStats a
JOIN Staging.IOStats b ON b.database_id = a.database_id AND b.file_id = a.file_id AND a.drive = b.drive AND b.InstanceID=@InstanceID
LEFT JOIN dbo.Databases D ON D.database_id = a.database_id AND d.InstanceID=@InstanceID AND D.IsActive=1
LEFT JOIN dbo.DBFiles F ON F.file_id = a.file_id AND F.DatabaseID = D.DatabaseID AND F.IsActive=1 
CROSS APPLY dbo.DateGroupingMins(A.SnapshotDate,60) DG
CROSS APPLY(SELECT ISNULL(D.DatabaseID,-999) AS DatabaseID,
					ISNULL(F.FileID,-999) AS FileID,
					CASE WHEN a.drive LIKE ''[A-Z,-]'' THEN a.drive WHEN F.physical_name LIKE ''_:\%'' THEN LEFT(F.physical_name,1) ELSE ''?'' END AS Drive,
					@InstanceID AS InstanceID
					) x
WHERE A.sample_ms > b.sample_ms
			AND A.SnapshotDate > B.SnapshotDate
			AND A.num_of_bytes_read>=B.num_of_bytes_read
			AND A.num_of_reads>=B.num_of_reads
			AND A.num_of_writes>=B.num_of_writes
			AND A.num_of_bytes_written>= B.num_of_bytes_written
GROUP BY GROUPING SETS(
			' + @Grp + '
			)
		,a.SnapshotDate,DG.DateGroup
HAVING(
		SUM(A.num_of_writes-B.num_of_writes)>0 
		OR SUM(A.num_of_reads-B.num_of_reads)>0
	   )'

INSERT INTO #DBIOStatsTemp
(
	InstanceID,
	SnapshotDate,
	SnapshotDate60,
	DatabaseID,
	Drive,
	FileID,
	sample_ms_diff,
	num_of_reads,
	num_of_bytes_read,
	io_stall_read_ms,
	num_of_writes,
	num_of_bytes_written,
	io_stall_write_ms,
	size_on_disk_bytes
)
EXEC sp_executesql @SQL,N'@IOStats IOStats READONLY,@InstanceID INT',@IOStats,@InstanceID

/* 
	Depending on the IO Collection Level, the database_id & file_id could be set to -1 and Drive could be set to "-".  
	These won't join to an existing DB/file in the repository DB so DatabaseID and FileID get set to -999 in this case.
	We just want to keep the aggregates produced by GROUPING SETS in this case.
*/
DELETE #DBIOStatsTemp 
WHERE InstanceID = @InstanceID 
AND (DatabaseID = -999 OR FileID=-999 OR Drive = '-')



BEGIN TRAN

IF NOT EXISTS(
	/* Check data hasn't been imported already */
	SELECT 1
	FROM #DBIOStatsTemp t
	WHERE EXISTS(SELECT 1 
				FROM dbo.DBIOStats IOS 
				WHERE t.InstanceID = IOS.InstanceID
				AND t.DatabaseID = IOS.DatabaseID
				AND t.Drive = IOS.Drive
				AND t.FileID = IOS.FileID
				AND t.SnapshotDate = IOS.SnapshotDate			
				)
	)
BEGIN

	/* Insert granular IO metrics */
	INSERT INTO dbo.DBIOStats
	(
		InstanceID,
		SnapshotDate,
		DatabaseID,
		Drive,
		FileID,
		sample_ms_diff,
		num_of_reads,
		num_of_bytes_read,
		io_stall_read_ms,
		num_of_writes,
		num_of_bytes_written,
		io_stall_write_ms,
		size_on_disk_bytes
	)
	SELECT 	InstanceID,
		SnapshotDate,
		DatabaseID,
		Drive,
		FileID,
		sample_ms_diff,
		num_of_reads,
		num_of_bytes_read,
		io_stall_read_ms,
		num_of_writes,
		num_of_bytes_written,
		io_stall_write_ms,
		size_on_disk_bytes
	FROM #DBIOStatsTemp t

	/* Update 60min aggregation */
	UPDATE T 
			SET T.num_of_reads += s.num_of_reads,
				T.num_of_writes += S.num_of_writes,
				T.num_of_bytes_read += S.num_of_bytes_read,
				T.num_of_bytes_written += S.num_of_bytes_written,
				T.io_stall_read_ms += S.io_stall_read_ms,
				T.io_stall_write_ms += S.io_stall_write_ms,
				T.sample_ms_diff += S.sample_ms_diff,
				T.MaxReadLatency = CASE WHEN S.MaxReadLatency > t.MaxReadLatency THEN S.MaxReadLatency ELSE T.MaxReadLatency END,
				T.MaxWriteLatency = CASE WHEN S.MaxWriteLatency > t.MaxWriteLatency THEN S.MaxReadLatency ELSE T.MaxReadLatency END,
				T.MaxLatency = CASE WHEN S.MaxLatency > t.MaxLatency THEN S.MaxLatency ELSE T.MaxLatency END,
				T.MaxReadIOPs = CASE WHEN S.MaxReadIOPs > t.MaxReadIOPs THEN S.MaxReadIOPs ELSE T.MaxReadIOPs END,
				T.MaxWriteIOPs = CASE WHEN S.MaxWriteIOPs > t.MaxWriteIOPs THEN S.MaxWriteIOPs ELSE T.MaxWriteIOPs END,
				T.MaxIOPs = CASE WHEN S.MaxIOPs > t.MaxIOPs THEN S.MaxIOPs ELSE T.MaxIOPs END,
				T.MaxReadMBsec = CASE WHEN S.MaxReadMBsec > t.MaxReadMBsec THEN S.MaxReadMBsec ELSE T.MaxReadMBsec END,
				T.MaxWriteMBsec = CASE WHEN S.MaxWriteMBsec > t.MaxWriteMBsec THEN S.MaxWriteMBsec ELSE T.MaxWriteMBsec END,
				T.MaxMBsec =CASE WHEN S.MaxMBsec > t.MaxMBsec THEN S.MaxMBsec ELSE T.MaxMBsec END
	FROM dbo.DBIOStats_60MIN T 
	JOIN #DBIOStatsTemp S ON T.InstanceID = S.InstanceID
			AND S.DatabaseID = T.DatabaseID
			AND S.Drive = T.Drive
			AND S.FileID = T.FileID
			AND S.SnapshotDate60 = T.SnapshotDate

	INSERT INTO dbo.DBIOStats_60MIN(
		InstanceID,
		DatabaseID,
		Drive,
		FileID,
		SnapshotDate,
		num_of_reads,
		num_of_writes,
		num_of_bytes_read,
		num_of_bytes_written,
		io_stall_read_ms,
		io_stall_write_ms,
		sample_ms_diff,
		MaxReadLatency,
		MaxWriteLatency,
		MaxLatency,
		MaxReadIOPs,
		MaxWriteIOPs,
		MaxIOPs,
		MaxReadMBsec,
		MaxWriteMBsec,
		MaxMBsec
	)
	SELECT  InstanceID,
			DatabaseID,
			Drive,
			FileID,
			SnapshotDate60,
			num_of_reads,
			num_of_writes,
			num_of_bytes_read,
			num_of_bytes_written,
			io_stall_read_ms,
			io_stall_write_ms,
			sample_ms_diff,
			MaxReadLatency,
			MaxWriteLatency,
			MaxLatency,
			MaxReadIOPs,
			MaxWriteIOPs,
			MaxIOPs,
			MaxReadMBsec,
			MaxWriteMBsec,
		MaxMBsec
	FROM #DBIOStatsTemp S
	WHERE NOT EXISTS(
				SELECT 1 
				FROM dbo.DBIOStats_60MIN T
				WHERE T.InstanceID = S.InstanceID
				AND S.DatabaseID = T.DatabaseID
				AND S.Drive = T.Drive
				AND S.FileID = T.FileID
				AND S.SnapshotDate60 = T.SnapshotDate
				)

END
/* Replace data in staging table */
DELETE Staging.IOStats
WHERE InstanceID=@InstanceID

INSERT INTO Staging.IOStats
(
    InstanceID,
    SnapshotDate,
    database_id,
    file_id,
    sample_ms,
    num_of_reads,
    num_of_bytes_read,
    io_stall_read_ms,
    num_of_writes,
    num_of_bytes_written,
    io_stall_write_ms,
    io_stall,
    size_on_disk_bytes,
	drive
)
SELECT @InstanceID InstanceID,
    SnapshotDate,
    database_id,
    file_id,
    sample_ms,
    num_of_reads,
    num_of_bytes_read,
    io_stall_read_ms,
    num_of_writes,
    num_of_bytes_written,
    io_stall_write_ms,
    io_stall,
    size_on_disk_bytes,
	drive
FROM @IOStats

COMMIT

EXEC dbo.CollectionDates_Upd @InstanceID = @InstanceID,  
										@Reference = @Ref,
										@SnapshotDate = @SnapshotDate