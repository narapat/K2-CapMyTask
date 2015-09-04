USE [K2]
GO

/****** Object:  View [dbo].[vwCapMyTask_TaskTypeList]    Script Date: 8/16/2015 4:04:33 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwCapMyTask_TaskTypeList]
AS
SELECT        dbo.Department.DepartmentName, dbo.Department.DepartmentManager, dbo.TaskType.TaskTypeName, dbo.TaskType.TaskTypeDescription
FROM            dbo.TaskTypeDepartment INNER JOIN
                         dbo.TaskType ON dbo.TaskTypeDepartment.TaskTypeID = dbo.TaskType.ID INNER JOIN
                         dbo.Department ON dbo.TaskTypeDepartment.DepartmentID = dbo.Department.DepartmentID

GO
