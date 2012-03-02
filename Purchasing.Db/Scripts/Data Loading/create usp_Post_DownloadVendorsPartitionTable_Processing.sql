USE [PrePurchasing]
GO
/****** Object:  StoredProcedure [dbo].[usp_Post_DownloadVendorsPartitionTable_Processing]    Script Date: 03/01/2012 13:31:26 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ken Taylor
-- Create date: March 01, 2012
-- Description:	Restoring Vendors indexes after downloading vendors data and swapping with load table.
-- =============================================
CREATE PROCEDURE [dbo].[usp_Post_DownloadVendorsPartitionTable_Processing]
	-- Add the parameters for the stored procedure here
	@LoadTableName varchar(255) = 'vVendors_Load', --Table name of load table being loaded 
	@ReferentialTableName varchar(244) = '', --Name of table being referenced; N/A in this case.
	@LinkedServerName varchar(20) = 'FIS_DS', --Name of the linked DaFIS server; N/A in this case.
	@PartitionColumn char(1) = 0, --Number to use for partition column; N/A in this case.
	@IsDebug bit = 0 --Set to 1 just print the SQL and not actually execute it. 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
	DECLARE @TableName varchar(255) = SUBSTRING(@LoadTableName, 0, CHARINDEX('_Load',@LoadTableName ))
	DECLARE @TSQL varchar(MAX) = ''

    -- Insert statements for procedure here
	SELECT @TSQL = '
	
	-- First recreate table''s unique index:
	IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N''[dbo].[' + @TableName + ']'') AND name = N''' + @TableName + '_Id_UDX'')
	BEGIN
		CREATE UNIQUE NONCLUSTERED INDEX [' + @TableName + '_Id_UDX] ON [dbo].[' + @TableName + '] 
		(
			[Id] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	END
	
	-- Secondly recreate the full-text catalog if missing:
	IF NOT EXISTS (SELECT * FROM [PrePurchasing].[sys].[fulltext_catalogs] WHERE [name] LIKE ''' + @TableName + '%'')
		CREATE FULLTEXT CATALOG ' + @TableName +'_Name_SDX
	
	-- Lastly recreate the table''s full-text search index:
	IF NOT EXISTS (
		SELECT * FROM sys.objects O 
		INNER JOIN sys.fulltext_indexes FTI ON O.object_id = FTI.object_id
		WHERE O.object_id = OBJECT_ID(N''[dbo].[' + @TableName + ']'') AND O.type in (N''U'')
	)
	BEGIN
		CREATE FULLTEXT INDEX ON [dbo].[' + @TableName + ']
		([Id] LANGUAGE English, [Name] LANGUAGE English)
		KEY INDEX [' + @TableName + '_Id_UDX] ON (' + @TableName + '_Name_SDX, FILEGROUP [PRIMARY])
		WITH (STOPLIST = SYSTEM, CHANGE_TRACKING = AUTO);
	END
'
	
	-------------------------------------------------------------------------
	if @IsDebug = 1
		BEGIN
			--used for testing
			PRINT @TSQL	
		END
	else
		BEGIN
			--Execute the command:
			EXEC(@TSQL)
		END 
END
