USE [${DatabaseName}]
GO

CREATE TABLE [dbo].[UpdateStatusReport] (
	[ID] bigint IDENTITY(1,1) NOT NULL,
	[Customer] NVARCHAR(50) NOT NULL,
	[ServerName] NVARCHAR(50) NOT NULL,
	[Timestamp] datetime2(0) NOT NULL,
	[KBArticleId] nvarchar(50) NOT NULL,
	[Status] bit NOT NULL,
	[Remarks] nvarchar(max),
	CONSTRAINT [PKC_UpdateStatusReport] PRIMARY KEY CLUSTERED ([ID])

)
ON [PRIMARY]