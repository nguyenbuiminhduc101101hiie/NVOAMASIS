USE [LMS]
GO

/****** Object:  Table [dbo].[LeaveRequests]    Script Date: 15/01/2026 9:42:56 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[LeaveRequests](
	[Id] [uniqueidentifier] NOT NULL,
	[EmployeeId] [uniqueidentifier] NOT NULL,
	[LeaveType] [int] NOT NULL,
	[DurationType] [int] NOT NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[StartTime] [time](7) NULL,
	[EndTime] [time](7) NULL,
	[Reason] [nvarchar](1000) NOT NULL,
	[Status] [int] NOT NULL,
	[ApproverId] [uniqueidentifier] NULL,
	[ApprovedDate] [datetime] NULL,
	[ApproverComment] [nvarchar](500) NULL,
	[CreatedDate] [datetime] NOT NULL,
	[UpdatedDate] [datetime] NULL,
	[TotalDays] [decimal](18, 2) NOT NULL,
	[TotalHours] [decimal](18, 2) NULL,
 CONSTRAINT [PK_LeaveRequests] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[LeaveRequests] ADD  DEFAULT (newid()) FOR [Id]
GO

ALTER TABLE [dbo].[LeaveRequests] ADD  DEFAULT ((1)) FOR [Status]
GO

ALTER TABLE [dbo].[LeaveRequests] ADD  DEFAULT (getdate()) FOR [CreatedDate]
GO

ALTER TABLE [dbo].[LeaveRequests] ADD  DEFAULT ((0)) FOR [TotalDays]
GO

ALTER TABLE [dbo].[LeaveRequests] ADD  DEFAULT ((0)) FOR [TotalHours]
GO

ALTER TABLE [dbo].[LeaveRequests]  WITH CHECK ADD  CONSTRAINT [FK_LeaveRequests_Approver] FOREIGN KEY([ApproverId])
REFERENCES [dbo].[UserList] ([UsrId])
GO

ALTER TABLE [dbo].[LeaveRequests] CHECK CONSTRAINT [FK_LeaveRequests_Approver]
GO

ALTER TABLE [dbo].[LeaveRequests]  WITH CHECK ADD  CONSTRAINT [FK_LeaveRequests_Employee] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[UserList] ([UsrId])
GO

ALTER TABLE [dbo].[LeaveRequests] CHECK CONSTRAINT [FK_LeaveRequests_Employee]
GO


