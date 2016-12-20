-- DROP TABLE IF EXISTS dbo.ImageData
Drop table dbo.ImageData

CREATE TABLE dbo.ImageData(
	Id int IDENTITY(1,1) PRIMARY KEY NOT NULL,
	MessageId int,
	ChatId bigint,
	UserId bigint,
	Descriptors varbinary(max),
	DateTimeAdded datetime2(0),
	UserFullName nvarchar(511),
	UserName nvarchar(32)
)

CREATE INDEX IX_ChatId ON dbo.ImageData (ChatId);