USE [PrePurchasing]
GO
/****** Object:  UserDefinedFunction [dbo].[udf_GetBuildingResults]    Script Date: 02/27/2012 09:57:37 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ken Taylor
-- Create date: March 13, 2012
-- Description:	Given an UserId (Kerberos) and ContainsSearchCondition search string, 
--				return the non-admin records matching the search string that the user can see
-- Usage:
-- USE [PrePurchasing]
-- GO
-- 
-- DECLARE @ContainsSearchCondition varchar(255) = '3209 test' –-Searches on either BuildingCode or BuildingName.
-- SELECT * from udf_GetBuildingResults(@ContainsSearchCondition)
-- 
-- results:
-- Id		CampusCode	BuildingCode	CampusName		CampusShortName	CampusTypeCode	BuildingName								LastUpdateDate			IsActive
-- 3-8187	3			8187			Davis Campus	Davis Campus	B				CAHFS SAN BERNARDINO DAIRY TESTING FACIL	2012-03-12 22:39:29.000	1
-- 3-3209	3			3209			Davis Campus	Davis Campus	B				GROUNDS TOOL HOUSE							2012-03-12 22:39:29.000	1
--
-- Modifications:
-- =============================================
ALTER FUNCTION [dbo].[udf_GetBuildingResults] 
(	
	-- Add the parameters for the function here
	@ContainsSearchCondition varchar(255) --A string containing the word or words to search on.
)
RETURNS @returntable TABLE 
(
	 Id varchar(10) not null
	,CampusCode varchar(2) not null
	,BuildingCode varchar(4) not null
	,CampusName varchar(40) null
	,CampusShortName varchar(12) null
	,CampusTypeCode varchar(1) null
	,BuildingName varchar(80) null
	,LastUpdateDate datetime null
	,IsActive bit null
)
AS
BEGIN
	INSERT INTO @returntable
	SELECT TOP 100 PERCENT [Id]
      ,[CampusCode]
      ,[BuildingCode]
      ,[CampusName]
      ,[CampusShortName]
      ,[CampusTypeCode]
      ,[BuildingName]
      ,[LastUpdateDate]
      ,[IsActive]
  FROM [PrePurchasing].[dbo].[vBuildings] SEARCH_TBL
  INNER JOIN FREETEXTTABLE(vBuildings, ([BuildingCode], [BuildingName]), @ContainsSearchCondition) KEY_TBL on SEARCH_TBL.Id = KEY_TBL.[KEY]
  ORDER BY KEY_TBL.[RANK] DESC

RETURN
END
