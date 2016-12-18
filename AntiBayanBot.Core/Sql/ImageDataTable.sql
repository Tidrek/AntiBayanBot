CREATE TABLE dbo.ImageData(
	Id int IDENTITY(1,1) PRIMARY KEY NOT NULL,
	ChatId int,
	UserId int,
	Features nvarchar(max),
	DateTimeAdded datetime2(0)
)

CREATE INDEX IX_ChatId ON dbo.ImageData (ChatId);