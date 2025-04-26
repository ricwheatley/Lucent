-- Lucent loader core (generated 2025-04-25)
USE [Lucent];
GO
------------------------------------------------------------
-- 1. Control table to track per-endpoint incremental loads
------------------------------------------------------------
IF OBJECT_ID('dbo.EndpointLoadControl','U') IS NULL
BEGIN
    CREATE TABLE dbo.EndpointLoadControl(
        EndpointName       nvarchar(128) PRIMARY KEY,
        ModifiedSinceUTC   datetime2(7)      NULL,  -- watermark passed to Xero
        LastRunUTC         datetime2(7)      NULL,  -- when loader started
        LastSuccessUTC     datetime2(7)      NULL,  -- successful merge time
        RowsInserted       bigint            NULL,
        RowsUpdated        bigint            NULL,
        LastError          nvarchar(max)     NULL
    );
END
GO
------------------------------------------------------------
-- 2. Generic MERGE proc landing.[Ep] -> ods.[Ep]
------------------------------------------------------------
IF OBJECT_ID('dbo.usp_MergeLandingToODS','P') IS NOT NULL
    DROP PROCEDURE dbo.usp_MergeLandingToODS;
GO
CREATE PROCEDURE dbo.usp_MergeLandingToODS
    @EndpointName nvarchar(128)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @pkCols nvarchar(max) =
    (SELECT STRING_AGG(c.name, ',')
       FROM sys.indexes i
       JOIN sys.index_columns ic ON ic.object_id=i.object_id AND ic.index_id=i.index_id
       JOIN sys.columns c ON c.object_id=i.object_id AND c.column_id=ic.column_id
      WHERE i.object_id = OBJECT_ID(QUOTENAME('ods') + '.' + QUOTENAME(@EndpointName))
        AND i.is_primary_key = 1);

    IF (@pkCols IS NULL)
    BEGIN
        RAISERROR ('ods.%s has no primary key, cannot merge',16,1,@EndpointName);
        RETURN;
    END

    DECLARE @merge nvarchar(max) = N'WITH src AS (SELECT * FROM landing.'+QUOTENAME(@EndpointName)+')
    MERGE ods.'+QUOTENAME(@EndpointName)+' AS tgt
    USING src ON ' + STRING_AGG('tgt.'+QUOTENAME(value)+' = src.'+QUOTENAME(value), ' AND ')
                  WITHIN GROUP (ORDER BY value)
        FROM string_split(@pkCols, ',') +
    N' WHEN MATCHED THEN UPDATE SET ' +
      (SELECT STRING_AGG('tgt.'+QUOTENAME(name)+' = src.'+QUOTENAME(name), ', ')
         FROM sys.columns
        WHERE object_id = OBJECT_ID(QUOTENAME('landing')+'.'+QUOTENAME(@EndpointName))) +
    N' WHEN NOT MATCHED BY TARGET THEN INSERT(' +
      (SELECT STRING_AGG(QUOTENAME(name), ', ')
         FROM sys.columns
        WHERE object_id = OBJECT_ID(QUOTENAME('landing')+'.'+QUOTENAME(@EndpointName))) +
    N') VALUES(' +
      (SELECT STRING_AGG('src.'+QUOTENAME(name), ', ')
         FROM sys.columns
        WHERE object_id = OBJECT_ID(QUOTENAME('landing')+'.'+QUOTENAME(@EndpointName))) + N');';

    EXEC (@merge);
END
GO
