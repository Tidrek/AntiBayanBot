DROP TABLE IF EXISTS dbo.[Statistics]

CREATE TABLE dbo.[Statistics] (
	ChatId bigint NOT NULL,
	UserId bigint NOT NULL,
	Bayans int NOT NULL,

	-- Composite key
    PRIMARY KEY (ChatId, UserId)
)