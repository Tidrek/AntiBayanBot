-- DROP TABLE IF EXISTS dbo.ImageData
DROP TABLE dbo.ImageData

CREATE TABLE dbo.ImageData (
	Id int IDENTITY(1,1) PRIMARY KEY NOT NULL,
	MessageId int NOT NULL,
	ChatId bigint NOT NULL,
	UserId bigint NOT NULL,
	Descriptors varbinary(max) NOT NULL,
	DateTimeAdded datetime2(0) NOT NULL,
	UserFullName nvarchar(511) NOT NULL,
	UserName nvarchar(32)
)

CREATE INDEX IX_ChatId ON dbo.ImageData (ChatId);