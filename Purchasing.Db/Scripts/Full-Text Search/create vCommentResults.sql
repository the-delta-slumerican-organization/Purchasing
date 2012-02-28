USE [PrePurchasing]
GO

/****** Object:  View [dbo].[vCommentResults]    Script Date: 02/27/2012 10:36:17 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[dbo].[vCommentResults]'))
DROP VIEW [dbo].[vCommentResults]
GO

USE [PrePurchasing]
GO

/****** Object:  View [dbo].[vCommentResults]    Script Date: 02/27/2012 10:36:17 ******
Usage:
    
 USE [PrePurchasing]
 GO
 
 DECLARE @ContainsSearchCondition varchar(255) = 'handle care' 
 DECLARE @UserId varchar(255) = 'anlai' --'jsylvest'

 SELECT OC.[OrderId]
      ,OC.[RequestNumber]
      ,OC.[DateCreated]
      ,OC.[Text]
      ,OC.[CreatedBy]
  FROM [PrePurchasing].[dbo].[vCommentResults] OC
  INNER JOIN [PrePurchasing].[dbo].[vAccess] A ON OC.[OrderId] = A.[OrderId] 
  WHERE FREETEXT(OC.[text], @ContainsSearchCondition) AND A.[AccessUserId] = @UserId AND A.[isadmin] = 0 )

****************************************************************************************/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vCommentResults] WITH SCHEMABINDING
AS
SELECT     OC.Id, OC.OrderId, O.RequestNumber, OC.DateCreated, OC.Text, U.FirstName + ' ' + U.LastName AS CreatedBy
FROM         dbo.OrderComments AS OC INNER JOIN
                      dbo.Orders AS O ON OC.OrderId = O.Id INNER JOIN
                      dbo.Users AS U ON OC.UserId = U.Id

GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "OC"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 125
               Right = 198
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "O"
            Begin Extent = 
               Top = 6
               Left = 236
               Bottom = 125
               Right = 449
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "U"
            Begin Extent = 
               Top = 6
               Left = 487
               Bottom = 125
               Right = 647
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vCommentResults'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'vCommentResults'
GO


CREATE UNIQUE CLUSTERED INDEX [vCommentResults_Id_UDX] ON [dbo].[vCommentResults] 
(
[Id] ASC
)  
GO 