USE [NVOAMASIS]
GO

/****** Object:  Table [dbo].[FormBooking] ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[FormBooking](
	[id] [uniqueidentifier] NOT NULL,
	[Ngay] [datetime] NULL,
	[Sale] [nvarchar](100) NULL,
	[Line] [nvarchar](100) NULL,
	[Shipper] [uniqueidentifier] NULL,
	[Volume] [nvarchar](100) NULL,
	[POL_POD] [nvarchar](200) NULL,
	[ETD] [datetime] NULL,
	[FreeTime] [nvarchar](100) NULL,
	[PP_CC] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
	[Buy_Rate] [float] NULL,
	[Remark] [nvarchar](max) NULL,
	[Acc] [nvarchar](100) NULL,
	[Type_Bill] [nvarchar](500) NULL,
	[BookingId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_FormBooking] PRIMARY KEY CLUSTERED
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[FormBooking] ADD  CONSTRAINT [DF_FormBooking_id]  DEFAULT (newid()) FOR [id]
GO
