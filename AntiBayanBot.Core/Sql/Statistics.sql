DROP TABLE IF EXISTS dbo.[Statistics]

CREATE TABLE dbo.[Statistics] (
	ChatId bigint NOT NULL,
	UserId bigint NOT NULL,
	UserFullName nvarchar(511) NOT NULL,
	UserName nvarchar(32),
	Bayans int NOT NULL,

	-- Composite key
    PRIMARY KEY (ChatId, UserId)
)

CREATE INDEX IX_Bayans ON dbo.[Statistics] (Bayans);