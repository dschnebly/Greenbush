USE [IndividualizedEducationProgram]
GO
/****** Object:  Table [dbo].[tblAccommodations]    Script Date: 1/15/2018 9:42:21 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET ANSI_PADDING ON
GO
CREATE TABLE [dbo].[tblAccommodations](
	[AccommodationID] [int] IDENTITY(1,1) NOT NULL,
	[IEPid] [int] NOT NULL,
	[Description] [nvarchar](500) NULL,
	[Location] [nchar](255) NULL,
	[Frequency] [int] NULL,
	[Duration] [int] NULL,
	[LocationCode] [char](1) NULL,
	[Create_Date] [datetime] NOT NULL CONSTRAINT [DF_tblAccommodations_Create_Date]  DEFAULT (getdate()),
	[Update_Date] [datetime] NULL CONSTRAINT [DF_tblAccommodations_Update_Date]  DEFAULT (getdate()),
	[AccomType] [int] NOT NULL,
	[AnticipatedStartDate] [datetime] NULL,
	[AnticipatedEndDate] [datetime] NULL,
 CONSTRAINT [PK_tblAccommodations] PRIMARY KEY CLUSTERED 
(
	[AccommodationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
SET ANSI_PADDING OFF
GO
